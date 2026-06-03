using Microsoft.Extensions.DependencyInjection;
using Minstrel.Application.Abstractions.Providers;
using Minstrel.Application.Sources.Interfaces;
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
        services.AddSingleton<PCloudTokenStore>();
        services.AddSingleton<PCloudApiClient>();
        services.AddSingleton<IPCloudAuthService, PCloudAuthService>();
        services.AddSingleton<IMediaSourceProvider, PCloudMediaSourceProvider>();
        services.AddSingleton<ISourceRegistry, SourceRegistry>();

        return services;
    }
}
