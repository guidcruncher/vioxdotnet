using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Spotify.Configuration;
using Viox.Client.Spotify.Services;

using Viox.Core.Services;

namespace Viox.Client.Spotify.Extensions;

/// <summary>
/// Extension methods for setting up Spotify Client services in an <see cref="IServiceCollection" />.
/// </summary>
public static class SpotifyClientServiceCollectionExtensions
{
    /// <summary>
    /// Adds Spotify Client components to the specified service collection.
    /// </summary>
    public static IServiceCollection AddSpotifyClient(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<SpotifyOptions>(configuration.GetSection(SpotifyOptions.SectionName));


        services.AddHttpClient<ISpotifyClient, SpotifyClient>();

        // Register implementations with unique string keys using Microsoft DI extensions
        services.AddKeyedSingleton<IMediaSource, SpotifyMediaSource>("spotify");

        return services;
    }

    /// <summary>
    /// Adds Spotify Client components with customized programmatic options.
    /// </summary>
    public static IServiceCollection AddSpotifyClient(this IServiceCollection services, Action<SpotifyOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);

        services.AddHttpClient<ISpotifyClient, SpotifyClient>();

        // Register implementations with unique string keys using Microsoft DI extensions
        services.AddKeyedSingleton<IMediaSource, SpotifyMediaSource>("spotify");

        return services;
    }
}
