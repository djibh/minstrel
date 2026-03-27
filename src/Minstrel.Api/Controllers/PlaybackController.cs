using Microsoft.AspNetCore.Mvc;
using Minstrel.Application.Playback.Services;

namespace Minstrel.Api.Controllers;

[ApiController]
[Route("playback")]
public class PlaybackController : ControllerBase
{
    private readonly PlaybackService _playbackService;

    public PlaybackController(PlaybackService playbackService)
    {
        _playbackService = playbackService;
    }

    [HttpGet("tracks/{trackId}/stream")]
    public async Task<IActionResult> StreamTrack(string trackId, CancellationToken cancellationToken)
    {
        var descriptor = await _playbackService.GetTrackStreamAsync(trackId, cancellationToken);

        if (descriptor is null)
        {
            return NotFound();
        }

        if (descriptor.IsRedirectPreferred)
        {
            return Redirect(descriptor.StreamUri.ToString());
        }

        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
