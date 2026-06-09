using System.Text.Json.Serialization;

namespace Minstrel.Infrastructure.Providers.PCloud.Models;

public record PCloudUserInfoResponse(
    [property: JsonPropertyName("result")] int Result,
    [property: JsonPropertyName("auth")] string? Auth,
    [property: JsonPropertyName("error")] string? Error
);
