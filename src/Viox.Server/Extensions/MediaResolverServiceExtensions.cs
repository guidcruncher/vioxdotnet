namespace Viox.Server.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Services;
using Viox.Server.Converters;
using Viox.Server.Librarys;

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

        services.AddKeyedSingleton<ILibrary, PodverseLibrary>("podverse");
        services.AddKeyedSingleton<ILibrary, SpotifyLibrary>("spotify");
        services.AddKeyedSingleton<ILibrary, TuneInLibrary>("tunein");
        services.AddKeyedSingleton<ILibrary, RadioBrowserLibrary>("radiobrowser");

        services.AddSingleton<SpotifyPagingHelper>();
        services.AddSingleton<LibraryResolver>();

        return services;
    }

}
