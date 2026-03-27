using Minstrel.Application.Abstractions.Providers;
using Minstrel.Domain.Interfaces;

namespace Minstrel.Infrastructure.Sources;

public class SourceRegistry : ISourceRegistry
{
    private readonly IReadOnlyCollection<IMediaSourceProvider> _providers;

    public SourceRegistry(IEnumerable<IMediaSourceProvider> providers)
    {
        _providers = providers.ToList();
    }

    public IReadOnlyCollection<IMediaSourceProvider> GetEnabledProviders() => _providers;

    public IMediaSourceProvider? GetProviderBySourceId(string sourceId)
        => _providers.FirstOrDefault(x => x.SourceId.Equals(sourceId, StringComparison.OrdinalIgnoreCase));
}
