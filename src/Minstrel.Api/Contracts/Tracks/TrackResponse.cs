namespace Minstrel.Api.Contracts.Tracks;

public record TrackResponse(
    string Id,
    string SourceId,
    string SourceKind,
    string Title,
    string ArtistName,
    string AlbumTitle,
    int? DurationSeconds,
    string? CoverUrl,
    bool IsOfflineAvailable
);
