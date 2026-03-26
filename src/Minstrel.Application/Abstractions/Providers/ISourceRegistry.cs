using Minstrel.Domain.Interfaces;

namespace Minstrel.Application.Abstractions.Providers;

public interface ISourceRegistry
{
    IReadOnlyCollection<IMediaSourceProvider> GetEnabledProviders();
    IMediaSourceProvider? GetProviderBySourceId(string sourceId);
}
