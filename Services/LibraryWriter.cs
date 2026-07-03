using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.YouTubeFast.Configuration;
using Jellyfin.Plugin.YouTubeFast.YouTube;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.YouTubeFast.Services;

/// <summary>
/// Turns API results into a flat on-disk layout:
///
///   {User}/{Channel}/
///       poster.jpg
///       chaine.nfo            (channel metadata)
///       .ytchannel           (cleanup marker, written by us)
///       {Video Title}/
///           {Video Title}.strm
///           {Video Title}.nfo
///           {Video Title}.jpg
///           .ytmeta           (dedup / age-cleanup marker)
///
/// There is no "Season YYYY" level: every video folder lives directly under
/// its channel folder.
/// </summary>
public class LibraryWriter
{
    /// <summary>Marker file dropped at every channel root we create, used to
    /// safely identify (and clean up) folders this plugin owns.</summary>
    public const string ChannelMarker = ".ytchannel";

    private const string VideoMarker = ".ytmeta";

    private readonly HttpClient _http;
    private readonly ILogger _logger;

    public LibraryWriter(HttpClient http, ILogger logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Rewrites just the .nfo (and re-downloads the thumbnail if missing) for a
    /// video folder that already exists. Used to backfill metadata (e.g. the
    /// description) onto videos written before that data was captured. No-op if
    /// the folder isn't there yet.
    /// </summary>
    public async Task RefreshVideoNfoAsync(
        string sourceRoot,
        SourceItem source,
        YouTubeVideo video,
        int episodeNumber,
        CancellationToken ct)
    {
        var safeTitle = Sanitize(video.Title);
        var videoDir = Path.Combine(sourceRoot, safeTitle);
        if (!Directory.Exists(videoDir))
        {
            return;
        }

        var isSeries = !string.Equals(source.Mode, "Movies", StringComparison.OrdinalIgnoreCase);
        var nfo = isSeries
            ? BuildEpisodeNfo(video, video.PublishedAt.Year, episodeNumber)
            : BuildMovieNfo(video);
        await File.WriteAllTextAsync(Path.Combine(videoDir, safeTitle + ".nfo"), nfo, ct).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(video.ThumbnailUrl))
        {
            await DownloadAsync(video.ThumbnailUrl, Path.Combine(videoDir, safeTitle + ".jpg"), ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Deletes a previously-written video folder if it exists. Used to remove
    /// Shorts that were imported before the source started excluding them.
    /// Returns true if something was deleted.
    /// </summary>
    public bool RemoveVideoIfExists(string sourceRoot, SourceItem source, YouTubeVideo video)
    {
        var safeTitle = Sanitize(video.Title);
        var videoDir = Path.Combine(sourceRoot, safeTitle);

        try
        {
            if (Directory.Exists(videoDir))
            {
                Directory.Delete(videoDir, recursive: true);
                _logger.LogInformation("Removed Short folder: {Dir}", videoDir);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove Short folder {Dir}", videoDir);
        }

        return false;
    }

    /// <summary>
    /// Writes the channel folder: chaine.nfo + poster.jpg + the .ytchannel
    /// marker. Called for every source (series and movies modes alike).
    /// </summary>
    public async Task WriteChannelRootAsync(
        string sourceRoot,
        string title,
        string description,
        string? thumbUrl,
        bool isSeries,
        CancellationToken ct)
    {
        Directory.CreateDirectory(sourceRoot);

        var rootTag = isSeries ? "tvshow" : "movie";
        var nfo = new StringBuilder();
        nfo.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        nfo.AppendLine($"<{rootTag}>");
        nfo.AppendLine($"  <title>{Esc(title)}</title>");
        nfo.AppendLine($"  <plot>{Esc(description)}</plot>");
        nfo.AppendLine($"</{rootTag}>");
        await File.WriteAllTextAsync(Path.Combine(sourceRoot, "chaine.nfo"), nfo.ToString(), ct).ConfigureAwait(false);

        // Ownership marker so cleanup only ever touches folders we created.
        await File.WriteAllTextAsync(
            Path.Combine(sourceRoot, ChannelMarker),
            title,
            ct).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(thumbUrl))
        {
            await DownloadAsync(thumbUrl, Path.Combine(sourceRoot, "poster.jpg"), ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Writes a single video into {sourceRoot}/{Title}/. Returns false if it
    /// already existed. strmTarget is the URL written into the .strm file.
    /// </summary>
    public async Task<bool> WriteVideoAsync(
        string sourceRoot,
        SourceItem source,
        YouTubeVideo video,
        int episodeNumber,
        string strmTarget,
        CancellationToken ct)
    {
        var safeTitle = Sanitize(video.Title);
        var videoDir = Path.Combine(sourceRoot, safeTitle);
        var isSeries = !string.Equals(source.Mode, "Movies", StringComparison.OrdinalIgnoreCase);

        var strmPath = Path.Combine(videoDir, safeTitle + ".strm");
        var markerPath = Path.Combine(videoDir, VideoMarker);

        if (File.Exists(strmPath) && File.Exists(markerPath))
        {
            return false; // already synced
        }

        Directory.CreateDirectory(videoDir);

        await File.WriteAllTextAsync(strmPath, strmTarget, ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            markerPath,
            $"{video.VideoId}|{video.PublishedAt.ToString("o", CultureInfo.InvariantCulture)}",
            ct).ConfigureAwait(false);

        // NFO ({Title}.nfo)
        var nfoPath = Path.Combine(videoDir, safeTitle + ".nfo");
        var nfo = isSeries
            ? BuildEpisodeNfo(video, video.PublishedAt.Year, episodeNumber)
            : BuildMovieNfo(video);
        await File.WriteAllTextAsync(nfoPath, nfo, ct).ConfigureAwait(false);

        // Thumbnail ({Title}.jpg)
        if (!string.IsNullOrEmpty(video.ThumbnailUrl))
        {
            await DownloadAsync(video.ThumbnailUrl, Path.Combine(videoDir, safeTitle + ".jpg"), ct).ConfigureAwait(false);
        }

        return true;
    }

    /// <summary>
    /// Deletes video folders whose stored publish date is older than the cutoff.
    /// Walks the source root recursively looking for .ytmeta markers.
    /// </summary>
    public void CleanupOldVideos(string sourceRoot, DateTime cutoffUtc)
    {
        if (!Directory.Exists(sourceRoot))
        {
            return;
        }

        foreach (var marker in Directory.EnumerateFiles(sourceRoot, VideoMarker, SearchOption.AllDirectories))
        {
            try
            {
                var content = File.ReadAllText(marker);
                var parts = content.Split('|');
                if (parts.Length < 2)
                {
                    continue;
                }

                if (DateTime.TryParse(
                        parts[1],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out var published)
                    && published < cutoffUtc)
                {
                    var dir = Path.GetDirectoryName(marker);
                    if (dir is not null && Directory.Exists(dir))
                    {
                        Directory.Delete(dir, recursive: true);
                        _logger.LogInformation("Removed expired video folder: {Dir}", dir);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cleanup failed for marker {Marker}", marker);
            }
        }
    }

    /// <summary>
    /// Removes video folders under <paramref name="channelRoot"/> whose stored
    /// video id is not in <paramref name="keepVideoIds"/>. Used to delete
    /// individually-added videos once a user removes them from their list.
    /// </summary>
    public void CleanupRemovedVideos(string channelRoot, ISet<string> keepVideoIds)
    {
        if (!Directory.Exists(channelRoot))
        {
            return;
        }

        foreach (var marker in Directory.EnumerateFiles(channelRoot, VideoMarker, SearchOption.AllDirectories))
        {
            try
            {
                var parts = File.ReadAllText(marker).Split('|');
                var id = parts.Length > 0 ? parts[0] : string.Empty;
                if (!string.IsNullOrEmpty(id) && !keepVideoIds.Contains(id))
                {
                    var dir = Path.GetDirectoryName(marker);
                    if (dir is not null && Directory.Exists(dir))
                    {
                        Directory.Delete(dir, recursive: true);
                        _logger.LogInformation("Removed video no longer in the list: {Dir}", dir);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Video cleanup failed for marker {Marker}", marker);
            }
        }
    }

    /// <summary>
    /// Removes channel folders that this plugin created (identified by the
    /// .ytchannel marker) but that are no longer in <paramref name="expectedRoots"/>.
    /// This is what deletes a channel's files once a user removes it from their
    /// list. Only ever touches marked folders, so unrelated library content is
    /// safe. <paramref name="scanRoots"/> are the parent folders to look under
    /// (e.g. each per-user folder + the global library folder).
    /// </summary>
    public void CleanupRemovedChannels(IEnumerable<string> expectedRoots, IEnumerable<string> scanRoots)
    {
        var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in expectedRoots)
        {
            expected.Add(Normalize(r));
        }

        var scannedParents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var scanRoot in scanRoots)
        {
            if (string.IsNullOrWhiteSpace(scanRoot) || !Directory.Exists(scanRoot))
            {
                continue;
            }

            if (!scannedParents.Add(Normalize(scanRoot)))
            {
                continue; // already handled this parent
            }

            // Find every channel folder we own underneath this parent.
            foreach (var marker in Directory.EnumerateFiles(scanRoot, ChannelMarker, SearchOption.AllDirectories))
            {
                var channelDir = Path.GetDirectoryName(marker);
                if (channelDir is null)
                {
                    continue;
                }

                if (expected.Contains(Normalize(channelDir)))
                {
                    continue; // still configured -> keep
                }

                try
                {
                    Directory.Delete(channelDir, recursive: true);
                    _logger.LogInformation("Removed channel no longer in the list: {Dir}", channelDir);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to remove orphaned channel folder {Dir}", channelDir);
                }
            }

            RemoveEmptyDirectories(scanRoot);
        }
    }

    private void RemoveEmptyDirectories(string root)
    {
        foreach (var dir in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
        {
            try
            {
                if (Directory.Exists(dir) && Directory.GetFileSystemEntries(dir).Length == 0)
                {
                    Directory.Delete(dir);
                }
            }
            catch
            {
                // ignore
            }
        }
    }

    private static string Normalize(string path) =>
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private static string BuildEpisodeNfo(YouTubeVideo v, int season, int episode)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<episodedetails>");
        sb.AppendLine($"  <title>{Esc(v.Title)}</title>");
        sb.AppendLine($"  <season>{season.ToString(CultureInfo.InvariantCulture)}</season>");
        sb.AppendLine($"  <episode>{episode.ToString(CultureInfo.InvariantCulture)}</episode>");
        sb.AppendLine($"  <plot>{Esc(v.Description)}</plot>");
        sb.AppendLine($"  <aired>{v.PublishedAt:yyyy-MM-dd}</aired>");
        sb.AppendLine($"  <premiered>{v.PublishedAt:yyyy-MM-dd}</premiered>");
        sb.AppendLine("</episodedetails>");
        return sb.ToString();
    }

    private static string BuildMovieNfo(YouTubeVideo v)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<movie>");
        sb.AppendLine($"  <title>{Esc(v.Title)}</title>");
        sb.AppendLine($"  <plot>{Esc(v.Description)}</plot>");
        sb.AppendLine($"  <year>{v.PublishedAt.Year.ToString(CultureInfo.InvariantCulture)}</year>");
        sb.AppendLine($"  <premiered>{v.PublishedAt:yyyy-MM-dd}</premiered>");
        sb.AppendLine("</movie>");
        return sb.ToString();
    }

    private async Task DownloadAsync(string url, string destination, CancellationToken ct)
    {
        try
        {
            if (File.Exists(destination))
            {
                return;
            }

            var bytes = await _http.GetByteArrayAsync(url, ct).ConfigureAwait(false);
            await File.WriteAllBytesAsync(destination, bytes, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Thumbnail download failed: {Url}", url);
        }
    }

    private static string Esc(string s) =>
        s.Replace("&", "&amp;")
         .Replace("<", "&lt;")
         .Replace(">", "&gt;");

    private static string Sanitize(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        name = name.Replace(":", "_").Replace("/", "_").Replace("\\", "_").Trim();
        if (name.Length > 120)
        {
            name = name.Substring(0, 120).Trim();
        }

        return string.IsNullOrWhiteSpace(name) ? "video" : name;
    }
}
