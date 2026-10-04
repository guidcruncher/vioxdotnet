using Viox.Client.Podverse.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.Podverse.Services;

public sealed class PodverseEpisodeConverter : MediaMetaDataConverterBase, IMediaMetaDataConverter<PodcastEpisode>
{

    public string Source { get => "podverse"; }
    public string Type { get => "episode"; }

    public PodverseEpisodeConverter() : base()
    {
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

        return Decorate(metaData);
    }

}
