namespace Minstrel.Application.Sources.Interfaces;

public interface IPCloudAuthService
{
    Task<bool> ConnectAsync(string email, string password, CancellationToken cancellationToken);
    bool IsConnected { get; }
    void Disconnect();
}
