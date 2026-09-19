using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Server.Converters;

public sealed class SpotifyTrackConverter : IMediaMetaDataConverter<SpotifyTrack>
{
    public string Source => "spotify";
    public string Type => "track";

    public MediaMetaData Convert(object input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Convert((SpotifyTrack)input);
    }

    public MediaMetaData Convert(SpotifyTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);

        return new MediaMetaData
        {
            Uri = track.Uri?.ParseMediaUri(),
            Title = track.Name ?? string.Empty,
            Album = track.Album?.Name ?? string.Empty,
            Artist = SpotifyConverterHelpers.GetArtists(track.Artists),
            Url = track.Href ?? string.Empty,
            ImageUrl = SpotifyConverterHelpers.GetSpotifyImageUrl(track.Album?.Images)
        };
    }
}
