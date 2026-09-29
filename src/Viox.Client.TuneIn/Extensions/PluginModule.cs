namespace Viox.Client.TuneIn.Extensions;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.TuneIn.Services;
using Viox.Core.Plugins;
using Viox.Core.Services;

public class PluginModule : IPluginModule
{
    public async Task ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, TuneInStationDetailConverter>("tunein:station");
        services.AddKeyedSingleton<IMediaSource, TuneInMediaSource>("tunein");

        services.AddTuneInClient(configuration);
    }
}
