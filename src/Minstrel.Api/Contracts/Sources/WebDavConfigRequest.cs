namespace Minstrel.Api.Contracts.Sources;

public record WebDavConfigRequest(
    string? ServerUrl,
    string? Username,
    string? Password,
    string? MusicFolderPath
);
