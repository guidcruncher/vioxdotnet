using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Playlists;
using Viox.Core.Services;

namespace Viox.Client.Spotify.Services;

public sealed class SpotifyTrackConverter : MediaMetaDataConverterBase, IMediaMetaDataConverter<SpotifyTrack>
{
    public string Source => "spotify";
    public string Type => "track";


    public SpotifyTrackConverter(IPlaylistIndexService indexer, IFavoritesService favourites) : base(indexer, favourites)
    {
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
        return Decorate(metaData);
    }
}
