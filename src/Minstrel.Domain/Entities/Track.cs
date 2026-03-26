using Minstrel.Domain.Enums;

namespace Minstrel.Domain.Entities;

public class Track
{
    public required string Id { get; init; }
    public required string SourceId { get; init; }
    public required SourceKind SourceKind { get; init; }

    public required string Title { get; init; }
    public required string ArtistName { get; init; }
    public required string AlbumTitle { get; init; }

    public int? DurationSeconds { get; init; }
    public string? CoverUrl { get; init; }
    public bool IsOfflineAvailable { get; init; }
}
