namespace Viox.Client.Spotify.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Client.Spotify.Models;
using Viox.Core.Models;
using Viox.Core.Services;

/// <summary>
/// Spotify implementation of <see cref="IMediaSource"/> registered under key "Spotify".
/// </summary>
public class SpotifyMediaSource : IMediaSource
{
    private readonly ILogger<SpotifyMediaSource> _logger;
    private readonly ISpotifyClient _client;

    public string Source { get => "spotify"; }

    public SpotifyMediaSource(
        ISpotifyClient client,
        ILogger<SpotifyMediaSource> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string GetSpotifyImageUrl(IEnumerable<SpotifyImage>? images)
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

    private string GetArtists(IEnumerable<SpotifyArtist>? artists)
    {
        if (artists is null)
        {
            return string.Empty;
        }

        return string.Join(", ", artists.Where(item => item is not null && !string.IsNullOrEmpty(item.Name)).Select(item => item.Name));
    }

    public async Task<MediaMetaData?> ResolveMetaData(MediaUri? uri, CancellationToken ct = default)
    {
        if (uri?.Id is null)
        {
            return null;
        }

        switch (uri.Type)
        {
            case "album":
                SpotifyResponse<SpotifyAlbum>? rAlbum = await _client.GetAlbumAsync(uri.Id, null, ct);
                SpotifyAlbum? album = rAlbum?.Data;
                if (album is null)
                {
                    _logger.LogWarning("Failed to resolve album metadata for URI: {Uri}", uri);
                    return null;
                }

                return new MediaMetaData()
                {
                    Uri = uri,
                    Title = album.Name ?? string.Empty,
                    Album = album.Name ?? string.Empty,
                    Artist = GetArtists(album.Artists),
                    Url = album.Href ?? string.Empty,
                    ImageUrl = GetSpotifyImageUrl(album.Images)
                };

            case "track":
                SpotifyResponse<SpotifyTrack>? rTrack = await _client.GetTrackAsync(uri.Id, null, ct);
                SpotifyTrack? track = rTrack?.Data;
                if (track is null)
                {
                    _logger.LogWarning("Failed to resolve track metadata for URI: {Uri}", uri);
                    return null;
                }

                return new MediaMetaData()
                {
                    Uri = uri,
                    Title = track.Name ?? string.Empty,
                    Album = track.Album?.Name ?? string.Empty,
                    Artist = GetArtists(track.Artists),
                    Url = track.Href ?? string.Empty,
                    ImageUrl = GetSpotifyImageUrl(track.Album?.Images)
                };

            case "show":
                SpotifyResponse<SpotifyShow>? rShow = await _client.GetShowAsync(uri.Id, null, ct);
                SpotifyShow? show = rShow?.Data;
                if (show is null)
                {
                    _logger.LogWarning("Failed to resolve show metadata for URI: {Uri}", uri);
                    return null;
                }

                return new MediaMetaData()
                {
                    Uri = uri,
                    Title = show.Name ?? string.Empty,
                    Album = show.Description ?? string.Empty,
                    Artist = show.Publisher ?? string.Empty,
                    Url = show.Href ?? string.Empty,
                    ImageUrl = GetSpotifyImageUrl(show.Images)
                };

            case "episode":
                SpotifyResponse<SpotifyEpisode>? rEpisode = await _client.GetEpisodeAsync(uri.Id, null, ct);
                SpotifyEpisode? episode = rEpisode?.Data;
                if (episode is null)
                {
                    _logger.LogWarning("Failed to resolve episode metadata for URI: {Uri}", uri);
                    return null;
                }

                return new MediaMetaData()
                {
                    Uri = uri,
                    Title = episode.Name ?? string.Empty,
                    Album = episode.Description ?? string.Empty,
                    Artist = string.Empty,
                    Url = episode.Href ?? string.Empty,
                    ImageUrl = GetSpotifyImageUrl(episode.Images)
                };

            case "playlist":
                SpotifyResponse<SpotifyPlaylist>? rPlaylist = await _client.GetPlaylistAsync(uri.Id, null, null, null, ct);
                SpotifyPlaylist? playlist = rPlaylist?.Data;
                if (playlist is null)
                {
                    return null;
                }

                return new MediaMetaData()
                {
                    Uri = uri,
                    Title = playlist.Name ?? string.Empty,
                    Album = playlist.Description ?? string.Empty,
                    Artist = playlist.Owner?.DisplayName ?? string.Empty,
                    Url = playlist.Href ?? string.Empty,
                    ImageUrl = GetSpotifyImageUrl(playlist.Images)
                };
        }

        return null;
    }

    public async Task<PagedList<MediaMetaData>> Query(string query, int pageNumber, int limit, CancellationToken ct = default)
    {
        SpotifyResponse<SpotifySearchResult> res = await _client.SearchAsync(query, ["album", "track"], null, limit, (pageNumber * limit), null, ct);

        if (res is null || res.Data is null)
        {
            return new PagedList<MediaMetaData>();
        }

        SpotifySearchResultConverter converter = new SpotifySearchResultConverter();

        return converter.ConvertToPagedList(res.Data, (pageNumber * limit), limit);
    }
}
