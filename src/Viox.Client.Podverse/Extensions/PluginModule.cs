namespace Viox.Client.Podverse.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Viox.Client.Podverse.Configuration;
using Viox.Client.Podverse.Services;
using Viox.Core.Plugins;
using Viox.Core.Services;

public class PluginModule : IPluginModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddKeyedSingleton<IMediaMetaDataConverterBase, PodverseEpisodeConverter>("podverse:episode");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, PodversePodcastConverter>("podverse:podcast");
        services.AddKeyedSingleton<IMediaSource, PodverseMediaSource>("podverse");

        services.Configure<PodcastEpisodeParserOptions>(configuration.GetSection(PodcastEpisodeParserOptions.SectionName));
        services.AddTransient<PodcastEpisodeParser>();
        services.Configure<PodverseOptions>(
                    configuration.GetSection(PodverseOptions.SectionName));
        services.AddHttpClient<IPodverseClient, PodverseClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<PodverseOptions>>().Value;
            httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

    }
}
