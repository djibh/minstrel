using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Minstrel.Infrastructure.Providers.PCloud;

public class PCloudRuntimeConfig
{
    public string ApiBaseUrl { get; set; } = "https://api.pcloud.com";
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string MusicFolderPath { get; set; } = "/";
    public string AuthToken { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
}

public class PCloudConfigStore
{
    private volatile PCloudRuntimeConfig _current;
    private readonly string _configFilePath;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public PCloudConfigStore(IOptions<PCloudOptions> startupOptions)
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrEmpty(baseDir))
            baseDir = AppContext.BaseDirectory;

        var appDataDir = Path.Combine(baseDir, "Minstrel");
        Directory.CreateDirectory(appDataDir);
        _configFilePath = Path.Combine(appDataDir, "pcloud-config.json");
        _current = TryLoadFromFile() ?? FromOptions(startupOptions.Value);
    }

    public PCloudRuntimeConfig Current => _current;

    public async Task UpdateAsync(string apiBaseUrl, string email, string password, string musicFolderPath)
    {
        var next = new PCloudRuntimeConfig
        {
            ApiBaseUrl = apiBaseUrl,
            Email = email,
            Password = password,
            MusicFolderPath = musicFolderPath,
            AuthToken = string.Empty, // clear cached token on config change
        };
        await PersistAsync(next);
    }

    public async Task UpdateTokenAsync(string token)
    {
        var c = _current;
        var next = new PCloudRuntimeConfig
        {
            ApiBaseUrl = c.ApiBaseUrl,
            Email = c.Email,
            Password = c.Password,
            MusicFolderPath = c.MusicFolderPath,
            AuthToken = token,
        };
        await PersistAsync(next);
    }

    private async Task PersistAsync(PCloudRuntimeConfig config)
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

    private PCloudRuntimeConfig? TryLoadFromFile()
    {
        if (!File.Exists(_configFilePath)) return null;
        try
        {
            var json = File.ReadAllText(_configFilePath);
            return JsonSerializer.Deserialize<PCloudRuntimeConfig>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static PCloudRuntimeConfig FromOptions(PCloudOptions options) => new()
    {
        ApiBaseUrl = options.ApiBaseUrl,
        Email = options.Email,
        Password = options.Password,
        MusicFolderPath = options.MusicFolderPath,
    };
}
