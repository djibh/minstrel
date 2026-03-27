namespace Minstrel.Api.Contracts.Artists;

public record ArtistResponse(
    string Id,
    string SourceId,
    string SourceKind,
    string Name,
    string? ImageUrl,
    int AlbumCount,
    int TrackCount
);