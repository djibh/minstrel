using Minstrel.Application.Abstractions.Providers;
using Minstrel.Domain.ValueObjects;

namespace Minstrel.Application.Playback.Services;

public class PlaybackService
{
    private readonly ISourceRegistry _sourceRegistry;

    public PlaybackService(ISourceRegistry sourceRegistry)
    {
        _sourceRegistry = sourceRegistry;
    }

    public async Task<StreamDescriptor?> GetTrackStreamAsync(string trackId, CancellationToken cancellationToken)
    {
        foreach (var provider in _sourceRegistry.GetEnabledProviders())
        {
            try
            {
                return await provider.GetTrackStreamAsync(trackId, cancellationToken);
            }
            catch
            {
                // MVP : on tente le provider suivant
            }
        }

        return null;
    }
}
