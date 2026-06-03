using System.Text.Json.Serialization;

namespace Minstrel.Infrastructure.Providers.PCloud.Models;

public record PCloudOAuthTokenResponse(
    [property: JsonPropertyName("result")] int Result,
    [property: JsonPropertyName("access_token")] string? AccessToken,
    [property: JsonPropertyName("token_type")] string? TokenType,
    [property: JsonPropertyName("uid")] long Uid
);
