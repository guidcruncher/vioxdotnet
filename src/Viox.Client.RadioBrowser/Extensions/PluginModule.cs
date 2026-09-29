namespace Viox.Client.RadioBrowser.Extensions;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.RadioBrowser.Services;
using Viox.Core.Plugins;
using Viox.Core.Services;

public class PluginModule : IPluginModule
{
    public async Task ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        string radioBrowserUrl = await RadioBrowserApiServerResolver.GetFastestApiUrlAsync();

        services.AddKeyedSingleton<IMediaMetaDataConverterBase, RadioBrowserStationConverter>("radiobrowser:station");
        services.AddKeyedSingleton<IMediaSource, RadioBrowserMediaSource>("radiobrowser");

        services.AddRadioBrowserClient(options =>
            {
                options.UserAgent = "Viox.net";
                options.BaseAddress = new Uri($"https://{radioBrowserUrl}");
            });
    }
}
