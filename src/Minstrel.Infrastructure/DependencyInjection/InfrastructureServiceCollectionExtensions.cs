using Microsoft.Extensions.DependencyInjection;
using Minstrel.Application.Abstractions.Providers;
using Minstrel.Domain.Interfaces;
using Minstrel.Infrastructure.Providers.Mock;
using Minstrel.Infrastructure.Sources;

namespace Minstrel.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddMinstrelInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IMediaSourceProvider, MockMediaSourceProvider>();
        services.AddSingleton<ISourceRegistry, SourceRegistry>();

        return services;
    }
}
