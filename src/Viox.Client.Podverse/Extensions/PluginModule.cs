namespace Viox.Client.Podverse.Extensions;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Podverse.Services;
using Viox.Core.Plugins;
using Viox.Core.Services;

public class PluginModule : IPluginModule
{
    public async Task ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, PodverseEpisodeConverter>("podverse:episode");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, PodversePodcastConverter>("podverse:podcast");
        services.AddKeyedSingleton<IMediaSource, PodverseMediaSource>("podverse");

        services.AddPodcastEpisodeParser(configuration);
        services.AddPodverseClient(configuration);
    }
}
