namespace Viox.Client.Spotify.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddFileAuthStore(options =>
        {
            options.FilePath = "/data/auth_token.json";
            options.CreateDirectoryIfNotExists = true;
        });

        services.AddSingleton<SpotifyPagingHelper>();
        services.AddSpotifyClient(configuration);
        services.AddSpotifyAuthServices(configuration);
    }
}
