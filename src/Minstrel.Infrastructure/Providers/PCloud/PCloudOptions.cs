namespace Minstrel.Infrastructure.Providers.PCloud;

public class PCloudOptions
{
    public const string SectionName = "PCloud";

    public string ApiBaseUrl { get; set; } = "https://api.pcloud.com";
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string MusicFolderPath { get; set; } = "/";
}
