using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Server.Converters;

public sealed class PodverseEpisodeConverter : IMediaMetaDataConverter<PodcastEpisode>
{

    public string Source { get => "podverse"; }
    public string Type { get => "episode"; }

    public MediaMetaData Convert(object input)
    {
        return Convert((PodcastEpisode)input);
    }

    public MediaMetaData Convert(PodcastEpisode episode)
    {
        return new MediaMetaData
        {
            Uri = episode.Uri.ParseMediaUri(),
            Title = episode.Title ?? string.Empty,
            Album = episode.Description ?? string.Empty,
            Artist = "",
            Url = episode.AudioUrl ?? string.Empty,
            ImageUrl = !string.IsNullOrEmpty(episode.ImageUrl)
                                ? episode.ImageUrl : string.Empty,
            Duration = episode.DurationSeconds ?? 0
        };
    }

}
