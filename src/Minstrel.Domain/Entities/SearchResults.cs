namespace Minstrel.Domain.Entities;

public class SearchResults
{
    public required string Query { get; init; }
    public IReadOnlyCollection<Track> Tracks { get; init; } = [];
    public IReadOnlyCollection<Album> Albums { get; init; } = [];
    public IReadOnlyCollection<Artist> Artists { get; init; } = [];
    public IReadOnlyCollection<Playlist> Playlists { get; init; } = [];
}
