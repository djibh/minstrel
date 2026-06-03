namespace Minstrel.Infrastructure.Providers.PCloud;

public class PCloudTokenStore
{
    private volatile string? _accessToken;
    private volatile string? _apiBaseUrl;

    public bool HasToken => _accessToken is not null;
    public string? GetToken() => _accessToken;
    public string? GetApiBaseUrl() => _apiBaseUrl;

    public void SetToken(string token, string apiBaseUrl)
    {
        _accessToken = token;
        _apiBaseUrl = apiBaseUrl;
    }

    public void ClearToken()
    {
        _accessToken = null;
        _apiBaseUrl = null;
    }
}
