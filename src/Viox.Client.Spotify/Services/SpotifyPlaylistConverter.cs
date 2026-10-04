using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Playlists;
using Viox.Core.Services;

namespace Viox.Client.Spotify.Services;

public sealed class SpotifyPlaylistConverter : MediaMetaDataConverterBase, IMediaMetaDataConverter<SpotifyPlaylist>
{
    public string Source => "spotify";
    public string Type => "playlist";


    public SpotifyPlaylistConverter(IPlaylistIndexService indexer, IFavoritesService favourites) : base(indexer, favourites)
    {
    }

    public MediaMetaData Convert(object input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Convert((SpotifyPlaylist)input);
    }

    public MediaMetaData Convert(SpotifyPlaylist playlist)
    {
        ArgumentNullException.ThrowIfNull(playlist);

        MediaMetaData metaData = new()
        {
            Uri = playlist.Uri?.ParseMediaUri(),
            Title = playlist.Name ?? string.Empty,
            Album = playlist.Description ?? string.Empty,
            Artist = playlist.Owner?.DisplayName ?? string.Empty,
            Url = playlist.Href ?? string.Empty,
            ImageUrl = SpotifyConverterHelpers.GetSpotifyImageUrl(playlist.Images)
        };
        return Decorate(metaData);
    }
}
