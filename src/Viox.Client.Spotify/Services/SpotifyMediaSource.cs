// File: SpotifyMediaSource.cs
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
    private readonly SpotifyPagingHelper _helper;
    private readonly MediaMetaDataConverterResolver _resolver;
    private readonly IMemoryCacheService<List<MediaMetaData>> _cache;
    private readonly IFavoritesService _favorites;

    public string Source => "spotify";

    public SpotifyMediaSource(
        MediaMetaDataConverterResolver resolver,
        IFavoritesService favorites,
        SpotifyPagingHelper helper,
        IMemoryCacheService<List<MediaMetaData>> cache,
        ISpotifyClient client,
        ILogger<SpotifyMediaSource> logger)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(favorites);
        ArgumentNullException.ThrowIfNull(helper);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(logger);

        _resolver = resolver;
        _favorites = favorites;
        _helper = helper;
        _cache = cache;
        _client = client;
        _logger = logger;
    }

    private string GetSpotifyImageUrl(IEnumerable<SpotifyImage>? images)
    {
        if (images is null)
        {
            return string.Empty;
        }

        SpotifyImage? largestImage = images
            .Where(img => img is not null && !string.IsNullOrWhiteSpace(img.Url))
            .OrderByDescending(CalculateArea)
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

                return new MediaMetaData
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

                return new MediaMetaData
                {
                    Uri = uri,
                    Title = track.Name ?? string.Empty,
                    Album = track.Album?.Name ?? string.Empty,
                    Artist = GetArtists(track.Artists),
                    Url = track.Href ?? string.Empty,
                    ImageUrl = GetSpotifyImageUrl(track.Album?.Images),
                    Duration = (track.DurationMs / 100)
                };

            case "show":
                SpotifyResponse<SpotifyShow>? rShow = await _client.GetShowAsync(uri.Id, null, ct);
                SpotifyShow? show = rShow?.Data;
                if (show is null)
                {
                    _logger.LogWarning("Failed to resolve show metadata for URI: {Uri}", uri);
                    return null;
                }

                return new MediaMetaData
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

                return new MediaMetaData
                {
                    Uri = uri,
                    Title = episode.Name ?? string.Empty,
                    Album = episode.Description ?? string.Empty,
                    Artist = string.Empty,
                    Url = episode.Href ?? string.Empty,
                    ImageUrl = GetSpotifyImageUrl(episode.Images),
                    Duration = (episode.DurationMs / 1000)
                };

            case "playlist":
                SpotifyResponse<SpotifyPlaylist>? rPlaylist = await _client.GetPlaylistAsync(uri.Id, null, null, null, ct);
                SpotifyPlaylist? playlist = rPlaylist?.Data;
                if (playlist is null)
                {
                    return null;
                }

                return new MediaMetaData
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
        SpotifyResponse<SpotifySearchResult> res = await _client.SearchAsync(query, ["album", "track", "playlist", "show"], null, limit, (pageNumber * limit), null, ct);

        if (res is null || res.Data is null)
        {
            return new PagedList<MediaMetaData>();
        }

        SpotifySearchResultConverter converter = new SpotifySearchResultConverter();
        return converter.ConvertToPagedList(res.Data, (pageNumber * limit), limit);
    }

    public async Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
    {
        List<MediaMetaData> res = [];
        bool existsBefore = await _cache.ExistsAsync("library.spotify", cancellationToken);

        if (existsBefore)
        {
            List<MediaMetaData>? cachedRes = await _cache.GetAsync("library.spotify", cancellationToken);
            if (cachedRes is not null)
            {
                return _favorites.SetFavoriteStates(cachedRes);
            }
        }

        List<SpotifyPlaylist> playlists = await _helper.FetchAllAsync<SpotifyPlaylist>(
            async (offset, limit, ct) =>
            {
                SpotifyResponse<SpotifyPagedResult<SpotifyPlaylist>> response = await _client.GetCurrentUserPlaylistsAsync(limit, offset, ct);
                return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for saved playlists.");
            }, 20, cancellationToken);

        _logger.LogInformation("Found {Count} playlists", playlists.Count);

        List<SpotifySavedAlbum> albums = await _helper.FetchAllAsync<SpotifySavedAlbum>(
            async (offset, limit, ct) =>
            {
                SpotifyResponse<SpotifyPagedResult<SpotifySavedAlbum>> response = await _client.GetUsersSavedAlbumsAsync(limit, offset, "", ct);
                return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for saved albums.");
            }, 20, cancellationToken);

        _logger.LogInformation("Found {Count} albums", albums.Count);

        List<SpotifySavedTrack> tracks = await _helper.FetchAllAsync<SpotifySavedTrack>(
            async (offset, limit, ct) =>
            {
                SpotifyResponse<SpotifyPagedResult<SpotifySavedTrack>> response = await _client.GetUsersSavedTracksAsync("", limit, offset, ct);
                return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for saved tracks.");
            }, 20, cancellationToken);

        _logger.LogInformation("Found {Count} tracks", tracks.Count);

        List<SpotifySavedShow> shows = await _helper.FetchAllAsync<SpotifySavedShow>(
            async (offset, limit, ct) =>
            {
                SpotifyResponse<SpotifyPagedResult<SpotifySavedShow>> response = await _client.GetUsersSavedShowsAsync(limit, offset, ct);
                return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for saved shows.");
            }, 20, cancellationToken);

        _logger.LogInformation("Found {Count} shows", shows.Count);

        if (playlists is not null)
        {
            res.AddRange(_resolver.ConvertList(playlists));
        }

        if (albums is not null)
        {
            IEnumerable<dynamic> validAlbums = albums
                .Select(a => a.Album)
                .OfType<SpotifyAlbum>()
                .Cast<dynamic>();

            res.AddRange(_resolver.ConvertList(validAlbums));
        }

        if (tracks is not null)
        {
            IEnumerable<dynamic> validTracks = tracks
                .Select(a => a.Track)
                .OfType<SpotifyTrack>()
                .Cast<dynamic>();

            res.AddRange(_resolver.ConvertList(validTracks));
        }

        if (shows is not null)
        {
            IEnumerable<dynamic> validShows = shows
                .Select(a => a.Show)
                .OfType<SpotifyShow>()
                .Cast<dynamic>();

            res.AddRange(_resolver.ConvertList(validShows));
        }

        if (res.Count > 0)
        {
            List<MediaMetaData> sorted = res
                .OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            await _cache.SetAsync("library.spotify", sorted, absoluteExpiration: TimeSpan.FromHours(1), cancellationToken: cancellationToken);
            return sorted;
        }

        return res;
    }

    public async Task<IList<MediaMetaData>> ResolveChildItems(MediaUri? parent, string childType, CancellationToken ct)
    {
        List<MediaMetaData> items = [];
        if (parent is null)
        {
            return items;
        }

        switch (parent.Type)
        {
            case "album":
                items = await GetAlbumTracks(parent.Id, ct);
                break;
            case "playlist":
                items = await GetPlaylistItems(parent.Id, ct);
                break;
            case "show":
                items = await GetShowEpisodes(parent.Id, ct);
                break;
        }

        return items;
    }

    public async Task<List<MediaMetaData>> GetAlbumTracks(
        string id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving tracks for album {AlbumId}", id);
        string cacheKey = $"spotify:album:{id}";

        bool existsBefore = await _cache.ExistsAsync(cacheKey, cancellationToken);
        if (existsBefore)
        {
            List<MediaMetaData>? cachedRes = await _cache.GetAsync(cacheKey, cancellationToken);
            if (cachedRes is not null)
            {
                return cachedRes;
            }
        }

        List<SpotifyTrack> tracks = await _helper.FetchAllAsync<SpotifyTrack>(
            async (offset, limit, ct) =>
            {
                SpotifyResponse<SpotifyPagedResult<SpotifyTrack>> response = await _client.GetAlbumTracksAsync(
                    id, null, limit, offset, ct);
                return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for tracks.");
            }, 20, cancellationToken);

        if (tracks is null || tracks.Count == 0)
        {
            return [];
        }

        List<MediaMetaData> res = _resolver.ConvertList(tracks).ToList();
        await _cache.SetAsync(cacheKey, res, absoluteExpiration: TimeSpan.FromHours(1), cancellationToken: cancellationToken);
        return res;
    }

    public async Task<List<MediaMetaData>> GetShowEpisodes(
        string id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving episodes for show {ShowId}", id);
        string cacheKey = $"spotify:show:{id}";

        bool existsBefore = await _cache.ExistsAsync(cacheKey, cancellationToken);
        if (existsBefore)
        {
            List<MediaMetaData>? cachedRes = await _cache.GetAsync(cacheKey, cancellationToken);
            if (cachedRes is not null)
            {
                return cachedRes;
            }
        }

        List<SpotifyEpisode> episodes = await _helper.FetchAllAsync<SpotifyEpisode>(
            async (offset, limit, ct) =>
            {
                SpotifyResponse<SpotifyPagedResult<SpotifyEpisode>> response = await _client.GetShowEpisodesAsync(
                    id, null, limit, offset, ct);

                return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for episodes.");
            }, 20, cancellationToken);

        if (episodes is null || episodes.Count == 0)
        {
            return [];
        }

        List<MediaMetaData> res = _resolver.ConvertList(episodes).ToList();
        await _cache.SetAsync(cacheKey, res, absoluteExpiration: TimeSpan.FromHours(1), cancellationToken: cancellationToken);
        return res;
    }

    public async Task<List<MediaMetaData>> GetPlaylistItems(
        string playlistId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving items for playlist {PlaylistId}", playlistId);
        string cacheKey = $"spotify:playlist:{playlistId}";

        bool existsBefore = await _cache.ExistsAsync(cacheKey, cancellationToken);
        if (existsBefore)
        {
            List<MediaMetaData>? cachedRes = await _cache.GetAsync(cacheKey, cancellationToken);
            if (cachedRes is not null)
            {
                return cachedRes;
            }
        }

        List<SpotifyPlaylistItem> playlistItems = await _helper.FetchAllAsync<SpotifyPlaylistItem>(
            async (offset, limit, ct) =>
            {
                SpotifyResponse<SpotifyPagedResult<SpotifyPlaylistItem>> response = await _client.GetPlaylistItemsAsync(
                    playlistId, null, null, limit, offset, null, ct);

                return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for playlist Items.");
            }, 20, cancellationToken);

        if (playlistItems is null || playlistItems.Count == 0)
        {
            _logger.LogWarning("Items for Playlist {PlaylistId} not found", playlistId);
            return [];
        }

        List<dynamic> validItems = playlistItems
            .Select(a => a.Item)
            .OfType<dynamic>()
            .ToList();

        List<MediaMetaData> res = _resolver.ConvertList(validItems).ToList();

        // Fixed: The original code was updating the "spotify:show" cache key instead of the playlist cache key
        await _cache.SetAsync(cacheKey, res, absoluteExpiration: TimeSpan.FromHours(1), cancellationToken: cancellationToken);

        return res;
    }
}
