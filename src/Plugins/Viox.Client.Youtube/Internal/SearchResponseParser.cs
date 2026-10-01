// /home/jcrocker/src/viox.net/src/Plugins/Viox.Client.Youtube/Internal/SearchResponseParser.cs
using System.Globalization;
using System.Text.Json;
using Viox.Core.Models;
using Viox.Core.Services;
using Viox.Client.Youtube.Models;

namespace Viox.Client.Youtube.Internal;

internal static class SearchResponseParser
{
    public static IReadOnlyList<YoutubeTrack> ParseSearch(JsonElement root)
    {
        var results = new List<YoutubeTrack>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in root.Descendants())
        {
            if (node.ValueKind != JsonValueKind.Object)
            {
                continue;
            }
            if (node.TryGetProperty("musicResponsiveListItemRenderer", out var listItem))
            {
                var parsed = ParseListItem(listItem);
                if (parsed is not null && seen.Add(parsed.VideoId))
                {
                    results.Add(parsed);
                }
            }
            else if (node.TryGetProperty("musicCardShelfRenderer", out var card))
            {
                var parsed = ParseCard(card);
                if (parsed is not null && seen.Add(parsed.VideoId))
                {
                    results.Insert(0, parsed);
                }
            }
        }
        return results;
    }

    public static YoutubeTrack? ParseWatchNext(string videoId, JsonElement root)
    {
        foreach (var node in root.Descendants())
        {
            if (node.ValueKind != JsonValueKind.Object ||
                !node.TryGetProperty("playlistPanelVideoRenderer", out var panel))
            {
                continue;
            }
            var id = panel.GetPropertyOrNull("videoId")?.GetString()
                     ?? panel.GetPropertyOrNull("navigationEndpoint")
                         ?.GetPropertyOrNull("watchEndpoint")
                         ?.GetPropertyOrNull("videoId")
                         ?.GetString();
            if (!string.Equals(id, videoId, StringComparison.Ordinal))
            {
                continue;
            }
            var title = panel.GetPropertyOrNull("title").ReadRunsText() ?? videoId;
            var artists = ExtractArtists(panel.GetPropertyOrNull("longBylineText") ?? panel.GetPropertyOrNull("shortBylineText"));
            var duration = DurationParser.TryParse(panel.GetPropertyOrNull("lengthText").ReadRunsText());
            var image = BestThumbnail(panel.GetPropertyOrNull("thumbnail")?.GetPropertyOrNull("thumbnails"));
            var (album, albumId) = ExtractAlbum(panel.GetPropertyOrNull("longBylineText"));
            var uri = $"youtube:track:{videoId}";
            return new YoutubeTrack
            {
                Uri = MediaUriParser.ParseMediaUriValue(uri),
                VideoId = videoId,
                Title = title,
                Album = album,
                Artists = artists,
                Duration = duration,
                ImageUrl = image,
                TrackUrl = VideoIdParser.ToTrackUrl(videoId),
                AlbumBrowseId = albumId,
                Kind = InferKind(panel)
            };
        }
        return null;
    }

    public static (string? Title, string? Author, TimeSpan? Duration, DateOnly? Published, string? ImageUrl, string? StreamUrl)
        ParsePlayer(JsonElement root)
    {
        var details = root.GetPropertyOrNull("videoDetails");
        var title = details?.GetPropertyOrNull("title")?.GetString();
        var author = details?.GetPropertyOrNull("author")?.GetString();
        TimeSpan? duration = null;
        if (int.TryParse(details?.GetPropertyOrNull("lengthSeconds")?.GetStringOrNull(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
        {
            duration = TimeSpan.FromSeconds(seconds);
        }
        var thumbnails = details?.GetPropertyOrNull("thumbnail")?.GetPropertyOrNull("thumbnails");
        var image = BestThumbnail(thumbnails);
        var micro = root.GetPropertyOrNull("microformat")?.GetPropertyOrNull("playerMicroformatRenderer");
        var publishedText = micro?.GetPropertyOrNull("publishDate")?.GetString()
                            ?? micro?.GetPropertyOrNull("uploadDate")?.GetString();
        DateOnly? published = null;
        if (DateOnly.TryParse(publishedText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            published = parsedDate;
        }
        var streamUrl = SelectAudioStreamUrl(root.GetPropertyOrNull("streamingData"));
        return (title, author, duration, published, image, streamUrl);
    }

    public static DateOnly? ParseAlbumYear(JsonElement root)
    {
        foreach (var node in root.Descendants())
        {
            if (node.ValueKind != JsonValueKind.Object)
            {
                continue;
            }
            if (node.TryGetProperty("subtitle", out var subtitle))
            {
                foreach (var (text, _, _, _) in ((JsonElement?)subtitle).ReadRuns())
                {
                    if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year) &&
                        year is >= 1900 and <= 2100)
                    {
                        return new DateOnly(year, 1, 1);
                    }
                }
            }
            if (node.TryGetProperty("year", out var yearNode))
            {
                var text = yearNode.ValueKind == JsonValueKind.Object
                    ? ((JsonElement?)yearNode).ReadRunsText()
                    : yearNode.GetStringOrNull();
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year) &&
                    year is >= 1900 and <= 2100)
                {
                    return new DateOnly(year, 1, 1);
                }
            }
        }
        return null;
    }

    public static string? FindChipParams(JsonElement root, string chipTitle)
    {
        foreach (var node in root.Descendants())
        {
            if (node.ValueKind != JsonValueKind.Object ||
                !node.TryGetProperty("chipCloudChipRenderer", out var chip))
            {
                continue;
            }
            var title = chip.GetPropertyOrNull("text").ReadRunsText();
            if (!string.Equals(title, chipTitle, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            return chip.GetPropertyOrNull("navigationEndpoint")
                ?.GetPropertyOrNull("searchEndpoint")
                ?.GetPropertyOrNull("params")
                ?.GetString();
        }
        return null;
    }

    private static YoutubeTrack? ParseListItem(JsonElement item)
    {
        var videoId = item.GetPropertyOrNull("playlistItemData")?.GetPropertyOrNull("videoId")?.GetString()
                      ?? FindFirstVideoId(item);
        if (string.IsNullOrWhiteSpace(videoId))
        {
            return null;
        }
        var columns = item.GetPropertyOrNull("flexColumns");
        var title = ReadColumnText(columns, 0) ?? videoId;
        var subtitleRuns = ReadColumnRuns(columns, 1);
        var artists = new List<string>();
        string? album = null;
        string? albumId = null;
        TimeSpan? duration = null;
        DateOnly? releaseDate = null;
        var kind = YoutubeMusicResultKind.Song;
        foreach (var (text, pageType, browseId, _) in subtitleRuns)
        {
            var trimmed = text.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed is "•" or "·")
            {
                continue;
            }
            if (DurationParser.LooksLikeDuration(trimmed))
            {
                duration = DurationParser.TryParse(trimmed);
                continue;
            }
            if (LooksLikeViews(trimmed) || LooksLikeLikes(trimmed))
            {
                continue;
            }
            if (string.Equals(pageType, "MUSIC_PAGE_TYPE_ALBUM", StringComparison.OrdinalIgnoreCase))
            {
                album = trimmed;
                albumId = browseId;
                continue;
            }
            if (string.Equals(pageType, "MUSIC_PAGE_TYPE_ARTIST", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(pageType, "MUSIC_PAGE_TYPE_USER_CHANNEL", StringComparison.OrdinalIgnoreCase))
            {
                artists.Add(trimmed);
                continue;
            }
            if (string.Equals(trimmed, "Song", StringComparison.OrdinalIgnoreCase))
            {
                kind = YoutubeMusicResultKind.Song;
                continue;
            }
            if (string.Equals(trimmed, "Video", StringComparison.OrdinalIgnoreCase))
            {
                kind = YoutubeMusicResultKind.Video;
                continue;
            }
            if (string.Equals(trimmed, "Episode", StringComparison.OrdinalIgnoreCase))
            {
                kind = YoutubeMusicResultKind.Episode;
                continue;
            }
            if (TryParseLooseDate(trimmed, out var date))
            {
                releaseDate = date;
                continue;
            }
            if (artists.Count == 0 && pageType is null && album is null && !LooksLikeCategory(trimmed))
            {
                artists.Add(trimmed);
            }
        }
        var image = BestThumbnail(
            item.GetPropertyOrNull("thumbnail")
                ?.GetPropertyOrNull("musicThumbnailRenderer")
                ?.GetPropertyOrNull("thumbnail")
                ?.GetPropertyOrNull("thumbnails"));
        var isExplicit = item.ToString().Contains("MUSIC_EXPLICIT_BADGE", StringComparison.Ordinal);
        var uri = $"youtube:track:{videoId}";
        return new YoutubeTrack
        {
                Uri = MediaUriParser.ParseMediaUriValue(uri),
            VideoId = videoId,
            Title = title,
            Album = album,
            Artists = artists,
            ReleaseDate = releaseDate,
            Duration = duration,
            ImageUrl = image,
            TrackUrl = VideoIdParser.ToTrackUrl(videoId),
            AlbumBrowseId = albumId,
            Kind = kind,
            IsExplicit = isExplicit
        };
    }

    private static YoutubeTrack? ParseCard(JsonElement card)
    {
        var videoId = card.GetPropertyOrNull("title")
                          ?.GetPropertyOrNull("runs")
                          ?.EnumerateArray()
                          .Select(r => r.GetPropertyOrNull("navigationEndpoint")
                              ?.GetPropertyOrNull("watchEndpoint")
                              ?.GetPropertyOrNull("videoId")
                              ?.GetString())
                          .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id))
                      ?? FindFirstVideoId(card);
        if (string.IsNullOrWhiteSpace(videoId))
        {
            return null;
        }
        var title = card.GetPropertyOrNull("title").ReadRunsText() ?? videoId;
        var subtitleRuns = ((JsonElement?)card.GetPropertyOrNull("subtitle")).ReadRuns();
        var artists = ExtractArtists(card.GetPropertyOrNull("subtitle"));
        var duration = subtitleRuns
            .Select(r => DurationParser.TryParse(r.Text.Trim()))
            .FirstOrDefault(d => d is not null);
        var image = BestThumbnail(
            card.GetPropertyOrNull("thumbnail")
                ?.GetPropertyOrNull("musicThumbnailRenderer")
                ?.GetPropertyOrNull("thumbnail")
                ?.GetPropertyOrNull("thumbnails"));
        var kind = YoutubeMusicResultKind.Video;
        foreach (var (text, _, _, _) in subtitleRuns)
        {
            if (string.Equals(text.Trim(), "Song", StringComparison.OrdinalIgnoreCase))
            {
                kind = YoutubeMusicResultKind.Song;
            }
        }
        var uri = $"youtube:track:{videoId}";
        return new YoutubeTrack
        {
                Uri = MediaUriParser.ParseMediaUriValue(uri),
            VideoId = videoId,
            Title = title,
            Artists = artists,
            Duration = duration,
            ImageUrl = image,
            TrackUrl = VideoIdParser.ToTrackUrl(videoId),
            Kind = kind
        };
    }

    private static IReadOnlyList<string> ExtractArtists(JsonElement? byline)
    {
        var artists = new List<string>();
        foreach (var (text, pageType, _, _) in byline.ReadRuns())
        {
            var trimmed = text.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed is "•" or "·")
            {
                continue;
            }
            if (string.Equals(pageType, "MUSIC_PAGE_TYPE_ARTIST", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(pageType, "MUSIC_PAGE_TYPE_USER_CHANNEL", StringComparison.OrdinalIgnoreCase))
            {
                artists.Add(trimmed);
            }
        }
        if (artists.Count == 0)
        {
            foreach (var (text, pageType, _, _) in byline.ReadRuns())
            {
                var trimmed = text.Trim();
                if (pageType is null &&
                    !string.IsNullOrEmpty(trimmed) &&
                    trimmed is not ("•" or "·") &&
                    !LooksLikeViews(trimmed) &&
                    !LooksLikeLikes(trimmed) &&
                    !DurationParser.LooksLikeDuration(trimmed) &&
                    !LooksLikeCategory(trimmed))
                {
                    artists.Add(trimmed);
                    break;
                }
            }
        }
        return artists;
    }

    private static (string? Album, string? AlbumId) ExtractAlbum(JsonElement? byline)
    {
        foreach (var (text, pageType, browseId, _) in byline.ReadRuns())
        {
            if (string.Equals(pageType, "MUSIC_PAGE_TYPE_ALBUM", StringComparison.OrdinalIgnoreCase))
            {
                return (text.Trim(), browseId);
            }
        }
        return (null, null);
    }

    private static YoutubeMusicResultKind InferKind(JsonElement panel)
    {
        var type = panel.GetPropertyOrNull("navigationEndpoint")
            ?.GetPropertyOrNull("watchEndpoint")
            ?.GetPropertyOrNull("watchEndpointMusicSupportedConfigs")
            ?.GetPropertyOrNull("watchEndpointMusicConfig")
            ?.GetPropertyOrNull("musicVideoType")
            ?.GetString();
        return type switch
        {
            "MUSIC_VIDEO_TYPE_ATV" => YoutubeMusicResultKind.Song,
            "MUSIC_VIDEO_TYPE_OMV" or "MUSIC_VIDEO_TYPE_UGC" => YoutubeMusicResultKind.Video,
            _ => YoutubeMusicResultKind.Song
        };
    }

    private static string? ReadColumnText(JsonElement? columns, int index)
    {
        var runs = ReadColumnRuns(columns, index);
        return runs.Count == 0 ? null : string.Concat(runs.Select(r => r.Text));
    }

    private static IReadOnlyList<(string Text, string? PageType, string? BrowseId, string? VideoId)> ReadColumnRuns(
        JsonElement? columns,
        int index)
    {
        if (columns is null || columns.Value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }
        var array = columns.Value.EnumerateArray().ToList();
        if (index >= array.Count)
        {
            return [];
        }
        var textNode = array[index]
            .GetPropertyOrNull("musicResponsiveListItemFlexColumnRenderer")
            ?.GetPropertyOrNull("text");
        return textNode.ReadRuns();
    }

    private static string? FindFirstVideoId(JsonElement element)
    {
        foreach (var node in element.Descendants())
        {
            if (node.ValueKind == JsonValueKind.Object &&
                node.TryGetProperty("videoId", out var id) &&
                id.ValueKind == JsonValueKind.String)
            {
                var value = id.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }
        return null;
    }

    private static string? BestThumbnail(JsonElement? thumbnails)
    {
        if (thumbnails is null || thumbnails.Value.ValueKind != JsonValueKind.Array)
        {
            return null;
        }
        string? best = null;
        var bestArea = -1;
        foreach (var thumb in thumbnails.Value.EnumerateArray())
        {
            var url = thumb.GetPropertyOrNull("url")?.GetString();
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }
            var width = thumb.GetPropertyOrNull("width")?.GetInt32() ?? 0;
            var height = thumb.GetPropertyOrNull("height")?.GetInt32() ?? 0;
            var area = width * height;
            if (area >= bestArea)
            {
                bestArea = area;
                best = url;
            }
        }
        return best;
    }

    private static string? SelectAudioStreamUrl(JsonElement? streamingData)
    {
        if (streamingData is null)
        {
            return null;
        }
        JsonElement? best = null;
        var bestBitrate = -1;
        foreach (var name in new[] { "adaptiveFormats", "formats" })
        {
            var formats = streamingData.Value.GetPropertyOrNull(name);
            if (formats is null || formats.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }
            foreach (var format in formats.Value.EnumerateArray())
            {
                var mime = format.GetPropertyOrNull("mimeType")?.GetString() ?? string.Empty;
                if (!mime.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var bitrate = format.GetPropertyOrNull("bitrate")?.GetInt32() ?? 0;
                var url = format.GetPropertyOrNull("url")?.GetString();
                if (string.IsNullOrWhiteSpace(url))
                {
                    continue;
                }
                if (bitrate > bestBitrate)
                {
                    bestBitrate = bitrate;
                    best = format;
                }
            }
        }
        return best?.GetPropertyOrNull("url")?.GetString();
    }

    private static bool LooksLikeViews(string text) =>
        text.Contains("view", StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeLikes(string text) =>
        text.Contains("like", StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeCategory(string text) =>
        text is "Song" or "Songs" or "Video" or "Videos" or "Album" or "Albums"
            or "Artist" or "Playlist" or "Episode" or "Podcast" or "Profile";

    private static bool TryParseLooseDate(string text, out DateOnly date)
    {
        if (DateOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return true;
        }
        if (DateOnly.TryParseExact(text, ["MMM d, yyyy", "MMM dd, yyyy", "d MMM yyyy", "yyyy"],
                CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return true;
        }
        date = default;
        return false;
    }
}
