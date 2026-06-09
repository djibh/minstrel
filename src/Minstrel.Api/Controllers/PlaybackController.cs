using Microsoft.AspNetCore.Mvc;
using Minstrel.Application.Playback.Services;

namespace Minstrel.Api.Controllers;

[ApiController]
[Route("playback")]
public class PlaybackController : ControllerBase
{
    private readonly PlaybackService _playbackService;
    private readonly IHttpClientFactory _httpClientFactory;

    public PlaybackController(PlaybackService playbackService, IHttpClientFactory httpClientFactory)
    {
        _playbackService = playbackService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("tracks/{trackId}/stream")]
    public async Task<IActionResult> StreamTrack(string trackId, CancellationToken cancellationToken)
    {
        var descriptor = await _playbackService.GetTrackStreamAsync(trackId, cancellationToken);

        if (descriptor is null)
            return NotFound();

        if (descriptor.IsRedirectPreferred)
            return Redirect(descriptor.StreamUri.ToString());

        var request = new HttpRequestMessage(HttpMethod.Get, descriptor.StreamUri);
        if (descriptor.ProxyHeaders is not null)
            foreach (var (key, value) in descriptor.ProxyHeaders)
                request.Headers.TryAddWithoutValidation(key, value);

        var upstream = await _httpClientFactory.CreateClient()
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!upstream.IsSuccessStatusCode)
            return StatusCode((int)upstream.StatusCode);

        var contentType = upstream.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        var stream = await upstream.Content.ReadAsStreamAsync(cancellationToken);
        return File(stream, contentType, enableRangeProcessing: true);
    }
}
