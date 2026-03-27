using System;
using Microsoft.AspNetCore.Mvc;
using Minstrel.Api.Contracts.Albums;
using Minstrel.Api.Contracts.Artists;
using Minstrel.Api.Contracts.Playlists;
using Minstrel.Api.Contracts.Tracks;
using Minstrel.Api.Mapping;
using Minstrel.Application.Librairy.Services;

namespace Minstrel.Api.Controllers;

[ApiController]
[Route("library")]
public class LibraryController : ControllerBase
{
    private readonly LibraryAggregationService _libraryService;

    public LibraryController(LibraryAggregationService libraryService)
    {
        _libraryService = libraryService;
    }

    [HttpGet("albums")]
    public async Task<ActionResult<IReadOnlyCollection<AlbumResponse>>> GetAlbums(
        [FromQuery] string? source,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        var items = await _libraryService.GetAlbumsAsync(source, sort, cancellationToken);
        return Ok(items.Select(x => x.ToResponse()).ToList());
    }

    [HttpGet("artists")]
    public async Task<ActionResult<IReadOnlyCollection<ArtistResponse>>> GetArtists(
        [FromQuery] string? source,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        var items = await _libraryService.GetArtistsAsync(source, sort, cancellationToken);
        return Ok(items.Select(x => x.ToResponse()).ToList());
    }

    [HttpGet("tracks")]
    public async Task<ActionResult<IReadOnlyCollection<TrackResponse>>> GetTracks(
        [FromQuery] string? source,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        var items = await _libraryService.GetTracksAsync(source, sort, cancellationToken);
        return Ok(items.Select(x => x.ToResponse()).ToList());
    }

    [HttpGet("playlists")]
    public async Task<ActionResult<IReadOnlyCollection<PlaylistResponse>>> GetPlaylists(
        [FromQuery] string? source,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        var items = await _libraryService.GetPlaylistsAsync(source, sort, cancellationToken);
        return Ok(items.Select(x => x.ToResponse()).ToList());
    }
}
