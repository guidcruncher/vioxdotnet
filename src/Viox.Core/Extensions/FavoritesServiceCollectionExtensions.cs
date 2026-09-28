using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Configuration;
using Viox.Core.Services;
using Viox.Core.Storage;

namespace Viox.Core.Extensions;

/// <summary>
/// Extension methods for setting up Favorites storage engine services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class FavoritesServiceCollectionExtensions
{
    /// <summary>
    /// Adds Favorites Storage Engine and application services to the DI container.
    /// </summary>
    public static IServiceCollection AddFavoritesEngine(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<FavoritesStorageOptions>(
            configuration.GetSection(FavoritesStorageOptions.SectionName));

        services.AddSingleton<IFavoritesStorageEngine, FavoritesStorageEngine>();
        services.AddSingleton<IFavoritesService, FavoritesService>();

        return services;
    }

    /// <summary>
    /// Adds Favorites Storage Engine with custom explicit options configured via delegate.
    /// </summary>
    public static IServiceCollection AddFavoritesEngine(
        this IServiceCollection services,
        Action<FavoritesStorageOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);

        services.AddSingleton<IFavoritesStorageEngine, FavoritesStorageEngine>();
        services.AddScoped<IFavoritesService, FavoritesService>();

        return services;
    }
}
