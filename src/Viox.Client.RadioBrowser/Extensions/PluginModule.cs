namespace Viox.Client.RadioBrowser.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Viox.Client.RadioBrowser.Configuration;
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

        AddRadioBrowserClient(services, options =>
        {
            options.UserAgent = "Viox.net";
            // BaseAddress will be filled in later
        });
    }

    private IServiceCollection AddRadioBrowserClient(
        IServiceCollection services,
        Action<RadioBrowserOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddOptions<RadioBrowserOptions>();
        }
        services.AddHttpClient<IRadioBrowserClient, RadioBrowserClient>((httpClient, provider) =>
            {
                var options = provider.GetRequiredService<IOptions<RadioBrowserOptions>>().Value;
                httpClient.BaseAddress = options.BaseAddress ?? options.FallbackBaseAddress;
                httpClient.Timeout = options.Timeout;
                return new RadioBrowserClient(httpClient, options);
            });
        return services;
    }

}
