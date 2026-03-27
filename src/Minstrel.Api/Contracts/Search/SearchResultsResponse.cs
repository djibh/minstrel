using Minstrel.Api.Contracts.Albums;
using Minstrel.Api.Contracts.Artists;
using Minstrel.Api.Contracts.Playlists;
using Minstrel.Api.Contracts.Tracks;

namespace Minstrel.Api.Contracts.Search;

public record SearchResultsResponse(
    string Query,
    IReadOnlyCollection<TrackResponse> Tracks,
    IReadOnlyCollection<AlbumResponse> Albums,
    IReadOnlyCollection<ArtistResponse> Artists,
    IReadOnlyCollection<PlaylistResponse> Playlists
);
