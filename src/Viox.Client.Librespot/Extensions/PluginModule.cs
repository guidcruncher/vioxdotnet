namespace Viox.Client.Librespot.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Viox.Client.Librespot.Configuration;
using Viox.Client.Librespot.Services;
using Viox.Core.Plugins;
using Viox.Core.Services;

public class PluginModule : IPluginModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, LibrespotConverter>("librespot:*");
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.Configure<LibrespotOptions>(configuration.GetSection(LibrespotOptions.SectionName));

        services.AddHttpClient<ILibrespotRestClient, LibrespotRestClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<LibrespotOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
        services.AddSingleton<ILibrespotWebSocketClient, LibrespotWebSocketClient>();
	services.AddSingleton<LibrespotManager>();

    }
}
