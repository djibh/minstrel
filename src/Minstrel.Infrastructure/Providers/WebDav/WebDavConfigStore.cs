using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Minstrel.Infrastructure.Providers.WebDav;

public class WebDavRuntimeConfig
{
    public string ServerUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string MusicFolderPath { get; set; } = "/";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ServerUrl) && !string.IsNullOrWhiteSpace(Username);
}

public class WebDavConfigStore
{
    private volatile WebDavRuntimeConfig _current;
    private readonly string _configFilePath;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public WebDavConfigStore(IOptions<WebDavOptions> startupOptions)
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrEmpty(baseDir))
            baseDir = AppContext.BaseDirectory;

        var appDataDir = Path.Combine(baseDir, "Minstrel");
        Directory.CreateDirectory(appDataDir);
        _configFilePath = Path.Combine(appDataDir, "webdav-config.json");
        _current = TryLoadFromFile() ?? FromOptions(startupOptions.Value);
    }

    public WebDavRuntimeConfig Current => _current;

    public async Task UpdateAsync(string serverUrl, string username, string password, string musicFolderPath)
    {
        var next = new WebDavRuntimeConfig
        {
            ServerUrl = serverUrl,
            Username = username,
            Password = password,
            MusicFolderPath = musicFolderPath,
        };
        await PersistAsync(next);
    }

    private async Task PersistAsync(WebDavRuntimeConfig config)
    {
        await _writeLock.WaitAsync();
        try
        {
            await File.WriteAllTextAsync(_configFilePath, JsonSerializer.Serialize(config, JsonOptions));
            _current = config;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private WebDavRuntimeConfig? TryLoadFromFile()
    {
        if (!File.Exists(_configFilePath)) return null;
        try
        {
            var json = File.ReadAllText(_configFilePath);
            return JsonSerializer.Deserialize<WebDavRuntimeConfig>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static WebDavRuntimeConfig FromOptions(WebDavOptions options) => new()
    {
        ServerUrl = options.ServerUrl,
        Username = options.Username,
        Password = options.Password,
        MusicFolderPath = options.MusicFolderPath,
    };
}
