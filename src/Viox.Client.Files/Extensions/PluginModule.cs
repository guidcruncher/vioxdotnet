namespace Viox.Client.Files.Extensions;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Files.Services;
using Viox.Core.Plugins;

public class PluginModule : IPluginModule
{
    public async Task ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddKeyedSingleton<IMediaSource, FileMediaSource>("file");

        services.AddFileScanner(configuration);
        services.AddM3uPlaylistParser(configuration);
        services.AddM3uPlaylistProvider();
    }
}
