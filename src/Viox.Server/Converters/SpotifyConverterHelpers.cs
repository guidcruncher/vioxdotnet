namespace Viox.Server.Converters;

using System;
using System.Collections.Generic;
using System.Linq;

using Viox.Client.Spotify.Models;

/// <summary>
/// Utility helper methods for Spotify object conversions.
/// </summary>
public static class SpotifyConverterHelpers
{
    public static string GetSpotifyImageUrl(IEnumerable<SpotifyImage>? images)
    {
        if (images is null)
        {
            return string.Empty;
        }

        var largestImage = images
            .Where(img => img is not null && !string.IsNullOrWhiteSpace(img.Url))
            .OrderByDescending(img => CalculateArea(img!))
            .FirstOrDefault();

        return largestImage?.Url ?? string.Empty;
    }

    public static string GetArtists(IEnumerable<SpotifyArtist>? artists)
    {
        if (artists is null)
        {
            return string.Empty;
        }

        return string.Join(", ", artists.Where(item => item is not null && !string.IsNullOrEmpty(item.Name)).Select(item => item.Name));
    }

    private static long CalculateArea(SpotifyImage image)
    {
        int width = image.Width ?? 0;
        int height = image.Height ?? 0;

        if (width > 0 && height > 0)
        {
            return (long)width * height;
        }

        return Math.Max(width, height);
    }
}

