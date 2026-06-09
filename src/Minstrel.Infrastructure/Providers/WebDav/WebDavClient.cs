using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace Minstrel.Infrastructure.Providers.WebDav;

public record WebDavAudioFile(string Id, string Url, string Name);

public class WebDavClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WebDavConfigStore _configStore;

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".flac", ".aac", ".ogg", ".wav", ".m4a", ".opus", ".wma", ".aiff"
    };

    private const string PropfindBody = """
        <?xml version="1.0" encoding="utf-8"?>
        <D:propfind xmlns:D="DAV:">
          <D:prop>
            <D:displayname/>
            <D:resourcetype/>
            <D:getcontenttype/>
          </D:prop>
        </D:propfind>
        """;

    public WebDavClient(IHttpClientFactory httpClientFactory, WebDavConfigStore configStore)
    {
        _httpClientFactory = httpClientFactory;
        _configStore = configStore;
    }

    public async Task<List<WebDavAudioFile>> ListAudioFilesAsync(CancellationToken cancellationToken)
    {
        var config = _configStore.Current;
        var client = CreateClient(config);
        var folderUrl = BuildFolderUrl(config);

        var request = new HttpRequestMessage(new HttpMethod("PROPFIND"), folderUrl);
        request.Headers.Add("Depth", "infinity");
        request.Content = new StringContent(PropfindBody, Encoding.UTF8, "application/xml");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync(cancellationToken);
        return ParsePropfindResponse(xml, config);
    }

    public string GetAuthorizationHeader()
    {
        var config = _configStore.Current;
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.Username}:{config.Password}"));
        return $"Basic {credentials}";
    }

    private HttpClient CreateClient(WebDavRuntimeConfig config)
    {
        var client = _httpClientFactory.CreateClient();
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.Username}:{config.Password}"));
        client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
        return client;
    }

    private static string BuildFolderUrl(WebDavRuntimeConfig config)
    {
        var serverUrl = config.ServerUrl.TrimEnd('/');
        var path = config.MusicFolderPath.StartsWith('/') ? config.MusicFolderPath : "/" + config.MusicFolderPath;
        return serverUrl + path;
    }

    private static List<WebDavAudioFile> ParsePropfindResponse(string xml, WebDavRuntimeConfig config)
    {
        var doc = XDocument.Parse(xml);
        XNamespace dav = "DAV:";
        var serverBase = new Uri(config.ServerUrl.TrimEnd('/') + "/");
        var result = new List<WebDavAudioFile>();

        foreach (var response in doc.Descendants(dav + "response"))
        {
            var href = response.Element(dav + "href")?.Value;
            if (string.IsNullOrEmpty(href)) continue;

            var propstat = response.Elements(dav + "propstat")
                .FirstOrDefault(ps => ps.Element(dav + "status")?.Value.Contains("200") == true);
            if (propstat is null) continue;

            var prop = propstat.Element(dav + "prop");
            if (prop is null) continue;

            // Skip collections (folders)
            if (prop.Element(dav + "resourcetype")?.Element(dav + "collection") is not null) continue;

            var displayName = prop.Element(dav + "displayname")?.Value;
            var contentType = prop.Element(dav + "getcontenttype")?.Value ?? string.Empty;
            var fileName = string.IsNullOrEmpty(displayName)
                ? Uri.UnescapeDataString(Path.GetFileName(href.TrimEnd('/')))
                : displayName;

            if (!IsAudioFile(fileName, contentType)) continue;

            var fileUrl = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? href
                : new Uri(serverBase, href.TrimStart('/')).ToString();

            var id = ComputeId(fileUrl);
            result.Add(new WebDavAudioFile(id, fileUrl, fileName));
        }

        return result;
    }

    private static bool IsAudioFile(string name, string contentType)
    {
        if (contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)) return true;
        return AudioExtensions.Contains(Path.GetExtension(name));
    }

    private static string ComputeId(string url)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(url));
        return $"webdav-{Convert.ToHexString(hash)[..16].ToLowerInvariant()}";
    }
}
