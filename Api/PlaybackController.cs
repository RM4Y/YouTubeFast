using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.YouTubeFast.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.YouTubeFast.Api;

/// <summary>
/// Resolver endpoint. The .strm files point here; when a client plays one,
/// we ask yt-dlp for the real HLS stream URL and 302-redirect to it.
/// </summary>
[ApiController]
[Route("YouTubeFast")]
public class PlaybackController : ControllerBase
{
    private readonly ILogger<PlaybackController> _logger;

    public PlaybackController(ILogger<PlaybackController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// GET /YouTubeFast/Stream/{videoId}
    ///
    /// AllowAnonymous so the server's own transcoder/clients can fetch the
    /// stream without juggling an auth token inside the .strm. Keep the endpoint
    /// on a trusted network.
    /// </summary>
    [HttpGet("Stream/{videoId}")]
    [AllowAnonymous]
    public async Task<ActionResult> Stream([FromRoute] string videoId, CancellationToken ct)
    {
        var resolver = new PlaybackResolver(_logger);
        var url = await resolver.ResolveAsync(videoId, ct).ConfigureAwait(false);

        if (string.IsNullOrEmpty(url))
        {
            return NotFound($"Could not resolve video {videoId}");
        }

        // Redirect; ffmpeg/clients follow the 302 to the resolved stream URL.
        return Redirect(url!);
    }
}
