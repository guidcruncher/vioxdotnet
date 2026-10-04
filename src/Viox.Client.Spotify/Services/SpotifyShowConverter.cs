using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Playlists;
using Viox.Core.Services;

namespace Viox.Client.Spotify.Services;

public sealed class SpotifyShowConverter : MediaMetaDataConverterBase, IMediaMetaDataConverter<SpotifyShow>
{
    public string Source => "spotify";
    public string Type => "show";


    public SpotifyShowConverter(IPlaylistIndexService indexer, IFavoritesService favourites) : base(indexer, favourites)
    {
    }

    public MediaMetaData Convert(object input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Convert((SpotifyShow)input);
    }

    public MediaMetaData Convert(SpotifyShow show)
    {
        ArgumentNullException.ThrowIfNull(show);

        MediaMetaData metaData = new()
        {
            Uri = show.Uri?.ParseMediaUri(),
            Title = show.Name ?? string.Empty,
            Album = show.Description ?? string.Empty,
            Artist = show.Publisher ?? string.Empty,
            Url = show.Href ?? string.Empty,
            ImageUrl = SpotifyConverterHelpers.GetSpotifyImageUrl(show.Images)
        };
        return Decorate(metaData);
    }
}
