using System;
using Minstrel.Api.Contracts.Albums;
using Minstrel.Api.Contracts.Artists;
using Minstrel.Api.Contracts.Playlists;
using Minstrel.Api.Contracts.Search;
using Minstrel.Api.Contracts.Sources;
using Minstrel.Api.Contracts.Tracks;
using Minstrel.Domain.Entities;

namespace Minstrel.Api.Mapping;

public static class ApiContractMapper
{
    public static AlbumResponse ToResponse(this Album item) =>
        new(
            item.Id,
            item.SourceId,
            item.SourceKind.ToString().ToLowerInvariant(),
            item.Title,
            item.ArtistName,
            item.Year,
            item.TrackCount,
            item.CoverUrl,
            item.IsOfflineAvailable);

    public static ArtistResponse ToResponse(this Artist item) =>
        new(
            item.Id,
            item.SourceId,
            item.SourceKind.ToString().ToLowerInvariant(),
            item.Name,
            item.ImageUrl,
            item.AlbumCount,
            item.TrackCount);

    public static TrackResponse ToResponse(this Track item) =>
        new(
            item.Id,
            item.SourceId,
            item.SourceKind.ToString().ToLowerInvariant(),
            item.Title,
            item.ArtistName,
            item.AlbumTitle,
            item.DurationSeconds,
            item.CoverUrl,
            item.IsOfflineAvailable);

    public static PlaylistResponse ToResponse(this Playlist item) =>
        new(
            item.Id,
            item.SourceId,
            item.SourceKind.ToString().ToLowerInvariant(),
            item.Name,
            item.CoverUrl,
            item.TrackCount,
            item.IsOfflineAvailable);

    public static SourceResponse ToResponse(this MediaSource item) =>
        new(
            item.Id,
            item.Kind.ToString().ToLowerInvariant(),
            item.DisplayName,
            item.IsEnabled,
            item.SyncStatus.ToString().ToLowerInvariant());

    public static SearchResultsResponse ToResponse(this SearchResults item) =>
        new(
            item.Query,
            item.Tracks.Select(ToResponse).ToList(),
            item.Albums.Select(ToResponse).ToList(),
            item.Artists.Select(ToResponse).ToList(),
            item.Playlists.Select(ToResponse).ToList());
}
