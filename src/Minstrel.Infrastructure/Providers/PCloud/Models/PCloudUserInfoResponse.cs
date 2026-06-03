using System.Text.Json.Serialization;

namespace Minstrel.Infrastructure.Providers.PCloud.Models;

public record PCloudUserInfoResponse(
    [property: JsonPropertyName("result")] int Result,
    [property: JsonPropertyName("token")] string? Token,
    [property: JsonPropertyName("auth")] string? Auth,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("userid")] long UserId,
    [property: JsonPropertyName("hostname")] string? Hostname,
    [property: JsonPropertyName("error")] string? Error
)
{
    public string? AccessToken => Token ?? Auth;
}
