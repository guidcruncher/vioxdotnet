using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Configuration;
using Viox.Core.Services;

namespace Viox.Core.Extensions;

public static class MemoryCacheServiceCollectionExtensions
{
    public static IServiceCollection AddMemoryCacheService<TValue>(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        services.AddSingleton<IMemoryCacheService<TValue>, MemoryCacheService<TValue>>();

        return services;
    }

    public static IServiceCollection AddMemoryCacheService<TValue>(
        this IServiceCollection services,
        Action<CacheOptions> configureOptions)
    {
        services.AddMemoryCache();
        services.Configure(configureOptions);
        services.AddSingleton<IMemoryCacheService<TValue>, MemoryCacheService<TValue>>();

        return services;
    }
}
