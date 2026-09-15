namespace Viox.Server.Extensions;

using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Server.Abstraction;
using Viox.Server.Adapters;
using Viox.Server.Services;

/// <summary>
/// Dependency Injection registration extensions for the unified media player control surface.
/// </summary>
public static class MediaPlayerServiceCollectionExtensions
{
    public static IServiceCollection AddUnifiedMediaPlayerControlSurface(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<MediaPlayerOptions>(configuration.GetSection(MediaPlayerOptions.Position));

        services.AddSingleton<IMediaPlayerAdapter, LibrespotMediaPlayerAdapter>();
        services.AddSingleton<IMediaPlayerAdapter, MpdMediaPlayerAdapter>();
        services.AddSingleton<IMediaPlayerControlSurface, CompositeMediaPlayerControlSurface>();

        return services;
    }
}
