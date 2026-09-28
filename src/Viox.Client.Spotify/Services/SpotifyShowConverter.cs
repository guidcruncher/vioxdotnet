using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.Spotify.Services;

public sealed class SpotifyShowConverter : IMediaMetaDataConverter<SpotifyShow>
{
    public string Source => "spotify";
    public string Type => "show";

    private readonly IFavoritesService _favourites;

    public SpotifyShowConverter(IFavoritesService favourites)
    {
        _favourites = favourites;
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
        metaData.Favourite = _favourites.Exists(metaData.RawUri);
        return metaData;
    }
}
