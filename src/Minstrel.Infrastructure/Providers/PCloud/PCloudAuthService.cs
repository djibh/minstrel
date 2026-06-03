using Minstrel.Application.Sources;
using Minstrel.Application.Sources.Interfaces;

namespace Minstrel.Infrastructure.Providers.PCloud;

public class PCloudAuthService : IPCloudAuthService
{
    private readonly PCloudApiClient _apiClient;
    private readonly PCloudTokenStore _tokenStore;

    public PCloudAuthService(PCloudApiClient apiClient, PCloudTokenStore tokenStore)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
    }

    public bool IsConnected => _tokenStore.HasToken;

    public async Task<PCloudAuthResult> ConnectAsync(string email, string password, string? code, CancellationToken cancellationToken)
    {
        var connected = await _apiClient.DirectAuthAsync(email, password, code, cancellationToken);
        return connected ? PCloudAuthResult.Success() : PCloudAuthResult.CodeRequired();
    }

    public void SetToken(string token, string apiBaseUrl) => _tokenStore.SetToken(token, apiBaseUrl);

    public void Disconnect() => _tokenStore.ClearToken();
}
