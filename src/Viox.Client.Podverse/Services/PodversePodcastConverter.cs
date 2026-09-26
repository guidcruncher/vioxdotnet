using Viox.Client.Podverse.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.Podverse.Services;

public sealed class PodversePodcastConverter : IMediaMetaDataConverter<Podcast>
{

    public string Source => "podverse";
    public string Type => "podcast";

    private readonly IFavoritesService _favourites;

    public PodversePodcastConverter(IFavoritesService favourites)
    {
        _favourites = favourites;
    }

    public MediaMetaData Convert(object input)
    {
        return Convert((Podcast)input);
    }

    public MediaMetaData Convert(Podcast input)
    {
        ArgumentNullException.ThrowIfNull(input);

        MediaMetaData metaData = new()
        {
            Uri = input.Uri?.ParseMediaUri(),
            Title = input.Title ?? string.Empty,
            Album = input.Subtitle ?? string.Empty,
            Artist = FormatAuthors(input.Authors),
            Url = input.LinkUrl ?? string.Empty,
            ImageUrl = input.ImageUrl ?? string.Empty
        };
        metaData.Favourite = _favourites.Exists(metaData.RawUri);
        return metaData;
    }

    private static string FormatAuthors(IEnumerable<Author>? authors)
    {
        if (authors is null)
        {
            return string.Empty;
        }

        IEnumerable<string> authorNames = authors
            .OfType<Author>()
            .Where(a => !string.IsNullOrWhiteSpace(a.Name))
            .Select(a => a.Name!);

        return string.Join(", ", authorNames);
    }
}
