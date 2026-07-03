using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.YouTubeFast.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.YouTubeFast.Services;

/// <summary>
/// Resolves a YouTube video id to a directly playable stream URL using yt-dlp,
/// caching the result for a short window (YouTube URLs are time-limited anyway).
/// </summary>
public class PlaybackResolver
{
    private static readonly ConcurrentDictionary<string, CacheEntry> Cache = new();

    private readonly ILogger _logger;

    public PlaybackResolver(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<string?> ResolveAsync(string videoId, CancellationToken ct)
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();

        if (Cache.TryGetValue(videoId, out var cached) && cached.ExpiresUtc > DateTime.UtcNow)
        {
            return cached.Url;
        }

        // Always aim for the best-quality HLS: the master manifest (adaptive
        // bitrate) first, then a single best HLS stream. Native players handle
        // HLS well; browsers are best directed to a native app. A combined mp4
        // remains only as a deep last resort so a video that exposes no HLS at
        // all still plays something.
        var url = await RunYtDlpAsync(config, videoId, ResolveMode.HlsManifest, ct).ConfigureAwait(false);

        if (string.IsNullOrEmpty(url))
        {
            url = await RunYtDlpAsync(config, videoId, ResolveMode.Combined, ct).ConfigureAwait(false);
        }

        if (!string.IsNullOrEmpty(url))
        {
            Cache[videoId] = new CacheEntry
            {
                Url = url!,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(Math.Max(1, config.LinkCacheMinutes))
            };
        }

        return url;
    }

    private enum ResolveMode
    {
        /// <summary>Print the HLS master manifest URL (multivariant playlist).</summary>
        HlsManifest,

        /// <summary>Resolve the best HLS stream (with a combined-mp4 last resort).</summary>
        Combined
    }

    private async Task<string?> RunYtDlpAsync(PluginConfiguration config, string videoId, ResolveMode mode, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = config.YtDlpPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (mode == ResolveMode.HlsManifest)
        {
            // Select an HLS format, then print ITS master manifest URL (shared by
            // all variants) rather than a single variant. The browser then does
            // adaptive bitrate up to the best H.264 variant on its own.
            psi.ArgumentList.Add("--print");
            psi.ArgumentList.Add("%(manifest_url)s");
            psi.ArgumentList.Add("-f");
            psi.ArgumentList.Add("b[protocol^=m3u8]/bv*[protocol^=m3u8]");
        }
        else
        {
            // -g prints the resolved media URL(s). We aim for a single playable
            // URL (Jellyfin then handles transcoding / HW acceleration itself).
            psi.ArgumentList.Add("-g");
            psi.ArgumentList.Add("-f");
            psi.ArgumentList.Add(BuildFormat(config));
        }

        // NOTE: we deliberately do NOT force the iOS player client. YouTube now
        // gates its iOS HTTPS formats behind a GVS PO token, so those formats get
        // skipped and resolution fails. The default client already exposes a
        // single combined HLS stream (up to 1080p+) that resolves cleanly.

        // --- Speed flags ---
        // Never expand playlists, keep output terse, and fail fast on stalls.
        psi.ArgumentList.Add("--no-playlist");
        psi.ArgumentList.Add("--no-warnings");
        psi.ArgumentList.Add("--socket-timeout");
        psi.ArgumentList.Add("15");

        // Persistent cache dir: yt-dlp reuses the solved player JS / nsig between
        // calls instead of re-extracting it each cold start (big win, esp. in
        // Docker where the service user's home may not be writable).
        try
        {
            var cacheDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "jellyfin-ytdlp-cache");
            System.IO.Directory.CreateDirectory(cacheDir);
            psi.ArgumentList.Add("--cache-dir");
            psi.ArgumentList.Add(cacheDir);
        }
        catch
        {
            // ignore - fall back to yt-dlp's default cache location
        }

        // Fast mode: skip downloading the full watch HTML page (~1 MB) and rely
        // on the lighter innertube API. Disable if a video fails to resolve.
        if (config.FastResolve)
        {
            psi.ArgumentList.Add("--extractor-args");
            psi.ArgumentList.Add("youtube:player_skip=webpage,configs");
        }

        // Any advanced extra args the user configured.
        if (!string.IsNullOrWhiteSpace(config.YtDlpExtraArgs))
        {
            foreach (var arg in config.YtDlpExtraArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                psi.ArgumentList.Add(arg);
            }
        }

        psi.ArgumentList.Add($"https://www.youtube.com/watch?v={videoId}");

        try
        {
            using var process = new Process { StartInfo = psi };
            process.Start();

            var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
            var stderrTask = process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct).ConfigureAwait(false);

            var stdout = await stdoutTask.ConfigureAwait(false);
            var stderr = await stderrTask.ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                _logger.LogWarning("yt-dlp ({Mode}) failed for {VideoId}: {Error}",
                    mode, videoId, stderr.Trim());
                return null;
            }

            // First non-empty http(s) line is the playable URL / manifest.
            foreach (var line in stdout.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch yt-dlp at '{Path}'", config.YtDlpPath);
            return null;
        }
    }

    /// <summary>
    /// Builds a yt-dlp format selector that resolves to ONE playable URL,
    /// honouring MaxHeight. Honours the user's manual override.
    /// </summary>
    private static string BuildFormat(PluginConfiguration config)
    {
        var h = config.MaxHeight > 0 ? config.MaxHeight : 1080;
        var hh = h.ToString(System.Globalization.CultureInfo.InvariantCulture);

        // Honour a manual override if the user set one.
        if (!string.IsNullOrWhiteSpace(config.YtDlpFormatOverride))
        {
            return config.YtDlpFormatOverride.Trim();
        }

        // HLS first, in preference order: H.264 HLS <=H -> any HLS <=H -> any
        // HLS. A combined mp4 (<=H, then itag 18) is kept ONLY as a deep last
        // resort so a video that exposes no HLS at all still plays something.
        return $"b[protocol^=m3u8][vcodec^=avc1][height<=?{hh}]/b[protocol^=m3u8][height<=?{hh}]/b[protocol^=m3u8]/" +
               $"best[ext=mp4][vcodec!=none][acodec!=none][height<=?{hh}]/18";
    }

    private sealed class CacheEntry
    {
        public string Url { get; set; } = string.Empty;
        public DateTime ExpiresUtc { get; set; }
    }
}
