// File: M3uPlaylistParserServiceCollectionExtensions.cs

namespace Viox.Core.Extensions;

using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Configuration;
using Viox.Core.Services;

/// <summary>
/// Extension methods for registering M3U playlist parser services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class M3uPlaylistParserServiceCollectionExtensions
{
    /// <summary>
    /// Adds the M3U playlist parser service and its dependencies to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <returns>The <see cref="IServiceCollection"/> to allow method chaining.</returns>
    public static IServiceCollection AddM3uPlaylistParser(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<M3uPlaylistParserOptions>();
        services.AddHttpClient<IM3uPlaylistParser, M3uPlaylistParser>();

        return services;
    }

    /// <summary>
    /// Adds the M3U playlist parser service to the specified <see cref="IServiceCollection"/> with custom options configuration.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="configureOptions">An action delegate to configure <see cref="M3uPlaylistParserOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> to allow method chaining.</returns>
    public static IServiceCollection AddM3uPlaylistParser(
        this IServiceCollection services,
        Action<M3uPlaylistParserOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddM3uPlaylistParser();

        return services;
    }

    /// <summary>
    /// Adds the M3U playlist parser service to the specified <see cref="IServiceCollection"/> bound to an <see cref="IConfiguration"/> section.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="configuration">The configuration section containing <see cref="M3uPlaylistParserOptions"/> settings.</param>
    /// <returns>The <see cref="IServiceCollection"/> to allow method chaining.</returns>
    public static IServiceCollection AddM3uPlaylistParser(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<M3uPlaylistParserOptions>(configuration.GetSection(M3uPlaylistParserOptions.SectionName));
        services.AddM3uPlaylistParser();

        return services;
    }
}
