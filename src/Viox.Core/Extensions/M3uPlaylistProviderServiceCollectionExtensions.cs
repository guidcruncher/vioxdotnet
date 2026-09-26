// File: M3uPlaylistProviderServiceCollectionExtensions.cs

namespace Viox.Core.Extensions;

using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Viox.Core.Services;

/// <summary>
/// Extension methods for registering M3U playlist provider services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class M3uPlaylistProviderServiceCollectionExtensions
{
    /// <summary>
    /// Adds the M3U playlist provider and its required dependencies to the specified <see cref="IServiceCollection"/> as a singleton service.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <returns>The <see cref="IServiceCollection"/> to allow method chaining.</returns>
    public static IServiceCollection AddM3uPlaylistProvider(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Ensure parser dependency is registered
        services.AddM3uPlaylistParser();

        // Register provider as a Singleton to maintain in-memory cached state across application execution
        services.TryAddSingleton<IM3uPlaylistProvider, M3uPlaylistProvider>();

        return services;
    }

}
