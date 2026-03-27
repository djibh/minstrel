namespace Minstrel.Api.Contacts.Albums;

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