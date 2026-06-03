using System.Text.Json.Serialization;

namespace Minstrel.Infrastructure.Providers.PCloud.Models;

public record PCloudListFolderResponse(
    [property: JsonPropertyName("result")] int Result,
    [property: JsonPropertyName("metadata")] PCloudFolderMetadata? Metadata
);

public record PCloudFolderMetadata(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("path")] string? Path,
    [property: JsonPropertyName("folderid")] long FolderId,
    [property: JsonPropertyName("isfolder")] bool IsFolder,
    [property: JsonPropertyName("contents")] List<PCloudItem>? Contents
);

public record PCloudItem(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("isfolder")] bool IsFolder,
    [property: JsonPropertyName("fileid")] long FileId,
    [property: JsonPropertyName("folderid")] long FolderId,
    [property: JsonPropertyName("size")] long Size,
    [property: JsonPropertyName("contenttype")] string? ContentType,
    [property: JsonPropertyName("audio")] PCloudAudioMetadata? Audio,
    [property: JsonPropertyName("contents")] List<PCloudItem>? Contents
);

public record PCloudAudioMetadata(
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("artist")] string? Artist,
    [property: JsonPropertyName("album")] string? Album,
    [property: JsonPropertyName("year")] int? Year,
    [property: JsonPropertyName("duration")] double? Duration,
    [property: JsonPropertyName("track")] int? Track
);
