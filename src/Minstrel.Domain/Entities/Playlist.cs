using Minstrel.Domain.Enums;

namespace Minstrel.Domain.Entities;

public class Playlist
{
    public required string Id { get; init; }
    public required string SourceId { get; init; }
    public required SourceKind SourceKind { get; init; }

    public required string Name { get; init; }
    public string? CoverUrl { get; init; }
    public int TrackCount { get; init; }
    public bool IsOfflineAvailable { get; init; }
}
