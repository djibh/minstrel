using Microsoft.Extensions.DependencyInjection;
using Minstrel.Application.Abstractions.Providers;
using Minstrel.Domain.Interfaces;
using Minstrel.Infrastructure.Providers.PCloud;
using Minstrel.Infrastructure.Sources;

namespace Minstrel.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddMinstrelInfrastructure(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddOptions<PCloudOptions>().BindConfiguration(PCloudOptions.SectionName);
        services.AddSingleton<PCloudConfigStore>();
        services.AddSingleton<PCloudApiClient>();
        services.AddSingleton<IMediaSourceProvider, PCloudMediaSourceProvider>();
        services.AddSingleton<ISourceRegistry, SourceRegistry>();

        return services;
    }
}
