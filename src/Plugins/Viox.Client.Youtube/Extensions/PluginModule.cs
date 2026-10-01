namespace Viox.Client.Youtube.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Youtube.Configuration;
using Viox.Client.Youtube.Services;
using Viox.Core.Plugins;
using Viox.Core.Services;

public class PluginModule : IPluginModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<YoutubeMusicOptions>(
                          configuration.GetSection(YoutubeMusicOptions.SectionName));

        services.AddKeyedSingleton<IMediaMetaDataConverterBase, YoutubeConverter>("youtube:track");
        services.AddKeyedSingleton<IMediaSource, YoutubeMediaSource>("youtube");

        services.AddYoutubeMusic(options =>
        {
            options.Region = "US";
            options.EnableYtDlpFallback = true;
        });
    }
}
