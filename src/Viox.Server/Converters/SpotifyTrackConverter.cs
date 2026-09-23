using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Services;
using Viox.Server.Services;

namespace Viox.Server.Converters;

public sealed class SpotifyTrackConverter : IMediaMetaDataConverter<SpotifyTrack>
{
    public string Source => "spotify";
    public string Type => "track";

    private readonly IFavoritesService _favourites;

    public SpotifyTrackConverter(IFavoritesService favourites)
    {
        _favourites = favourites;
    }

    public MediaMetaData Convert(object input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Convert((SpotifyTrack)input);
    }

    public MediaMetaData Convert(SpotifyTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);

        MediaMetaData metaData = new()
        {
            Uri = track.Uri?.ParseMediaUri(),
            Title = track.Name ?? string.Empty,
            Album = track.Album?.Name ?? string.Empty,
            Artist = SpotifyConverterHelpers.GetArtists(track.Artists),
            Url = track.Href ?? string.Empty,
            ImageUrl = SpotifyConverterHelpers.GetSpotifyImageUrl(track.Album?.Images),
            Duration = (track.DurationMs / 1000),
            ReleaseDate = SpotifyConverterHelpers.ParseSpotifyDate(track.ReleaseDate, track.ReleaseDatePrecision)
        };
        metaData.Favourite = _favourites.Exists(metaData.RawUri);
        return metaData;
    }
}
