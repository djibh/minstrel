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

    public async Task<bool> ConnectAsync(string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _apiClient.DirectAuthAsync(email, password, cancellationToken);
            _tokenStore.SetToken(token);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Disconnect() => _tokenStore.ClearToken();
}
