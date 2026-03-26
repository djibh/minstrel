using Minstrel.Domain.Enums;

namespace Minstrel.Domain.Entities;

public class MediaSource
{
    public required string Id { get; init; }
    public required SourceKind Kind { get; init; }
    public required string DisplayName { get; init; }
    public bool IsEnabled { get; init; }
    public SourceSyncStatus SyncStatus { get; init; }
}
