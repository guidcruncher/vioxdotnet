namespace Viox.Core.Services;

using System;
using System.Globalization;

/// <summary>
/// Handles parsing of RSS item duration properties into total seconds.
/// </summary>
public static class RssDurationParser
{
    public static double ParseToSeconds(string? rawDuration)
    {
        if (TryParseToSeconds(rawDuration, out double totalSeconds))
        {
            return totalSeconds;
        }

        return 0;
    }

    public static bool TryParseToSeconds(string? rawDuration, out double totalSeconds)
    {
        totalSeconds = 0.0;

        if (string.IsNullOrWhiteSpace(rawDuration))
        {
            return false;
        }

        ReadOnlySpan<char> trimmed = rawDuration.AsSpan().Trim();

        if (!trimmed.Contains(':'))
        {
            if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out double rawSeconds))
            {
                if (rawSeconds < 0)
                {
                    return false;
                }

                totalSeconds = rawSeconds;
                return true;
            }

            return false;
        }

        string[] parts = trimmed.ToString().Split(':');
        if (parts.Length is < 2 or > 4)
        {
            return false;
        }

        try
        {
            double calculatedSeconds = 0.0;

            if (parts.Length == 2)
            {
                if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double minutes) &&
                    double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
                {
                    calculatedSeconds = (minutes * 60.0) + seconds;
                }
                else
                {
                    return false;
                }
            }
            else if (parts.Length == 3)
            {
                if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double hours) &&
                    double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double minutes) &&
                    double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
                {
                    calculatedSeconds = (hours * 3600.0) + (minutes * 60.0) + seconds;
                }
                else
                {
                    return false;
                }
            }
            else if (parts.Length == 4)
            {
                if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double days) &&
                    double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double hours) &&
                    double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double minutes) &&
                    double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
                {
                    calculatedSeconds = (days * 86400.0) + (hours * 3600.0) + (minutes * 60.0) + seconds;
                }
                else
                {
                    return false;
                }
            }

            if (calculatedSeconds < 0)
            {
                return false;
            }

            totalSeconds = calculatedSeconds;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
