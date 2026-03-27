namespace Minstrel.Api.Contracts.Playlists;

public record PlaylistResponse(
    string Id,
    string SourceId,
    string SourceKind,
    string Name,
    string? CoverUrl,
    int TrackCount,
    bool IsOfflineAvailable
);
