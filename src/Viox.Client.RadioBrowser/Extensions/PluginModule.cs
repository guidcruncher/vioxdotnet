namespace Viox.Client.RadioBrowser.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.RadioBrowser.Services;
using Viox.Core.Plugins;
using Viox.Core.Services;

public class PluginModule : IPluginModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<RadioBrowserInitializer>();
        services.AddHostedService<RadioBrowserInitializerHostedService>();

        services.AddKeyedSingleton<IMediaMetaDataConverterBase, RadioBrowserStationConverter>("radiobrowser:station");
        services.AddKeyedSingleton<IMediaSource, RadioBrowserMediaSource>("radiobrowser");

        services.AddRadioBrowserClient(options =>
        {
            options.UserAgent = "Viox.net";
            // BaseAddress will be filled in later
        });
    }
}
