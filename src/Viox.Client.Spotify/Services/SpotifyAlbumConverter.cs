using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.Spotify.Services;

public sealed class SpotifyAlbumConverter : MediaMetaDataConverterBase, IMediaMetaDataConverter<SpotifyAlbum>
{
    public string Source => "spotify";
    public string Type => "album";


    public SpotifyAlbumConverter() : base()
    {
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
        return Decorate(metaData);
    }
}
