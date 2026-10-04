using Viox.Core.Playlists;
using Viox.Client.Youtube.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.Youtube.Services;

public sealed class YoutubeConverter : MediaMetaDataConverterBase, IMediaMetaDataConverter<YoutubeTrack>
{

    public string Source { get => "youtube"; }
    public string Type { get => "track"; }


    public YoutubeConverter(): base()
    {
    }

    public MediaMetaData Convert(object input)
    {
        return Convert((YoutubeTrack)input);
    }

    public MediaMetaData Convert(YoutubeTrack input)
    {
        MediaMetaData metaData = new()
        {
            Uri = input.Uri,
            Title = input.Title ?? string.Empty,
            Album = input.Album ?? string.Empty,
            Artist = input.Artists is not null ? string.Join(", ", input.Artists) : string.Empty,
            Url = input.MpdUri,
            ImageUrl = input.ImageUrl ?? string.Empty,
            Duration = input.Duration?.TotalSeconds
        };

        metaData.Favourite = _favourites.Exists(metaData.RawUri);
        return Decorate(metaData);
    }

}
