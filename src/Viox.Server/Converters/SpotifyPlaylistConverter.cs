using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Server.Converters;

public sealed class SpotifyPlaylistConverter : IMediaMetaDataConverter<SpotifyPlaylist>
{
    public string Source => "spotify";
    public string Type => "playlist";

    public MediaMetaData Convert(object input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Convert((SpotifyPlaylist)input);
    }

    public MediaMetaData Convert(SpotifyPlaylist playlist)
    {
        ArgumentNullException.ThrowIfNull(playlist);

        return new MediaMetaData
        {
            Uri = playlist.Uri?.ParseMediaUri(),
            Title = playlist.Name ?? string.Empty,
            Album = playlist.Description ?? string.Empty,
            Artist = playlist.Owner?.DisplayName ?? string.Empty,
            Url = playlist.Href ?? string.Empty,
            ImageUrl = SpotifyConverterHelpers.GetSpotifyImageUrl(playlist.Images)
        };
    }
}
