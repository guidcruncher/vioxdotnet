using System.Globalization;
using System.Text.RegularExpressions;

namespace Viox.Client.Youtube.Internal;

internal static partial class DurationParser
{
    [GeneratedRegex(@"^(?:(\d+):)?(\d{1,2}):(\d{2})$")]
    private static partial Regex ClockRegex();

    public static TimeSpan? TryParse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var value = text.Trim();
        var clock = ClockRegex().Match(value);
        if (clock.Success)
        {
            var hours = clock.Groups[1].Success ? int.Parse(clock.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
            var minutes = int.Parse(clock.Groups[2].Value, CultureInfo.InvariantCulture);
            var seconds = int.Parse(clock.Groups[3].Value, CultureInfo.InvariantCulture);
            return new TimeSpan(hours, minutes, seconds);
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var totalSeconds) &&
            totalSeconds >= 0)
        {
            return TimeSpan.FromSeconds(totalSeconds);
        }

        return null;
    }

    public static bool LooksLikeDuration(string? text) => TryParse(text) is not null;
}
