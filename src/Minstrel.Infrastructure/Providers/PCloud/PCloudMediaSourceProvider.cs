using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Minstrel.Domain.Entities;
using Minstrel.Domain.Enums;
using Minstrel.Domain.Interfaces;
using Minstrel.Domain.ValueObjects;
using Minstrel.Infrastructure.Providers.PCloud.Models;

namespace Minstrel.Infrastructure.Providers.PCloud;

public class PCloudMediaSourceProvider : IMediaSourceProvider
{
    private readonly PCloudApiClient _apiClient;
    private readonly PCloudTokenStore _tokenStore;
    private readonly PCloudOptions _options;

    private List<PCloudItem>? _cachedFiles;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private const int CacheDurationMinutes = 5;

    public string SourceId => "pcloud-main";

    public PCloudMediaSourceProvider(PCloudApiClient apiClient, PCloudTokenStore tokenStore, IOptions<PCloudOptions> options)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
        _options = options.Value;
    }

    public Task<MediaSource> GetSourceAsync(CancellationToken cancellationToken)
        => Task.FromResult(new MediaSource
        {
            Id = SourceId,
            Kind = SourceKind.PCloud,
            DisplayName = "pCloud",
            IsEnabled = _tokenStore.HasToken,
            SyncStatus = SourceSyncStatus.Idle
        });

    public async Task<IReadOnlyCollection<Album>> GetAlbumsAsync(CancellationToken cancellationToken)
    {
        if (!_tokenStore.HasToken) return [];

        var files = await GetCachedAudioFilesAsync(cancellationToken);

        return files
            .Where(f => f.Audio?.Album is not null)
            .GroupBy(f => (Album: f.Audio!.Album!, Artist: f.Audio.Artist ?? "Unknown Artist"))
            .Select(g => new Album
            {
                Id = ComputeId("album", g.Key.Album, g.Key.Artist),
                SourceId = SourceId,
                SourceKind = SourceKind.PCloud,
                Title = g.Key.Album,
                ArtistName = g.Key.Artist,
                Year = g.FirstOrDefault(f => f.Audio?.Year is not null)?.Audio?.Year,
                TrackCount = g.Count(),
                CoverUrl = null,
                IsOfflineAvailable = false
            })
            .ToList();
    }

    public async Task<IReadOnlyCollection<Artist>> GetArtistsAsync(CancellationToken cancellationToken)
    {
        if (!_tokenStore.HasToken) return [];

        var files = await GetCachedAudioFilesAsync(cancellationToken);

        return files
            .Where(f => f.Audio?.Artist is not null)
            .GroupBy(f => f.Audio!.Artist!)
            .Select(g => new Artist
            {
                Id = ComputeId("artist", g.Key),
                SourceId = SourceId,
                SourceKind = SourceKind.PCloud,
                Name = g.Key,
                ImageUrl = null,
                AlbumCount = g.Select(f => f.Audio?.Album).Distinct().Count(a => a is not null),
                TrackCount = g.Count()
            })
            .ToList();
    }

    public async Task<IReadOnlyCollection<Track>> GetTracksAsync(CancellationToken cancellationToken)
    {
        if (!_tokenStore.HasToken) return [];

        var files = await GetCachedAudioFilesAsync(cancellationToken);
        return files.Select(MapToTrack).ToList();
    }

    public Task<IReadOnlyCollection<Playlist>> GetPlaylistsAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyCollection<Playlist>>([]);

    public async Task<SearchResults> SearchAsync(string query, CancellationToken cancellationToken)
    {
        if (!_tokenStore.HasToken) return new SearchResults { Query = query };

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
        if (!long.TryParse(trackId, out var fileId))
            throw new ArgumentException($"Invalid pCloud track ID: {trackId}");

        var streamUrl = await _apiClient.GetFileLinkAsync(fileId, cancellationToken);

        return new StreamDescriptor
        {
            StreamUri = new Uri(streamUrl),
            IsRedirectPreferred = true
        };
    }

    private async Task<List<PCloudItem>> GetCachedAudioFilesAsync(CancellationToken cancellationToken)
    {
        if (_cachedFiles is not null && DateTime.UtcNow < _cacheExpiry)
            return _cachedFiles;

        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedFiles is not null && DateTime.UtcNow < _cacheExpiry)
                return _cachedFiles;

            _cachedFiles = await _apiClient.ListAudioFilesAsync(_options.MusicFolderPath, cancellationToken);
            _cacheExpiry = DateTime.UtcNow.AddMinutes(CacheDurationMinutes);
            return _cachedFiles;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private Track MapToTrack(PCloudItem file)
    {
        var (title, artist, album) = ParseMetadata(file);

        return new Track
        {
            Id = file.FileId.ToString(),
            SourceId = SourceId,
            SourceKind = SourceKind.PCloud,
            Title = title,
            ArtistName = artist,
            AlbumTitle = album,
            DurationSeconds = file.Audio?.Duration,
            CoverUrl = null,
            IsOfflineAvailable = false
        };
    }

    private static (string title, string artist, string album) ParseMetadata(PCloudItem file)
    {
        if (file.Audio is { Title: not null } audio)
        {
            return (
                audio.Title,
                audio.Artist ?? "Unknown Artist",
                audio.Album ?? "Unknown Album"
            );
        }

        var nameWithoutExt = Path.GetFileNameWithoutExtension(file.Name);
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
