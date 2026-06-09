namespace Minstrel.Api.Contracts.Sources;

public record PCloudConfigRequest(
    string? Email,
    string? Password,
    string? MusicFolderPath,
    string? ApiBaseUrl,
    string? VerificationCode
);
