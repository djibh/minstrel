using Minstrel.Application.Abstractions.Providers;
using Minstrel.Domain.Entities;
using Minstrel.Domain.Interfaces;

namespace Minstrel.Application.Librairy.Services;

public class LibraryAggregationService
{
    private readonly ISourceRegistry _sourceRegistry;

    public LibraryAggregationService(ISourceRegistry sourceRegistry)
    {
        _sourceRegistry = sourceRegistry;
    }

    public async Task<IReadOnlyCollection<Album>> GetAlbumsAsync(string? sourceFilter, string? sort, CancellationToken cancellationToken)
    {
        var providers = FilterProviders(sourceFilter);
        var albums = new List<Album>();

        foreach (var provider in providers)
        {
            albums.AddRange(await provider.GetAlbumsAsync(cancellationToken));
        }

        return ApplyAlbumSort(albums, sort).ToList();
    }

    public async Task<IReadOnlyCollection<Artist>> GetArtistsAsync(string? sourceFilter, string? sort, CancellationToken cancellationToken)
    {
        var providers = FilterProviders(sourceFilter);
        var artists = new List<Artist>();

        foreach (var provider in providers)
        {
            artists.AddRange(await provider.GetArtistsAsync(cancellationToken));
        }

        return artists.OrderBy(x => x.Name).ToList();
    }

    public async Task<IReadOnlyCollection<Track>> GetTracksAsync(string? sourceFilter, string? sort, CancellationToken cancellationToken)
    {
        var providers = FilterProviders(sourceFilter);
        var tracks = new List<Track>();

        foreach (var provider in providers)
        {
            tracks.AddRange(await provider.GetTracksAsync(cancellationToken));
        }

        return ApplyTrackSort(tracks, sort).ToList();
    }

    public async Task<IReadOnlyCollection<Playlist>> GetPlaylistsAsync(string? sourceFilter, string? sort, CancellationToken cancellationToken)
    {
        var providers = FilterProviders(sourceFilter);
        var playlists = new List<Playlist>();

        foreach (var provider in providers)
        {
            playlists.AddRange(await provider.GetPlaylistsAsync(cancellationToken));
        }

        return playlists.OrderBy(x => x.Name).ToList();
    }

    private IReadOnlyCollection<IMediaSourceProvider> FilterProviders(string? sourceFilter)
    {
        var providers = _sourceRegistry.GetEnabledProviders();

        if (string.IsNullOrWhiteSpace(sourceFilter) || sourceFilter.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return providers;
        }

        return providers
            .Where(x => x.SourceId.Equals(sourceFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static IEnumerable<Album> ApplyAlbumSort(IEnumerable<Album> items, string? sort)
        => sort?.ToLowerInvariant() switch
        {
            "artist" => items.OrderBy(x => x.ArtistName).ThenBy(x => x.Title),
            "year" => items.OrderByDescending(x => x.Year).ThenBy(x => x.Title),
            _ => items.OrderBy(x => x.Title)
        };

    private static IEnumerable<Track> ApplyTrackSort(IEnumerable<Track> items, string? sort)
        => sort?.ToLowerInvariant() switch
        {
            "artist" => items.OrderBy(x => x.ArtistName).ThenBy(x => x.AlbumTitle).ThenBy(x => x.Title),
            _ => items.OrderBy(x => x.Title)
        };
}