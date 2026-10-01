using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Viox.Client.Youtube.Configuration;
using Viox.Client.Youtube.Internal;
using Viox.Client.Youtube.Services;


namespace Viox.Client.Youtube.Extensions;

public static class YoutubeMusicServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IYoutubeMusicClient"/> plus HttpClient, options and logging dependencies.
    /// </summary>
    public static IServiceCollection AddYoutubeMusic(
        this IServiceCollection services,
        Action<YoutubeMusicOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddOptions<YoutubeMusicOptions>();
        }

        services.AddHttpClient<InnerTubeClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.All,
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            });

        services.TryAddSingleton<YtDlpStreamResolver>();
        services.TryAddTransient<IYoutubeMusicClient, YoutubeMusicClient>();
        return services;
    }
}
