using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.Spotify.Services;

public sealed class SpotifyEpisodeConverter : IMediaMetaDataConverter<SpotifyEpisode>
{
    public string Source => "spotify";
    public string Type => "episode";

    private readonly IFavoritesService _favourites;

    public SpotifyEpisodeConverter(IFavoritesService favourites)
    {
        _favourites = favourites;
    }

    public MediaMetaData Convert(object input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Convert((SpotifyEpisode)input);
    }

    public MediaMetaData Convert(SpotifyEpisode episode)
    {
        ArgumentNullException.ThrowIfNull(episode);

        MediaMetaData metaData = new()
        {
            Uri = episode.Uri?.ParseMediaUri(),
            Title = episode.Name ?? string.Empty,
            Album = episode.Description ?? string.Empty,
            Artist = string.Empty,
            Url = episode.Href ?? string.Empty,
            ImageUrl = SpotifyConverterHelpers.GetSpotifyImageUrl(episode.Images),
            Duration = (episode.DurationMs / 1000),
            ReleaseDate = SpotifyConverterHelpers.ParseSpotifyDate(episode.ReleaseDate, episode.ReleaseDatePrecision)
        };
        metaData.Favourite = _favourites.Exists(metaData.RawUri);
        return metaData;
    }
}
