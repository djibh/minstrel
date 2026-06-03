namespace Minstrel.Api.Contracts.Sources;

public record PCloudConnectRequest(string Email, string Password, string? Code = null);
