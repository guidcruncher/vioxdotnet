using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.Spotify.Services;

/// <summary>
/// Service responsible for mapping and flattening Spotify search results into standardized <see cref="MediaMetaData"/> items.
/// </summary>
public sealed class SpotifySearchResultConverter
{

    /// <summary>
    /// Converts a <see cref="SpotifySearchResult"/> into a flattened read-only list of <see cref="MediaMetaData"/>.
    /// </summary>
    /// <param name="searchResult">The Spotify search result payload to convert.</param>
    /// <returns>A flattened read-only list containing converted media metadata.</returns>
    public IReadOnlyList<MediaMetaData> Convert(SpotifySearchResult? searchResult)
    {
        if (searchResult is null)
        {
            return [];
        }

        var results = new List<MediaMetaData>();

        if (searchResult.Tracks?.Items is { Count: > 0 } tracks)
        {
            foreach (var track in tracks)
            {
                if (track is not null)
                {
                    results.Add(MapTrack(track));
                }
            }
        }

        if (searchResult.Albums?.Items is { Count: > 0 } albums)
        {
            foreach (var album in albums)
            {
                if (album is not null)
                {
                    results.Add(MapAlbum(album));
                }
            }
        }

        if (searchResult.Artists?.Items is { Count: > 0 } artists)
        {
            foreach (var artist in artists)
            {
                if (artist is not null)
                {
                    results.Add(MapArtist(artist));
                }
            }
        }

        if (searchResult.Playlists?.Items is { Count: > 0 } playlists)
        {
            foreach (var playlist in playlists)
            {
                if (playlist is not null)
                {
                    results.Add(MapPlaylist(playlist));
                }
            }
        }

        if (searchResult.Shows?.Items is { Count: > 0 } shows)
        {
            foreach (var show in shows)
            {
                if (show is not null)
                {
                    results.Add(MapShow(show));
                }
            }
        }

        if (searchResult.Episodes?.Items is { Count: > 0 } episodes)
        {
            foreach (var episode in episodes)
            {
                if (episode is not null)
                {
                    results.Add(MapEpisode(episode));
                }
            }
        }

        if (searchResult.Audiobooks?.Items is { Count: > 0 } audiobooks)
        {
            foreach (var audiobook in audiobooks)
            {
                if (audiobook is not null)
                {
                    results.Add(MapAudiobook(audiobook));
                }
            }
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Converts a <see cref="SpotifySearchResult"/> into a <see cref="PagedList{MediaMetaData}"/> with explicit pagination parameters.
    /// </summary>
    /// <param name="searchResult">The Spotify search result payload to convert.</param>
    /// <param name="offset">The 0-based offset starting index.</param>
    /// <param name="limit">The maximum number of items per page.</param>
    /// <returns>A paged list containing converted media metadata.</returns>
    public PagedList<MediaMetaData> ConvertToPagedList(SpotifySearchResult? searchResult, int offset, int limit)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        if (searchResult is null)
        {
            return new PagedList<MediaMetaData>([], totalCount: 0, offset: offset, limit: limit);
        }

        var items = Convert(searchResult);
        int totalCount = CalculateTotalCount(searchResult);

        return new PagedList<MediaMetaData>(items, totalCount, offset, limit);
    }

    /// <summary>
    /// Converts a <see cref="SpotifySearchResult"/> into a <see cref="PagedList{MediaMetaData}"/> by inferring offset and limit parameters from the search payload.
    /// </summary>
    /// <param name="searchResult">The Spotify search result payload to convert.</param>
    /// <returns>A paged list containing converted media metadata.</returns>
    public PagedList<MediaMetaData> ConvertToPagedList(SpotifySearchResult? searchResult)
    {
        if (searchResult is null)
        {
            return new PagedList<MediaMetaData>();
        }

        int inferredOffset = InferOffset(searchResult);
        int inferredLimit = InferLimit(searchResult);

        return ConvertToPagedList(searchResult, inferredOffset, inferredLimit);
    }

    private static int CalculateTotalCount(SpotifySearchResult searchResult)
    {
        int total = 0;

        total += searchResult.Tracks?.Total ?? 0;
        total += searchResult.Albums?.Total ?? 0;
        total += searchResult.Artists?.Total ?? 0;
        total += searchResult.Playlists?.Total ?? 0;
        total += searchResult.Shows?.Total ?? 0;
        total += searchResult.Episodes?.Total ?? 0;
        total += searchResult.Audiobooks?.Total ?? 0;

        return total;
    }

    private static int InferOffset(SpotifySearchResult searchResult)
    {
        if (searchResult.Tracks is not null) return searchResult.Tracks.Offset;
        if (searchResult.Albums is not null) return searchResult.Albums.Offset;
        if (searchResult.Artists is not null) return searchResult.Artists.Offset;
        if (searchResult.Playlists is not null) return searchResult.Playlists.Offset;
        if (searchResult.Shows is not null) return searchResult.Shows.Offset;
        if (searchResult.Episodes is not null) return searchResult.Episodes.Offset;
        if (searchResult.Audiobooks is not null) return searchResult.Audiobooks.Offset;

        return 0;
    }

    private static int InferLimit(SpotifySearchResult searchResult)
    {
        if (searchResult.Tracks?.Limit > 0) return searchResult.Tracks.Limit;
        if (searchResult.Albums?.Limit > 0) return searchResult.Albums.Limit;
        if (searchResult.Artists?.Limit > 0) return searchResult.Artists.Limit;
        if (searchResult.Playlists?.Limit > 0) return searchResult.Playlists.Limit;
        if (searchResult.Shows?.Limit > 0) return searchResult.Shows.Limit;
        if (searchResult.Episodes?.Limit > 0) return searchResult.Episodes.Limit;
        if (searchResult.Audiobooks?.Limit > 0) return searchResult.Audiobooks.Limit;

        return 20; // Standard fallback page limit for Spotify Web API
    }

    private static MediaMetaData MapTrack(SpotifyTrack track) =>
        new()
        {
            Uri = track.Uri.ParseMediaUri(),
            Title = track.Name,
            Artist = track.Artists.Count > 0 ? string.Join(", ", track.Artists.Select(a => a.Name)) : string.Empty,
            Album = track.Album?.Name ?? string.Empty,
            Url = GetUrl(track.ExternalUrls, track.Href),
            ImageUrl = track.Album?.Images.FirstOrDefault()?.Url ?? string.Empty,
            Duration = track.DurationMs > 0 ? track.DurationMs / 1000.0 : null
        };

    private static MediaMetaData MapAlbum(SpotifyAlbum album) =>
        new()
        {
            Uri = album.Uri.ParseMediaUri(),
            Title = album.Name,
            Artist = album.Artists.Count > 0 ? string.Join(", ", album.Artists.Select(a => a.Name)) : string.Empty,
            Album = album.Name,
            Url = GetUrl(album.ExternalUrls, album.Href),
            ImageUrl = album.Images.FirstOrDefault()?.Url ?? string.Empty,
            Duration = null
        };

    private static MediaMetaData MapArtist(SpotifyArtist artist) =>
        new()
        {
            Uri = artist.Uri.ParseMediaUri(),
            Title = artist.Name,
            Artist = artist.Name,
            Album = string.Empty,
            Url = GetUrl(artist.ExternalUrls, artist.Href),
            ImageUrl = artist.Images.FirstOrDefault()?.Url ?? string.Empty,
            Duration = null
        };

    private static MediaMetaData MapPlaylist(SpotifyPlaylist playlist) =>
        new()
        {
            Uri = playlist.Uri.ParseMediaUri(),
            Title = playlist.Name,
            Artist = playlist.Owner?.DisplayName ?? string.Empty,
            Album = string.Empty,
            Url = GetUrl(playlist.ExternalUrls, playlist.Href),
            ImageUrl = playlist.Images.FirstOrDefault()?.Url ?? string.Empty,
            Duration = null
        };

    private static MediaMetaData MapShow(SpotifyShow show) =>
        new()
        {
            Uri = show.Uri.ParseMediaUri(),
            Title = show.Name,
            Artist = show.Publisher,
            Album = string.Empty,
            Url = GetUrl(show.ExternalUrls, show.Href),
            ImageUrl = show.Images.FirstOrDefault()?.Url ?? string.Empty,
            Duration = null
        };

    private static MediaMetaData MapEpisode(SpotifyEpisode episode) =>
        new()
        {
            Uri = episode.Uri.ParseMediaUri(),
            Title = episode.Name,
            Artist = string.Empty,
            Album = string.Empty,
            Url = GetUrl(episode.ExternalUrls, episode.Href),
            ImageUrl = episode.Images.FirstOrDefault()?.Url ?? string.Empty,
            Duration = episode.DurationMs > 0 ? episode.DurationMs / 1000.0 : null
        };

    private static MediaMetaData MapAudiobook(SpotifyAudiobook audiobook) =>
        new()
        {
            Uri = audiobook.Uri.ParseMediaUri(),
            Title = audiobook.Name,
            Artist = audiobook.Authors.Count > 0 ? string.Join(", ", audiobook.Authors.Select(a => a.Name)) : string.Empty,
            Album = string.Empty,
            Url = GetUrl(audiobook.ExternalUrls, audiobook.Href),
            ImageUrl = audiobook.Images.FirstOrDefault()?.Url ?? string.Empty,
            Duration = null
        };

    private static string GetUrl(SpotifyExternalUrls? externalUrls, string fallbackHref)
    {
        if (externalUrls is not null && !string.IsNullOrWhiteSpace(externalUrls.Spotify))
        {
            return externalUrls.Spotify;
        }

        return fallbackHref;
    }

}
