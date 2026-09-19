using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Server.Converters;

public sealed class SpotifyAlbumConverter : IMediaMetaDataConverter<SpotifyAlbum>
{
    public string Source => "spotify";
    public string Type => "album";

    public MediaMetaData Convert(object input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Convert((SpotifyAlbum)input);
    }

    public MediaMetaData Convert(SpotifyAlbum album)
    {
        ArgumentNullException.ThrowIfNull(album);

        return new MediaMetaData
        {
            Uri = album.Uri?.ParseMediaUri(),
            Title = album.Name ?? string.Empty,
            Album = album.Name ?? string.Empty,
            Artist = SpotifyConverterHelpers.GetArtists(album.Artists),
            Url = album.Href ?? string.Empty,
            ImageUrl = SpotifyConverterHelpers.GetSpotifyImageUrl(album.Images)
        };
    }
}
