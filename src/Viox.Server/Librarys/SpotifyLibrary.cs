namespace Viox.Server.Librarys;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Client.Spotify.Models;
using Viox.Client.Spotify.Services;
using Viox.Core.Models;
using Viox.Core.Services;
using Viox.Server.Services;

public class SpotifyLibrary : ILibrary
{
    private readonly ISpotifyClient _client;
    private readonly ILogger<SpotifyLibrary> _logger;
    private readonly SpotifyPagingHelper _helper;
    private readonly MediaMetaDataConverterResolver _resolver;
    private readonly IMemoryCacheService<List<MediaMetaData>> _cache;
    private readonly IFavoritesService _favorites;

    public SpotifyLibrary(
        MediaMetaDataConverterResolver resolver,
    IFavoritesService favorites,
        ISpotifyClient client,
        SpotifyPagingHelper helper,
        IMemoryCacheService<List<MediaMetaData>> cache,
        ILogger<SpotifyLibrary> logger)
    {
        _favorites = favorites ?? throw new ArgumentNullException(nameof(favorites));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _helper = helper ?? throw new ArgumentNullException(nameof(helper));
    }

    public string Source => "spotify";

    public async Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
    {
        List<MediaMetaData> res = new();
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
                SpotifyResponse<SpotifyPagedResult<SpotifyPlaylist>> response =
          await _client.GetCurrentUserPlaylistsAsync(limit, offset, ct);
                return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for saved playlists.");
            }, 20, cancellationToken);
        _logger.LogInformation("Found {count} playlists", playlists.Count);

        List<SpotifySavedAlbum> albums = await _helper.FetchAllAsync<SpotifySavedAlbum>(
            async (offset, limit, ct) =>
            {
                SpotifyResponse<SpotifyPagedResult<SpotifySavedAlbum>> response =
                    await _client.GetUsersSavedAlbumsAsync(limit, offset, "", ct);

                return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for saved albums.");
            }, 20, cancellationToken);

        _logger.LogInformation("Found {count} albums", albums.Count);

        List<SpotifySavedTrack> tracks = await _helper.FetchAllAsync<SpotifySavedTrack>(
            async (offset, limit, ct) =>
            {
                SpotifyResponse<SpotifyPagedResult<SpotifySavedTrack>> response =
                    await _client.GetUsersSavedTracksAsync("", limit, offset, ct);

                return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for saved tracks.");
            }, 20, cancellationToken);

        _logger.LogInformation("Found {count} tracks", tracks.Count);

        List<SpotifySavedShow> shows = await _helper.FetchAllAsync<SpotifySavedShow>(
            async (offset, limit, ct) =>
            {
                SpotifyResponse<SpotifyPagedResult<SpotifySavedShow>> response =
                    await _client.GetUsersSavedShowsAsync(limit, offset, ct);

                return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for saved shows.");
            }, 20, cancellationToken);

        _logger.LogInformation("Found {count} shows", shows.Count);

        if (playlists is not null)
        {
            res.AddRange(_resolver.ConvertList(playlists));
        }

        if (albums is not null)
        {
            var validAlbums = albums
                .Select(a => a.Album)
                .OfType<SpotifyAlbum>()
                .Cast<dynamic>();

            res.AddRange(_resolver.ConvertList(validAlbums));
        }

        if (tracks is not null)
        {
            var validTracks = tracks
                .Select(a => a.Track)
                .OfType<SpotifyTrack>()
                .Cast<dynamic>();

            res.AddRange(_resolver.ConvertList(validTracks));
        }

        if (shows is not null)
        {
            var validShows = shows
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
}
