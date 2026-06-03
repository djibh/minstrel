using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Minstrel.Infrastructure.Providers.PCloud.Models;

namespace Minstrel.Infrastructure.Providers.PCloud;

public class PCloudApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PCloudTokenStore _tokenStore;
    private readonly PCloudOptions _options;

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".flac", ".aac", ".ogg", ".wav", ".m4a", ".opus", ".wma", ".aiff"
    };

    public PCloudApiClient(IHttpClientFactory httpClientFactory, PCloudTokenStore tokenStore, IOptions<PCloudOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
        _options = options.Value;
    }

    public async Task<string> DirectAuthAsync(string email, string password, CancellationToken cancellationToken)
    {
        var url = $"{_options.ApiBaseUrl}/userinfo" +
                  $"?getauth=1&logout=1" +
                  $"&username={Uri.EscapeDataString(email)}" +
                  $"&password={Uri.EscapeDataString(password)}";

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetFromJsonAsync<PCloudUserInfoResponse>(url, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from pCloud userinfo.");

        if (response.Result != 0 || response.AccessToken is null)
            throw new InvalidOperationException($"pCloud auth error (result={response.Result}).");

        return response.AccessToken;
    }

    public async Task<List<PCloudItem>> ListAudioFilesAsync(string folderPath, CancellationToken cancellationToken)
    {
        var token = _tokenStore.GetToken()
            ?? throw new InvalidOperationException("No pCloud access token.");

        var url = $"{_options.ApiBaseUrl}/listfolder" +
                  $"?path={Uri.EscapeDataString(folderPath)}" +
                  $"&recursive=1" +
                  $"&access_token={Uri.EscapeDataString(token)}";

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetFromJsonAsync<PCloudListFolderResponse>(url, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from pCloud listfolder.");

        if (response.Result != 0)
            throw new InvalidOperationException($"pCloud listfolder error (result={response.Result}).");

        var audioFiles = new List<PCloudItem>();
        CollectAudioFiles(response.Metadata?.Contents, audioFiles);
        return audioFiles;
    }

    public async Task<string> GetFileLinkAsync(long fileId, CancellationToken cancellationToken)
    {
        var token = _tokenStore.GetToken()
            ?? throw new InvalidOperationException("No pCloud access token.");

        var url = $"{_options.ApiBaseUrl}/getfilelink" +
                  $"?fileid={fileId}" +
                  $"&access_token={Uri.EscapeDataString(token)}";

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetFromJsonAsync<PCloudFileLinkResponse>(url, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from pCloud getfilelink.");

        if (response.Result != 0 || response.Hosts is null || response.Path is null)
            throw new InvalidOperationException($"pCloud getfilelink error (result={response.Result}).");

        return $"https://{response.Hosts[0]}{response.Path}";
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

        var extension = Path.GetExtension(item.Name);
        return AudioExtensions.Contains(extension);
    }
}
