namespace Viox.Server.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Extensions;
using Viox.Core.Models;

public static class CacheServiceCollectionExtensions
{

    public static IServiceCollection AddMemoryCaches(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddMemoryCacheService<List<MediaMetaData>>(configuration);

        return services;
    }
}
