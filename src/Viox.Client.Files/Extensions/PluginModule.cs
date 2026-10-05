namespace Viox.Client.Files.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Files.Configuration;
using Viox.Client.Files.Services;
using Viox.Core.Plugins;

public class PluginModule : IPluginModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.Configure<M3uPlaylistParserOptions>(configuration.GetSection(M3uPlaylistParserOptions.SectionName));
        services.AddHttpClient<IM3uPlaylistParser, M3uPlaylistParser>();

        services.AddSingleton<IM3uPlaylistProvider, M3uPlaylistProvider>();

        services.Configure<MediaScannerOptions>(
            configuration.GetSection(MediaScannerOptions.SectionName));
        services.AddTransient<IFileScanner, FileScanner>();
        services.AddKeyedSingleton<IMediaSource, FileMediaSource>("file");
        services.AddKeyedSingleton<IMediaSource,M3uPlaylistMediaSource>("fileplaylist");
    }
}
