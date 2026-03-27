using Minstrel.Domain.Entities;
using Minstrel.Domain.Enums;
using Minstrel.Domain.Interfaces;
using Minstrel.Domain.ValueObjects;

namespace Minstrel.Infrastructure.Providers.Mock;

public class MockMediaSourceProvider : IMediaSourceProvider
{
    private const string MockSourceId = "pcloud-main";

    private readonly List<Album> _albums =
    [
        new()
        {
            Id = "album-1",
            SourceId = MockSourceId,
            SourceKind = SourceKind.PCloud,
            Title = "Midnight Echoes",
            ArtistName = "Nova Lines",
            Year = 2025,
            TrackCount = 4,
            CoverUrl = null,
            IsOfflineAvailable = false
        },
        new()
        {
            Id = "album-2",
            SourceId = MockSourceId,
            SourceKind = SourceKind.PCloud,
            Title = "Blue Static",
            ArtistName = "Kara North",
            Year = 2024,
            TrackCount = 3,
            CoverUrl = null,
            IsOfflineAvailable = true
        }
    ];


    private readonly List<Artist> _artists =
    [
    new()
        {
            Id = "artist-1",
            SourceId = MockSourceId,
            SourceKind = SourceKind.PCloud,
            Name = "Nova Lines",
            ImageUrl = null,
            AlbumCount = 1,
            TrackCount = 4
        },
        new()
        {
            Id = "artist-2",
            SourceId = MockSourceId,
            SourceKind = SourceKind.PCloud,
            Name = "Kara North",
            ImageUrl = null,
            AlbumCount = 1,
            TrackCount = 3
        }
    ];

    private readonly List<Track> _tracks =
    [
    new()
        {
            Id = "track-1",
            SourceId = MockSourceId,
            SourceKind = SourceKind.PCloud,
            Title = "Afterglow",
            ArtistName = "Nova Lines",
            AlbumTitle = "Midnight Echoes",
            DurationSeconds = 238,
            CoverUrl = null,
            IsOfflineAvailable = false
        },
        new()
        {
            Id = "track-2",
            SourceId = MockSourceId,
            SourceKind = SourceKind.PCloud,
            Title = "Static Hearts",
            ArtistName = "Nova Lines",
            AlbumTitle = "Midnight Echoes",
            DurationSeconds = 251,
            CoverUrl = null,
            IsOfflineAvailable = false
        },
        new()
        {
            Id = "track-3",
            SourceId = MockSourceId,
            SourceKind = SourceKind.PCloud,
            Title = "Blue Static",
            ArtistName = "Kara North",
            AlbumTitle = "Blue Static",
            DurationSeconds = 214,
            CoverUrl = null,
            IsOfflineAvailable = true
        }
    ];

    private readonly List<Playlist> _playlists =
    [
        new()
        {
            Id = "playlist-1",
            SourceId = MockSourceId,
            SourceKind = SourceKind.PCloud,
            Name = "Night Drive",
            CoverUrl = null,
            TrackCount = 12,
            IsOfflineAvailable = false
        },
        new()
        {
            Id = "playlist-2",
            SourceId = MockSourceId,
            SourceKind = SourceKind.PCloud,
            Name = "Offline Mix",
            CoverUrl = null,
            TrackCount = 9,
            IsOfflineAvailable = true
        }
    ];

    public string SourceId => MockSourceId;

    public Task<MediaSource> GetSourceAsync(CancellationToken cancellationToken)
        => Task.FromResult(new MediaSource
        {
            Id = MockSourceId,
            Kind = SourceKind.PCloud,
            DisplayName = "pCloud",
            IsEnabled = true,
            SyncStatus = SourceSyncStatus.Idle
        });

    public Task<IReadOnlyCollection<Album>> GetAlbumsAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyCollection<Album>>(_albums);

    public Task<IReadOnlyCollection<Artist>> GetArtistsAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyCollection<Artist>>(_artists);

    public Task<IReadOnlyCollection<Track>> GetTracksAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyCollection<Track>>(_tracks);

    public Task<IReadOnlyCollection<Playlist>> GetPlaylistsAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyCollection<Playlist>>(_playlists);

    public Task<SearchResults> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var normalized = query.Trim().ToLowerInvariant();

        return Task.FromResult(new SearchResults
        {
            Query = query,
            Tracks = _tracks.Where(x => x.Title.ToLowerInvariant().Contains(normalized) || x.ArtistName.ToLowerInvariant().Contains(normalized)).ToList(),
            Albums = _albums.Where(x => x.Title.ToLowerInvariant().Contains(normalized) || x.ArtistName.ToLowerInvariant().Contains(normalized)).ToList(),
            Artists = _artists.Where(x => x.Name.ToLowerInvariant().Contains(normalized)).ToList(),
            Playlists = _playlists.Where(x => x.Name.ToLowerInvariant().Contains(normalized)).ToList()
        });
    }

    public Task<StreamDescriptor> GetTrackStreamAsync(string trackId, CancellationToken cancellationToken)
        => Task.FromResult(new StreamDescriptor
        {
            StreamUri = new Uri("https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3"),
            IsRedirectPreferred = true
        });
}