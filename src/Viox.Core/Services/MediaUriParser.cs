// File: MediaUriParser.cs
namespace Viox.Core.Services;

using System;
using System.Linq;

using Viox.Core.Models;

/// <summary>
/// Provides extension methods for parsing custom media URI strings into structured <see cref="MediaUri"/> objects.
/// </summary>
public static class MediaUriParser
{
    private static readonly string[] validSources = ["spotify", "tunein", "radiobrowser", "podverse", "playlist"];
    private static readonly string[] validTypes = ["album", "track", "episode", "podcast", "show", "station", "playlist", "link", "media"];

    /// <summary>
    /// Parses a formatted media URI string into a <see cref="MediaUri"/> instance.
    /// </summary>
    /// <param name="uri">
    /// The colon-delimited media URI string to parse. 
    /// Expected format is <c>source:type:id</c> or <c>source:type:id:secondaryId</c>.
    /// </param>
    /// <returns>
    /// A <see cref="MediaUri"/> instance containing parsed components if valid; otherwise, <c>null</c>.
    /// </returns>
    public static MediaUri? ParseMediaUri(this string uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        string[] segments = uri.Split(":");

        if (segments.Length < 3 || segments.Length > 4)
        {
            return null;
        }

        if (!validSources.Contains(segments[0]))
        {
            return null;
        }

        if (!validTypes.Contains(segments[1]))
        {
            return null;
        }

        if (segments.Length == 4)
        {
            return new MediaUri()
            {
                Source = segments[0],
                Type = segments[1],
                Id = segments[2],
                SecondaryId = segments[3]
            };
        }

        return new MediaUri()
        {
            Source = segments[0],
            Type = segments[1],
            Id = segments[2]
        };
    }

    public static string ToString(this MediaUri uri)
    {
        if (string.IsNullOrEmpty(uri.SecondaryId))
        {
            return $"{uri.Source}:{uri.Type}:{uri.Id}";
        }

        return $"{uri.Source}:{uri.Type}:{uri.Id}:{uri.SecondaryId}";
    }

    public static MediaUri? ParseMediaUriValue(string uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        string[] segments = uri.Split(":");
        if (segments.Length < 3 || segments.Length > 4)
        {
            return null;
        }
        if (!validSources.Contains(segments[0]))
        {
            return null;
        }
        if (!validTypes.Contains(segments[1]))
        {
            return null;
        }
        if (segments.Length == 4)
        {
            return new MediaUri()
            {
                Source = segments[0],
                Type = segments[1],
                Id = segments[2],
                SecondaryId = segments[3]
            };
        }
        return new MediaUri()
        {
            Source = segments[0],
            Type = segments[1],
            Id = segments[2]
        };
    }

}
