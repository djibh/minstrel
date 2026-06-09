using System.Net.Http.Json;
using Minstrel.Infrastructure.Providers.PCloud.Models;

namespace Minstrel.Infrastructure.Providers.PCloud;

public class PCloudApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PCloudConfigStore _configStore;
    private readonly SemaphoreSlim _authLock = new(1, 1);

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".flac", ".aac", ".ogg", ".wav", ".m4a", ".opus", ".wma", ".aiff"
    };

    public PCloudApiClient(IHttpClientFactory httpClientFactory, PCloudConfigStore configStore)
    {
        _httpClientFactory = httpClientFactory;
        _configStore = configStore;
    }

    public async Task<List<PCloudItem>> ListAudioFilesAsync(CancellationToken cancellationToken)
    {
        var token = await GetOrRefreshTokenAsync(cancellationToken);
        var config = _configStore.Current;

        var url = $"{config.ApiBaseUrl}/listfolder" +
                  $"?path={Uri.EscapeDataString(config.MusicFolderPath)}" +
                  $"&recursive=1" +
                  $"&auth={Uri.EscapeDataString(token)}";

        return await ExecuteWithTokenRetryAsync(token, async t =>
        {
            var u = $"{config.ApiBaseUrl}/listfolder" +
                    $"?path={Uri.EscapeDataString(config.MusicFolderPath)}" +
                    $"&recursive=1" +
                    $"&auth={Uri.EscapeDataString(t)}";

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<PCloudListFolderResponse>(u, cancellationToken)
                ?? throw new InvalidOperationException("Empty response from pCloud listfolder.");

            if (response.Result != 0)
                throw new PCloudApiException(response.Result, $"pCloud listfolder error (result={response.Result}).");

            var audioFiles = new List<PCloudItem>();
            CollectAudioFiles(response.Metadata?.Contents, audioFiles);
            return audioFiles;
        }, cancellationToken);
    }

    public async Task<string> GetFileLinkAsync(long fileId, CancellationToken cancellationToken)
    {
        var config = _configStore.Current;

        return await ExecuteWithTokenRetryAsync(await GetOrRefreshTokenAsync(cancellationToken), async t =>
        {
            var url = $"{config.ApiBaseUrl}/getfilelink" +
                      $"?fileid={fileId}" +
                      $"&auth={Uri.EscapeDataString(t)}";

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<PCloudFileLinkResponse>(url, cancellationToken)
                ?? throw new InvalidOperationException("Empty response from pCloud getfilelink.");

            if (response.Result != 0 || response.Hosts is null || response.Path is null)
                throw new PCloudApiException(response.Result, $"pCloud getfilelink error (result={response.Result}).");

            return $"https://{response.Hosts[0]}{response.Path}";
        }, cancellationToken);
    }

    public async Task<PCloudAuthResult> AuthenticateAsync(string? verificationCode, CancellationToken cancellationToken)
    {
        await _configStore.UpdateTokenAsync(string.Empty);
        try
        {
            await DoAuthenticateAsync(verificationCode, cancellationToken);
            return PCloudAuthResult.Success;
        }
        catch (PCloudApiException ex) when (ex.ResultCode == 1022)
        {
            return PCloudAuthResult.VerificationRequired;
        }
        catch (Exception ex)
        {
            return PCloudAuthResult.Failure(ex.Message);
        }
    }

    private async Task<string> GetOrRefreshTokenAsync(CancellationToken cancellationToken)
    {
        var token = _configStore.Current.AuthToken;
        if (!string.IsNullOrWhiteSpace(token))
            return token;

        await _authLock.WaitAsync(cancellationToken);
        try
        {
            token = _configStore.Current.AuthToken;
            if (!string.IsNullOrWhiteSpace(token))
                return token;

            return await DoAuthenticateAsync(null, cancellationToken);
        }
        finally
        {
            _authLock.Release();
        }
    }

    private async Task<string> DoAuthenticateAsync(string? verificationCode, CancellationToken cancellationToken)
    {
        var config = _configStore.Current;
        var url = $"{config.ApiBaseUrl}/userinfo?getauth=1" +
                  $"&username={Uri.EscapeDataString(config.Email)}" +
                  $"&password={Uri.EscapeDataString(config.Password)}";

        if (!string.IsNullOrWhiteSpace(verificationCode))
            url += $"&code={Uri.EscapeDataString(verificationCode)}";

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetFromJsonAsync<PCloudUserInfoResponse>(url, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from pCloud userinfo.");

        if (response.Result == 1022)
            throw new PCloudApiException(1022, "pCloud email verification required.");

        if (response.Result != 0 || string.IsNullOrWhiteSpace(response.Auth))
            throw new InvalidOperationException($"pCloud auth error (result={response.Result}): {response.Error}");

        await _configStore.UpdateTokenAsync(response.Auth);
        return response.Auth;
    }

    private async Task<T> ExecuteWithTokenRetryAsync<T>(string token, Func<string, Task<T>> action, CancellationToken cancellationToken)
    {
        try
        {
            return await action(token);
        }
        catch (PCloudApiException ex) when (ex.ResultCode is 1000 or 2000 or 2094)
        {
            await _configStore.UpdateTokenAsync(string.Empty);
            var refreshed = await GetOrRefreshTokenAsync(cancellationToken);
            return await action(refreshed);
        }
    }

    private static void CollectAudioFiles(List<PCloudItem>? items, List<PCloudItem> result)
    {
        if (items is null) return;

        foreach (var item in items)
        {
            if (item.IsFolder)
                CollectAudioFiles(item.Contents, result);
            else if (IsAudioFile(item))
                result.Add(item);
        }
    }

    private static bool IsAudioFile(PCloudItem item)
    {
        if (item.ContentType?.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) == true)
            return true;

        return AudioExtensions.Contains(Path.GetExtension(item.Name));
    }
}

internal sealed class PCloudApiException(int resultCode, string message) : InvalidOperationException(message)
{
    public int ResultCode { get; } = resultCode;
}

public sealed class PCloudAuthResult
{
    public bool Connected { get; private init; }
    public bool RequiresVerification { get; private init; }
    public string? Error { get; private init; }

    public static readonly PCloudAuthResult Success = new() { Connected = true };
    public static readonly PCloudAuthResult VerificationRequired = new() { RequiresVerification = true };
    public static PCloudAuthResult Failure(string error) => new() { Error = error };
}
