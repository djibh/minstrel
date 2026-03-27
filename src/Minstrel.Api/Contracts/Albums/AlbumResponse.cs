namespace Minstrel.Api.Contracts.Albums;

public record AlbumResponse
(
    string Id,
    string SourceId,
    string SourceKind,
    string Title,
    string ArtistName,
    int? Year,
    int TrackCount,
    string? CoverUrl,
    bool IsOfflineAvailable
);