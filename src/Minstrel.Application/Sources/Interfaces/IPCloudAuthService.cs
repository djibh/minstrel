using Minstrel.Application.Sources;

namespace Minstrel.Application.Sources.Interfaces;

public interface IPCloudAuthService
{
    Task<PCloudAuthResult> ConnectAsync(string email, string password, string? code, CancellationToken cancellationToken);
    void SetToken(string token, string apiBaseUrl);
    bool IsConnected { get; }
    void Disconnect();
}
