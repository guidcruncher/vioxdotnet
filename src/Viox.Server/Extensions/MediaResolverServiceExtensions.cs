namespace Viox.Server.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Librespot.Services;
using Viox.Client.Podverse.Services;
using Viox.Client.RadioBrowser.Services;
using Viox.Client.Spotify.Services;
using Viox.Client.TuneIn.Services;
using Viox.Core.Services;

public static class MediaResolverServiceCollectionExtensions
{

    public static IServiceCollection AddMediaResolvers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, LibrespotConverter>("librespot:*");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, PodverseEpisodeConverter>("podverse:episode");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, PodversePodcastConverter>("podverse:podcast");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, RadioBrowserStationConverter>("radiobrowser:station");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, SpotifyAlbumConverter>("spotify:album");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, SpotifyTrackConverter>("spotify:track");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, SpotifyShowConverter>("spotify:show");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, SpotifyEpisodeConverter>("spotify:episode");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, SpotifyPlaylistConverter>("spotify:playlist");
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, TuneInStationDetailConverter>("tunein:station");

        services.AddSingleton<MediaMetaDataConverterResolver>();

        services.AddKeyedSingleton<IMediaSource, M3uPlaylistMediaSource>("playlist");
        services.AddKeyedSingleton<IMediaSource, PodverseMediaSource>("podverse");
        services.AddKeyedSingleton<IMediaSource, SpotifyMediaSource>("spotify");
        services.AddKeyedSingleton<IMediaSource, TuneInMediaSource>("tunein");
        services.AddKeyedSingleton<IMediaSource, RadioBrowserMediaSource>("radiobrowser");

        services.AddSingleton<SpotifyPagingHelper>();
        services.AddSingleton<MediaSourceResolverService>();

        return services;
    }

}
