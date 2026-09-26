using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.Podverse.Services;

public sealed class PodverseEpisodeConverter : IMediaMetaDataConverter<PodcastEpisode>
{

    public string Source { get => "podverse"; }
    public string Type { get => "episode"; }

    private readonly IFavoritesService _favourites;

    public PodverseEpisodeConverter(IFavoritesService favourites)
    {
        _favourites = favourites;
    }

    public MediaMetaData Convert(object input)
    {
        return Convert((PodcastEpisode)input);
    }

    public MediaMetaData Convert(PodcastEpisode episode)
    {
        MediaMetaData metaData = new()
        {
            Uri = episode.Uri.ParseMediaUri(),
            Title = episode.Title ?? string.Empty,
            Album = episode.Description ?? string.Empty,
            Artist = "",
            Url = episode.AudioUrl ?? string.Empty,
            ImageUrl = !string.IsNullOrEmpty(episode.ImageUrl)
                                ? episode.ImageUrl : string.Empty,
            Duration = episode.DurationSeconds ?? 0,
            ReleaseDate = episode.PublishedDate
        };
        metaData.Favourite = _favourites.Exists(metaData.RawUri);
        return metaData;
    }

}
