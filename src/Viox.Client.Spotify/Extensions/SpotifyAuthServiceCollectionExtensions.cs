using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Spotify.Configuration;
using Viox.Client.Spotify.Services;

namespace Viox.Client.Spotify.Extensions;

/// <summary>
/// Extension methods for configuring Spotify services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class SpotifyAuthServiceCollectionExtensions
{
    /// <summary>
    /// Adds Spotify authentication services and options configuration to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The application configuration containing Spotify options.</param>
    /// <returns>The same <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddSpotifyAuthServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bind Spotify options section
        services.Configure<SpotifyAuthOptions>(
            configuration.GetSection(SpotifyAuthOptions.SectionName));

        // Register HttpClient and Spotify Service via Dependency Injection
        services.AddHttpClient<ISpotifyAuthService, SpotifyAuthService>();

        return services;
    }
}
