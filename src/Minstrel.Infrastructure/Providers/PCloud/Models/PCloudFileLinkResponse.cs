using System.Text.Json.Serialization;

namespace Minstrel.Infrastructure.Providers.PCloud.Models;

public record PCloudFileLinkResponse(
    [property: JsonPropertyName("result")] int Result,
    [property: JsonPropertyName("hosts")] List<string>? Hosts,
    [property: JsonPropertyName("path")] string? Path
);
