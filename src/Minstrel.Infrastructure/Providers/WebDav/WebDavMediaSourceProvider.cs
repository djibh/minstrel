using System.Security.Cryptography;
using System.Text;
using Minstrel.Domain.Entities;
using Minstrel.Domain.Enums;
using Minstrel.Domain.Interfaces;
using Minstrel.Domain.ValueObjects;

namespace Minstrel.Infrastructure.Providers.WebDav;

public class WebDavMediaSourceProvider : IMediaSourceProvider
{
    private readonly WebDavClient _client;
    private readonly WebDavConfigStore _configStore;

    private List<WebDavAudioFile>? _cachedFiles;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private string? _cachedForServer;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private const int CacheDurationMinutes = 5;

    public string SourceId => "webdav";

    public WebDavMediaSourceProvider(WebDavClient client, WebDavConfigStore configStore)
    {
        _client = client;
        _configStore = configStore;
    }

    public Task<MediaSource> GetSourceAsync(CancellationToken cancellationToken)
        => Task.FromResult(new MediaSource
        {
            Id = SourceId,
            Kind = SourceKind.WebDav,
            DisplayName = "WebDAV",
            IsEnabled = _configStore.Current.IsConfigured,
            SyncStatus = SourceSyncStatus.Idle
        });

    public async Task<IReadOnlyCollection<Album>> GetAlbumsAsync(CancellationToken cancellationToken)
    {
        if (!_configStore.Current.IsConfigured) return [];

        var files = await GetCachedAudioFilesAsync(cancellationToken);

        return files
            .Select(f => ParseMetadata(f.Name))
            .Where(m => m.Album != "Unknown Album")
            .GroupBy(m => (m.Album, m.Artist))
            .Select(g => new Album
            {
                Id = ComputeId("album", g.Key.Album, g.Key.Artist),
                SourceId = SourceId,
                SourceKind = SourceKind.WebDav,
                Title = g.Key.Album,
                ArtistName = g.Key.Artist,
                Year = null,
                TrackCount = g.Count(),
                CoverUrl = null,
                IsOfflineAvailable = false
            })
            .ToList();
    }

    public async Task<IReadOnlyCollection<Artist>> GetArtistsAsync(CancellationToken cancellationToken)
    {
        if (!_configStore.Current.IsConfigured) return [];

        var files = await GetCachedAudioFilesAsync(cancellationToken);

        return files
            .Select(f => ParseMetadata(f.Name))
            .GroupBy(m => m.Artist)
            .Where(g => g.Key != "Unknown Artist")
            .Select(g => new Artist
            {
                Id = ComputeId("artist", g.Key),
                SourceId = SourceId,
                SourceKind = SourceKind.WebDav,
                Name = g.Key,
                ImageUrl = null,
                AlbumCount = g.Select(m => m.Album).Distinct().Count(a => a != "Unknown Album"),
                TrackCount = g.Count()
            })
            .ToList();
    }

    public async Task<IReadOnlyCollection<Track>> GetTracksAsync(CancellationToken cancellationToken)
    {
        if (!_configStore.Current.IsConfigured) return [];

        var files = await GetCachedAudioFilesAsync(cancellationToken);
        return files.Select(MapToTrack).ToList();
    }

    public Task<IReadOnlyCollection<Playlist>> GetPlaylistsAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyCollection<Playlist>>([]);

    public async Task<SearchResults> SearchAsync(string query, CancellationToken cancellationToken)
    {
        if (!_configStore.Current.IsConfigured) return new SearchResults { Query = query };

        var normalized = query.Trim().ToLowerInvariant();
        var files = await GetCachedAudioFilesAsync(cancellationToken);
        var tracks = files.Select(MapToTrack).ToList();

        return new SearchResults
        {
            Query = query,
            Tracks = tracks.Where(t =>
                t.Title.ToLowerInvariant().Contains(normalized) ||
                t.ArtistName.ToLowerInvariant().Contains(normalized)).ToList(),
            Albums = (await GetAlbumsAsync(cancellationToken)).Where(a =>
                a.Title.ToLowerInvariant().Contains(normalized) ||
                a.ArtistName.ToLowerInvariant().Contains(normalized)).ToList(),
            Artists = (await GetArtistsAsync(cancellationToken)).Where(a =>
                a.Name.ToLowerInvariant().Contains(normalized)).ToList(),
            Playlists = []
        };
    }

    public async Task<StreamDescriptor> GetTrackStreamAsync(string trackId, CancellationToken cancellationToken)
    {
        var files = await GetCachedAudioFilesAsync(cancellationToken);
        var file = files.FirstOrDefault(f => f.Id == trackId)
            ?? throw new ArgumentException($"WebDAV track not found: {trackId}");

        return new StreamDescriptor
        {
            StreamUri = new Uri(file.Url),
            IsRedirectPreferred = false,
            ProxyHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = _client.GetAuthorizationHeader()
            }
        };
    }

    private async Task<List<WebDavAudioFile>> GetCachedAudioFilesAsync(CancellationToken cancellationToken)
    {
        var config = _configStore.Current;

        if (_cachedFiles is not null && DateTime.UtcNow < _cacheExpiry && _cachedForServer == config.ServerUrl)
            return _cachedFiles;

        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            config = _configStore.Current;
            if (_cachedFiles is not null && DateTime.UtcNow < _cacheExpiry && _cachedForServer == config.ServerUrl)
                return _cachedFiles;

            _cachedFiles = await _client.ListAudioFilesAsync(cancellationToken);
            _cacheExpiry = DateTime.UtcNow.AddMinutes(CacheDurationMinutes);
            _cachedForServer = config.ServerUrl;
            return _cachedFiles;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private Track MapToTrack(WebDavAudioFile file)
    {
        var (title, artist, album) = ParseMetadata(file.Name);

        return new Track
        {
            Id = file.Id,
            SourceId = SourceId,
            SourceKind = SourceKind.WebDav,
            Title = title,
            ArtistName = artist,
            AlbumTitle = album,
            DurationSeconds = null,
            CoverUrl = null,
            IsOfflineAvailable = false
        };
    }

    private static (string Title, string Artist, string Album) ParseMetadata(string fileName)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var parts = nameWithoutExt.Split(" - ", StringSplitOptions.TrimEntries);

        return parts.Length switch
        {
            >= 3 => (parts[2], parts[0], parts[1]),
            2 => (parts[1], parts[0], "Unknown Album"),
            _ => (nameWithoutExt, "Unknown Artist", "Unknown Album")
        };
    }

    private static string ComputeId(string prefix, params string[] parts)
    {
        var combined = string.Join("|", parts);
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(combined));
        return $"{prefix}-{Convert.ToHexString(hash)[..12].ToLowerInvariant()}";
    }
}
