namespace Minstrel.Infrastructure.Providers.PCloud;

public class PCloudOptions
{
    public const string SectionName = "PCloud";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = "https://eapi.pcloud.com";
    public long? MusicFolderId { get; set; }
    public string MusicFolderPath { get; set; } = "/";
}
