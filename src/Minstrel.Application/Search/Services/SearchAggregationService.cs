using Minstrel.Application.Abstractions.Providers;
using Minstrel.Domain.Entities;

namespace Minstrel.Application.Search.Services;

public class SearchAggregationService
{
    private readonly ISourceRegistry _sourceRegistry;

    public SearchAggregationService(ISourceRegistry sourceRegistry)
    {
        _sourceRegistry = sourceRegistry;
    }

    public async Task<SearchResults> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var providers = _sourceRegistry.GetEnabledProviders();

        var tracks = new List<Track>();
        var albums = new List<Album>();
        var artists = new List<Artist>();
        var playlists = new List<Playlist>();

        foreach (var provider in providers)
        {
            var result = await provider.SearchAsync(query, cancellationToken);
            tracks.AddRange(result.Tracks);
            albums.AddRange(result.Albums);
            artists.AddRange(result.Artists);
            playlists.AddRange(result.Playlists);
        }

        return new SearchResults
        {
            Query = query,
            Tracks = tracks.OrderBy(x => x.Title).ToList(),
            Albums = albums.OrderBy(x => x.Title).ToList(),
            Artists = artists.OrderBy(x => x.Name).ToList(),
            Playlists = playlists.OrderBy(x => x.Name).ToList()
        };
    }
}
