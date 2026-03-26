using Minstrel.Domain.Entities;
using Minstrel.Domain.ValueObjects;

namespace Minstrel.Domain.Interfaces;

public interface IMediaSourceProvider
{
    string SourceId { get; }

    Task<MediaSource> GetSourceAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Album>> GetAlbumsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Artist>> GetArtistsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Track>> GetTracksAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Playlist>> GetPlaylistsAsync(CancellationToken cancellationToken);

    Task<SearchResults> SearchAsync(string query, CancellationToken cancellationToken);
    Task<StreamDescriptor> GetTrackStreamAsync(string trackId, CancellationToken cancellationToken);
}
