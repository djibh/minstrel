namespace Minstrel.Infrastructure.Providers.WebDav;

public class WebDavOptions
{
    public const string SectionName = "WebDav";

    public string ServerUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string MusicFolderPath { get; set; } = "/";
}
