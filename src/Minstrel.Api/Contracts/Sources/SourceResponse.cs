namespace Minstrel.Api.Contracts.Sources;

public record SourceResponse(
    string Id,
    string Kind,
    string DisplayName,
    bool IsEnabled,
    string SyncStatus
);
