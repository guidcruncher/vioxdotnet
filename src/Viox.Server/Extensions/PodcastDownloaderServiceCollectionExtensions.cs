namespace Microsoft.Extensions.DependencyInjection;

using System;

using Microsoft.Extensions.Configuration;

using Viox.Server.Configuration;
using Viox.Server.Services;

/// <summary>
/// Extension methods for setting up Podcast Downloader services in Dependency Injection container.
/// </summary>
public static class PodcastDownloaderServiceCollectionExtensions
{
    /// <summary>
    /// Adds podcast background downloading services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Configuration instance to bind options from.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddPodcastDownloader(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<PodcastDownloadOptions>(configuration.GetSection(PodcastDownloadOptions.SectionName));

        services.AddHttpClient("AudioProxyClient");

        services.AddSingleton<AudioCacheManager>();
        services.AddTransient<IPodcastProvider, FavoritesPodcastProvider>();
        services.AddHostedService<PodcastDownloaderBackgroundService>();

        return services;
    }
}
