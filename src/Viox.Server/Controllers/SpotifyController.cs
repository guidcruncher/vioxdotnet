using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Viox.Client.Spotify.Models;
using Viox.Client.Spotify.Services;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Server.Controllers;

/// <summary>
/// Options configuration for controlling behavior in <see cref="SpotifyController"/>.
/// </summary>
public class SpotifyControllerOptions
{
    /// <summary>
    /// Gets or sets the default page size limit for paged results.
    /// </summary>
    public int DefaultLimit { get; set; } = 20;
}

/// <summary>
/// Provides REST API endpoints for accessing Spotify catalog items, user profiles, playlists, and library state.
/// </summary>
[ApiController]
[Route("api/v1/media/spotify")]
[Tags("Spotify")]
[Produces("application/json")]
public class SpotifyController : ControllerBase
{
    private readonly ISpotifyClient _spotifyClient;
    private readonly SpotifyControllerOptions _options;
    private readonly ILogger<SpotifyController> _logger;
    private readonly MediaMetaDataConverterResolver _resolver;
    private readonly SpotifyPagingHelper _helper;
    private readonly IMemoryCacheService<List<MediaMetaData>> _cache;

    public SpotifyController(
        ISpotifyClient spotifyClient,
        SpotifyPagingHelper helper,
        IOptions<SpotifyControllerOptions> options,
        MediaMetaDataConverterResolver resolver,
        IMemoryCacheService<List<MediaMetaData>> cache,
        ILogger<SpotifyController> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _spotifyClient = spotifyClient ?? throw new ArgumentNullException(nameof(spotifyClient));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _helper = helper ?? throw new ArgumentNullException(nameof(helper));
    }

    /// <summary>
    /// Retrieves catalog details for a single album by Spotify ID.
    /// </summary>
    /// <param name="id">The Spotify ID for the album.</param>
    /// <param name="market">Optional country code or 'from_token'.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Album details if found.</returns>
    [HttpGet("albums/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaAlbum>> GetAlbum(
        string id,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving album {AlbumId}", id);

        var result = await _spotifyClient.GetAlbumAsync(id, market, cancellationToken);
        if (result?.Data is null)
        {
            _logger.LogWarning("Album {AlbumId} not found", id);
            return NotFound("Album not found");
        }

        return Ok(_resolver.Convert(result.Data));
    }

    /// <summary>
    /// Retrieves tracks contained within a specific album.
    /// </summary>
    /// <param name="id">The Spotify ID for the album.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged result set of tracks.</returns>
    [HttpGet("albums/{id}/tracks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<MediaMetaData>>> GetAlbumTracks(
        string id,
        [FromQuery] string? market = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving tracks for album {AlbumId}", id);

        var result = await _spotifyClient.GetAlbumTracksAsync(
            id, market, limit ?? _options.DefaultLimit, offset, cancellationToken);

        if (result?.Data?.Items is null)
        {
            _logger.LogWarning("Tracks for album {AlbumId} not found", id);
            return NotFound();
        }

        return Ok(_resolver.ConvertList(result.Data.Items));
    }

    /// <summary>
    /// Retrieves catalog information for a single show.
    /// </summary>
    /// <param name="id">The Spotify ID for the show.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Show details if found.</returns>
    [HttpGet("shows/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaMetaData>> GetShow(
        string id,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving show {ShowId}", id);

        var result = await _spotifyClient.GetShowAsync(id, market, cancellationToken);
        if (result?.Data is null)
        {
            _logger.LogWarning("Show {ShowId} not found", id);
            return NotFound();
        }

        return Ok(_resolver.Convert(result.Data));
    }

    /// <summary>
    /// Retrieves episodes for a target show.
    /// </summary>
    /// <param name="id">The Spotify ID for the show.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged set of episodes.</returns>
    [HttpGet("shows/{id}/episodes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<MediaMetaData>>> GetShowEpisodes(
        string id,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving episodes for show {ShowId}", id);
        bool existsBefore = await _cache.ExistsAsync("spotify:show:${id}", cancellationToken);
        if (existsBefore)
        {
            List<MediaMetaData>? cachedRes = await _cache.GetAsync("spotify:show:${id}", cancellationToken);
            if (cachedRes is not null)
            {
                return Ok(cachedRes);
            }
        }

        List<SpotifyEpisode> episodes = await _helper.FetchAllAsync<SpotifyEpisode>(
              async (offset, limit, ct) =>
              {
                  SpotifyResponse<SpotifyPagedResult<SpotifyEpisode>> response = await _spotifyClient.GetShowEpisodesAsync(
              id, market, limit, offset, cancellationToken);

                  return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for episodes.");
              }, 20, cancellationToken);

        if (episodes is null || episodes.Count() == 0)
        {
            _logger.LogWarning("Episodes for show {ShowId} not found", id);
            return NotFound();
        }

        List<MediaMetaData> res = _resolver.ConvertList(episodes).ToList();
        await _cache.SetAsync("spotify:show:${id}", res, absoluteExpiration: TimeSpan.FromHours(1), cancellationToken: cancellationToken);
        return Ok(res);
    }

    /// <summary>
    /// Retrieves information for a single podcast episode.
    /// </summary>
    /// <param name="id">The Spotify ID for the episode.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Episode details if found.</returns>
    [HttpGet("episodes/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaMetaData>> GetEpisode(
        string id,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving episode {EpisodeId}", id);

        var result = await _spotifyClient.GetEpisodeAsync(id, market, cancellationToken);
        if (result?.Data is null)
        {
            _logger.LogWarning("Episode {EpisodeId} not found", id);
            return NotFound();
        }

        return Ok(_resolver.Convert(result.Data));
    }

    /// <summary>
    /// Retrieves details for a single track.
    /// </summary>
    /// <param name="id">The Spotify ID for the track.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Track details if found.</returns>
    [HttpGet("tracks/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaMetaData>> GetTrack(
        string id,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving track {TrackId}", id);

        var result = await _spotifyClient.GetTrackAsync(id, market, cancellationToken);
        if (result?.Data is null)
        {
            _logger.LogWarning("Track {TrackId} not found", id);
            return NotFound();
        }

        return Ok(_resolver.Convert(result.Data));
    }

    /// <summary>
    /// Gets detailed profile information about the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Current user profile.</returns>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyUserProfile>>> GetCurrentUserProfile(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving current user profile");

        var result = await _spotifyClient.GetCurrentUserProfileAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets details for a playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID for the playlist.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="fields">Fields filter string.</param>
    /// <param name="additionalTypes">Comma-separated item types (track, episode).</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Playlist details if found.</returns>
    [HttpGet("playlists/{playlistId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaMetaData>> GetPlaylist(
        string playlistId,
        [FromQuery] string? market = null,
        [FromQuery] string? fields = null,
        [FromQuery] string? additionalTypes = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving playlist {PlaylistId}", playlistId);

        var result = await _spotifyClient.GetPlaylistAsync(playlistId, market, fields, additionalTypes, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Playlist {PlaylistId} not found", playlistId);
            return NotFound();
        }

        if (result.Data is null)
        {
            return NotFound();
        }

        return Ok(_resolver.Convert(result.Data));
    }

    /// <summary>
    /// Gets full details of items contained in a playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID for the playlist.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="fields">Fields filter string.</param>
    /// <param name="additionalTypes">Comma-separated item types (track, episode).</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged set of playlist items.</returns>
    [HttpGet("playlists/{playlistId}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MediaMetaData>>> GetPlaylistItems(
        string playlistId,
        [FromQuery] string? market = null,
        [FromQuery] string? fields = null,
        [FromQuery] string? additionalTypes = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving items for playlist {PlaylistId}", playlistId);
        bool existsBefore = await _cache.ExistsAsync("spotify:playlist:${playlistId}", cancellationToken);
        if (existsBefore)
        {
            List<MediaMetaData>? cachedRes = await _cache.GetAsync("spotify:playlist:${playlistId}", cancellationToken);
            if (cachedRes is not null)
            {
                return Ok(cachedRes);
            }
        }
        List<SpotifyPlaylistItem> playlistItems = await _helper.FetchAllAsync<SpotifyPlaylistItem>(
              async (offset, limit, ct) =>
              {
                  var response = await _spotifyClient.GetPlaylistItemsAsync(
            playlistId, market, fields, limit, offset, additionalTypes, cancellationToken);

                  return response.Data ?? throw new InvalidOperationException("Spotify API returned a null response payload for playlist Items.");
              }, 20, cancellationToken);

        if (playlistItems is null || playlistItems.Count() == 0)
        {
            _logger.LogWarning("Items for Playlist {PlaylistId} not found", playlistId);
            return NotFound();
        }

        List<dynamic> validItems = playlistItems.Select(a => a.Item)
                    .OfType<dynamic>()
                    .Cast<dynamic>().ToList();

        List<MediaMetaData> res = _resolver.ConvertList(validItems).ToList();
        await _cache.SetAsync("spotify:show:${id}", res, absoluteExpiration: TimeSpan.FromHours(1), cancellationToken: cancellationToken);
        return Ok(res);
    }

    /// <summary>
    /// Gets playlists owned or followed by the current user.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged set of current user playlists.</returns>
    [HttpGet("me/playlists")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifyPlaylist>>>> GetCurrentUserPlaylists(
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving current user's playlists");

        var result = await _spotifyClient.GetCurrentUserPlaylistsAsync(limit ?? _options.DefaultLimit, offset, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets albums saved in the current user's library.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged set of saved albums.</returns>
    [HttpGet("me/albums")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifySavedAlbum>>>> GetUsersSavedAlbums(
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving current user's saved albums");

        var result = await _spotifyClient.GetUsersSavedAlbumsAsync(
            limit ?? _options.DefaultLimit, offset, market, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets tracks saved in the current user's library.
    /// </summary>
    /// <param name="market">Optional country code.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged set of saved tracks.</returns>
    [HttpGet("me/tracks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifySavedTrack>>>> GetUsersSavedTracks(
        [FromQuery] string? market = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving current user's saved tracks");

        var result = await _spotifyClient.GetUsersSavedTracksAsync(
            market, limit ?? _options.DefaultLimit, offset, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets podcast episodes saved in the current user's library.
    /// </summary>
    /// <param name="market">Optional country code.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged set of saved episodes.</returns>
    [HttpGet("me/episodes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifySavedEpisode>>>> GetUsersSavedEpisodes(
        [FromQuery] string? market = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving current user's saved episodes");

        var result = await _spotifyClient.GetUsersSavedEpisodesAsync(
            market, limit ?? _options.DefaultLimit, offset, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets shows saved in the current user's library.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged set of saved shows.</returns>
    [HttpGet("me/shows")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifySavedShow>>>> GetUsersSavedShows(
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving current user's saved shows");

        var result = await _spotifyClient.GetUsersSavedShowsAsync(
            limit ?? _options.DefaultLimit, offset, cancellationToken);
        return Ok(result);
    }
}
