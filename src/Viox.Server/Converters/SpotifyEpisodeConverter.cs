using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Server.Converters;

public sealed class SpotifyEpisodeConverter : IMediaMetaDataConverter<SpotifyEpisode>
{
    public string Source => "spotify";
    public string Type => "episode";

    public MediaMetaData Convert(object input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Convert((SpotifyEpisode)input);
    }

    public MediaMetaData Convert(SpotifyEpisode episode)
    {
        ArgumentNullException.ThrowIfNull(episode);

        return new MediaMetaData
        {
            Uri = episode.Uri?.ParseMediaUri(),
            Title = episode.Name ?? string.Empty,
            Album = episode.Description ?? string.Empty,
            Artist = string.Empty,
            Url = episode.Href ?? string.Empty,
            ImageUrl = SpotifyConverterHelpers.GetSpotifyImageUrl(episode.Images)
        };
    }
}
