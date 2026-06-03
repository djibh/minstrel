namespace Minstrel.Infrastructure.Providers.PCloud;

public class PCloudTokenStore
{
    private volatile string? _accessToken;

    public bool HasToken => _accessToken is not null;
    public string? GetToken() => _accessToken;
    public void SetToken(string token) => _accessToken = token;
    public void ClearToken() => _accessToken = null;
}
