namespace Viox.Server.Extensions;

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Mpd.Extensions;
using Viox.Core.Extensions;
using Viox.Core.Models;
using Viox.Server.Abstraction;
using Viox.Server.Adapters;
using Viox.Server.Configuration;
using Viox.Server.Extensions;
using Viox.Server.Services;
using Viox.Snapcast.Extensions;

/// <summary>
/// Extension methods for registering all provided application services into a single unified service collection.
/// </summary>
public static class VioxServerServiceCollectionExtensions
{
    /// <summary>
    /// Registers all included application services, caches, proxy configurations, media player control surfaces, and API endpoints.
    /// </summary>
    /// <param name="services">The service collection instance.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <returns>The updated service collection for chaining.</returns>
    public static IServiceCollection AddVioxServerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);


        // 2. Memory Caches
        services.AddMemoryCacheService<List<MediaMetaData>>(configuration);

        // 3. Audio Proxy Services & Controllers Setup
        services.Configure<StreamingOptions>(
            configuration.GetSection(StreamingOptions.SectionName));

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
        });

        services.AddSingleton<AudioCacheManager>();
        services.AddHostedService<AudioCacheCleanupService>();

        services.AddHttpClient("AudioProxyClient", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<StreamingOptions>>().Value;
            client.Timeout = TimeSpan.FromMinutes(options.TimeoutMinutes);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
            AutomaticDecompression = DecompressionMethods.None
        });

        services.AddSnapcastClient(configuration);
        services.AddMpdClient(options =>
        {
            options.Host = "127.0.0.1";
            options.Port = 6600;
        });

        // 4. Unified Media Player Control Surface
        services.AddPlaybackQueue(configuration);
        services.Configure<MediaPlayerOptions>(configuration.GetSection(MediaPlayerOptions.SectionName));
        services.AddSingleton<IMediaPlayerAdapter, LibrespotMediaPlayerAdapter>();
        services.AddSingleton<IMediaPlayerAdapter, MpdMediaPlayerAdapter>();
        services.AddSingleton<IMediaPlayerControlSurface, CompositeMediaPlayerControlSurface>();
        services.AddHostedService<MediaPlayerPollingService>();

        // 5. Podcast Downloader
        services.Configure<PodcastDownloadOptions>(configuration.GetSection(PodcastDownloadOptions.SectionName));
        services.AddTransient<IPodcastProvider, FavoritesPodcastProvider>();
        services.AddHostedService<PodcastDownloaderBackgroundService>();

        // 6. API and OpenAPI Setup with JSON Options
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            });

        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "Viox.Net";
                document.Info.Version = "v1.0";
                document.Info.Description = "The Viox API";

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static IServiceCollection AddPlaybackQueue(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<PlaybackQueueOptions>(configuration.GetSection(PlaybackQueueOptions.SectionName));
        services.AddSingleton<IPlaybackQueueService, PlaybackQueueService>();

        return services;
    }

}
