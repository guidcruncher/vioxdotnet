using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Viox.Client.Podverse.Configuration;
using Viox.Client.Podverse.Services;
using Viox.Core.Services;

namespace Viox.Client.Podverse.Extensions;

/// <summary>
/// Extension methods for registering <see cref="IPodverseClient"/> in Dependency Injection containers.
/// </summary>
public static class PodverseServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Podverse API client and options to the service collection using an action delegate.
    /// </summary>
    /// <param name="services">The target <see cref="IServiceCollection"/>.</param>
    /// <param name="configureOptions">An action to configure <see cref="PodverseOptions"/>.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddPodverseClient(
        this IServiceCollection services,
        Action<PodverseOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);

        services.AddHttpClient<IPodverseClient, PodverseClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<PodverseOptions>>().Value;
            httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        // Register implementations with unique string keys using Microsoft DI extensions
        services.AddKeyedSingleton<IMediaSource, PodverseMediaSource>("podverse");

        return services;
    }

    /// <summary>
    /// Adds the Podverse API client and options to the service collection using an <see cref="IConfiguration"/> instance.
    /// </summary>
    /// <param name="services">The target <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">The configuration section containing Podverse settings.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddPodverseClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<PodverseOptions>(configuration);

        services.AddHttpClient<IPodverseClient, PodverseClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<PodverseOptions>>().Value;
            httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        // Register implementations with unique string keys using Microsoft DI extensions
        services.AddKeyedSingleton<IMediaSource, PodverseMediaSource>("podverse");

        return services;
    }
}
