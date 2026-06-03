namespace Minstrel.Api.Contracts.Sources;

public record PCloudSetTokenRequest(string Token, string ApiBaseUrl = "https://eapi.pcloud.com");
