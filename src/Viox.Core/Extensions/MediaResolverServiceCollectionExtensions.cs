namespace Viox.Core.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Services;

public static class MediaResolverServiceCollectionExtensions
{

    public static IServiceCollection AddMediaResolver(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<MediaMetaDataConverterResolver>();
        services.AddSingleton<MediaSourceResolverService>();

        return services;
    }

}
