namespace Viox.Core.Extensions;

using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Configuration;
using Viox.Core.Services;

/// <summary>
/// Extension methods for registering media search services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class MediaSearchServiceCollectionExtensions
{
    public static IServiceCollection AddMediaSearchServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<MediaSearchOptions>(
            configuration.GetSection(MediaSearchOptions.SectionName));

        services.AddSingleton<MediaSearchService>();

        return services;
    }
}
