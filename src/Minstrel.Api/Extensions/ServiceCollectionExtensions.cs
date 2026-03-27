using System;
using Minstrel.Application.Librairy.Services;
using Minstrel.Application.Playback.Services;
using Minstrel.Application.Search.Services;
using Minstrel.Infrastructure.DependencyInjection;

namespace Minstrel.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMinstrelServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddScoped<LibraryAggregationService>();
        services.AddScoped<SearchAggregationService>();
        services.AddScoped<PlaybackService>();

        services.AddMinstrelInfrastructure();

        return services;
    }
}
