using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Services;
using Viox.Server.Services;

namespace Viox.Server.Converters;

public sealed class SpotifyAlbumConverter : IMediaMetaDataConverter<SpotifyAlbum>
{
    public string Source => "spotify";
    public string Type => "album";

    private readonly IFavoritesService _favourites;

    public SpotifyAlbumConverter(IFavoritesService favourites)
    {
        _favourites = favourites;
    }

    public MediaMetaData Convert(object input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Convert((SpotifyAlbum)input);
    }

    public MediaMetaData Convert(SpotifyAlbum album)
    {
        ArgumentNullException.ThrowIfNull(album);

        MediaMetaData metaData = new()
        {
            Uri = album.Uri?.ParseMediaUri(),
            Title = album.Name ?? string.Empty,
            Album = album.Name ?? string.Empty,
            Artist = SpotifyConverterHelpers.GetArtists(album.Artists),
            Url = album.Href ?? string.Empty,
            ImageUrl = SpotifyConverterHelpers.GetSpotifyImageUrl(album.Images),
            Duration = 0,
            ReleaseDate = SpotifyConverterHelpers.ParseSpotifyDate(album.ReleaseDate, album.ReleaseDatePrecision)
        };
        metaData.Favourite = _favourites.Exists(metaData.RawUri);
        return metaData;
    }
}
