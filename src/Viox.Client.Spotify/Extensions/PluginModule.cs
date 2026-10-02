namespace Viox.Client.Spotify.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Spotify.Configuration;
using Viox.Client.Spotify.Services;
using Viox.Core.Plugins;
using Viox.Core.Services;

public class PluginModule : IPluginModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, SpotifyAlbumConverter>("spotify:album");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, SpotifyTrackConverter>("spotify:track");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, SpotifyShowConverter>("spotify:show");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, SpotifyEpisodeConverter>("spotify:episode");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, SpotifyPlaylistConverter>("spotify:playlist");

        services.AddKeyedSingleton<IMediaSource, SpotifyMediaSource>("spotify");

        AddFileAuthStore(services, options =>
        {
            options.FilePath = "/data/auth_token.json";
            options.CreateDirectoryIfNotExists = true;
        });

        services.AddSingleton<SpotifyPagingHelper>();
        AddSpotifyClient(services, configuration);
        AddSpotifyAuthServices(services, configuration);
    }

    private IServiceCollection AddFileAuthStore(
        IServiceCollection services,
        Action<FileAuthStoreOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddSingleton<IAuthTokenStore, FileAuthTokenStore>();

        return services;
    }

    private IServiceCollection AddSpotifyAuthServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bind Spotify options section
        services.Configure<SpotifyAuthOptions>(
            configuration.GetSection(SpotifyAuthOptions.SectionName));

        // Register HttpClient and Spotify Service via Dependency Injection
        services.AddHttpClient<ISpotifyAuthService, SpotifyAuthService>();

        return services;
    }

    private IServiceCollection AddSpotifyClient(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<SpotifyOptions>(configuration.GetSection(SpotifyOptions.SectionName));


        services.AddHttpClient<ISpotifyClient, SpotifyClient>();

        return services;
    }

}
