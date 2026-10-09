namespace Viox.Epg.Services;

using System.Globalization;

public static class EpgTimeHelper
{
    public static long ToEpoch(string xmltvDate)
    {
        if (string.IsNullOrWhiteSpace(xmltvDate))
        {
            return 0;
        }

        // Expected format: "20261007005000 +0000" or similar
        var cleanDate = xmltvDate.Trim();

        if (DateTimeOffset.TryParseExact(
            cleanDate,
            ["yyyyMMddHHmmss zzz", "yyyyMMddHHmmss", "yyyyMMddHHmmss zzzz"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out var parsed))
        {
            return parsed.ToUnixTimeSeconds();
        }

        // Fallback attempt
        if (DateTimeOffset.TryParse(cleanDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var fallbackParsed))
        {
            return fallbackParsed.ToUnixTimeSeconds();
        }

        return 0;
    }
}
