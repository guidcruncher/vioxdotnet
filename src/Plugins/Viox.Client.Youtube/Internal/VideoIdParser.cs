using System.Text.RegularExpressions;

namespace Viox.Client.Youtube.Internal;

internal static partial class VideoIdParser
{
    [GeneratedRegex(@"(?:v=|/watch\?v=|/embed/|/shorts/|/youtu\.be/)([A-Za-z0-9_-]{11})", RegexOptions.IgnoreCase)]
    private static partial Regex UrlVideoIdRegex();

    [GeneratedRegex(@"^[A-Za-z0-9_-]{11}$")]
    private static partial Regex BareVideoIdRegex();

    public static string? TryParse(string videoIdOrUrl)
    {
        if (string.IsNullOrWhiteSpace(videoIdOrUrl))
        {
            return null;
        }

        var trimmed = videoIdOrUrl.Trim();
        if (BareVideoIdRegex().IsMatch(trimmed))
        {
            return trimmed;
        }

        var match = UrlVideoIdRegex().Match(trimmed);
        return match.Success ? match.Groups[1].Value : null;
    }

    public static string ToTrackUrl(string videoId) =>
        $"https://music.youtube.com/watch?v={videoId}";
}
