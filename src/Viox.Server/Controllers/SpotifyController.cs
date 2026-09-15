using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Spotify.Models;
using Viox.Client.Spotify.Services;

namespace Viox.Server.Controllers;

/// <summary>
/// Options configuration for controlling behavior in <see cref="SpotifyController"/>.
/// </summary>
public class SpotifyControllerOptions
{
    /// <summary>
    /// Gets or sets the default market to use if non-specified in queries.
    /// </summary>
    public string DefaultMarket { get; set; } = "US";

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

    /// <summary>
    /// Initializes a new instance of the <see cref="SpotifyController"/> class.
    /// </summary>
    /// <param name="spotifyClient">The Spotify client service instance.</param>
    /// <param name="options">Options for configuring controller logic.</param>
    /// <param name="logger">The application logging context.</param>
    public SpotifyController(
        ISpotifyClient spotifyClient,
        IOptions<SpotifyControllerOptions> options,
        ILogger<SpotifyController> logger)
    {
        _spotifyClient = spotifyClient ?? throw new ArgumentNullException(nameof(spotifyClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    public async Task<ActionResult<SpotifyResponse<SpotifyAlbum>>> GetAlbum(
        string id,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving album {AlbumId}", id);

        var result = await _spotifyClient.GetAlbumAsync(id, market ?? _options.DefaultMarket, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Album {AlbumId} not found", id);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieves catalog information for several albums.
    /// </summary>
    /// <param name="ids">A collection of Spotify album IDs.</param>
    /// <param name="market">Optional country code or 'from_token'.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A wrapper containing album details.</returns>
    [HttpPost("albums/batch")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Obsolete("This endpoint is marked as deprecated by Spotify.")]
    public async Task<ActionResult<SpotifyResponse<SpotifyAlbumList>>> GetSeveralAlbums(
        [FromBody] IEnumerable<string> ids,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving multiple albums");

        var result = await _spotifyClient.GetSeveralAlbumsAsync(ids, market ?? _options.DefaultMarket, cancellationToken);
        return Ok(result);
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
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifyTrack>>>> GetAlbumTracks(
        string id,
        [FromQuery] string? market = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving tracks for album {AlbumId}", id);

        var result = await _spotifyClient.GetAlbumTracksAsync(
            id, market ?? _options.DefaultMarket, limit ?? _options.DefaultLimit, offset, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves catalog information for a single artist.
    /// </summary>
    /// <param name="id">The Spotify ID for the artist.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Artist details if found.</returns>
    [HttpGet("artists/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpotifyResponse<SpotifyArtist>>> GetArtist(
        string id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving artist {ArtistId}", id);

        var result = await _spotifyClient.GetArtistAsync(id, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Artist {ArtistId} not found", id);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieves catalog information for multiple artists.
    /// </summary>
    /// <param name="ids">A collection of Spotify artist IDs.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A wrapper containing artist details.</returns>
    [HttpPost("artists/batch")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Obsolete("This endpoint is marked as deprecated by Spotify.")]
    public async Task<ActionResult<SpotifyResponse<SpotifyArtistList>>> GetSeveralArtists(
        [FromBody] IEnumerable<string> ids,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving multiple artists");

        var result = await _spotifyClient.GetSeveralArtistsAsync(ids, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves albums published by a specific artist.
    /// </summary>
    /// <param name="id">The Spotify ID for the artist.</param>
    /// <param name="includeGroups">Comma-separated filters (e.g. album, single).</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged result set of albums.</returns>
    [HttpGet("artists/{id}/albums")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifyAlbum>>>> GetArtistAlbums(
        string id,
        [FromQuery] string? includeGroups = null,
        [FromQuery] string? market = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving albums for artist {ArtistId}", id);

        var result = await _spotifyClient.GetArtistAlbumsAsync(
            id, includeGroups, market ?? _options.DefaultMarket, limit ?? _options.DefaultLimit, offset, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves top tracks for a specific artist.
    /// </summary>
    /// <param name="id">The Spotify ID for the artist.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A wrapper containing top tracks.</returns>
    [HttpGet("artists/{id}/top-tracks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Obsolete("This endpoint is marked as deprecated by Spotify.")]
    public async Task<ActionResult<SpotifyResponse<SpotifyTrackList>>> GetArtistTopTracks(
        string id,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving top tracks for artist {ArtistId}", id);

        var result = await _spotifyClient.GetArtistTopTracksAsync(id, market ?? _options.DefaultMarket, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves artists related to a given artist.
    /// </summary>
    /// <param name="id">The Spotify ID for the artist.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A wrapper containing related artists.</returns>
    [HttpGet("artists/{id}/related-artists")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Obsolete("This endpoint is marked as deprecated by Spotify.")]
    public async Task<ActionResult<SpotifyResponse<SpotifyArtistList>>> GetArtistRelatedArtists(
        string id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving related artists for artist {ArtistId}", id);

        var result = await _spotifyClient.GetArtistRelatedArtistsAsync(id, cancellationToken);
        return Ok(result);
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
    public async Task<ActionResult<SpotifyResponse<SpotifyShow>>> GetShow(
        string id,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving show {ShowId}", id);

        var result = await _spotifyClient.GetShowAsync(id, market ?? _options.DefaultMarket, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Show {ShowId} not found", id);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieves episodes for a target show.
    /// </summary>
    /// <param name="id">The Spotify ID for the show.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged set of episodes.</returns>
    [HttpGet("shows/{id}/episodes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifyEpisode>>>> GetShowEpisodes(
        string id,
        [FromQuery] string? market = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving episodes for show {ShowId}", id);

        var result = await _spotifyClient.GetShowEpisodesAsync(
            id, market ?? _options.DefaultMarket, limit ?? _options.DefaultLimit, offset, cancellationToken);
        return Ok(result);
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
    public async Task<ActionResult<SpotifyResponse<SpotifyEpisode>>> GetEpisode(
        string id,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving episode {EpisodeId}", id);

        var result = await _spotifyClient.GetEpisodeAsync(id, market ?? _options.DefaultMarket, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Episode {EpisodeId} not found", id);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieves information for a single audiobook.
    /// </summary>
    /// <param name="id">The Spotify ID for the audiobook.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Audiobook details if found.</returns>
    [HttpGet("audiobooks/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpotifyResponse<SpotifyAudiobook>>> GetAudiobook(
        string id,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving audiobook {AudiobookId}", id);

        var result = await _spotifyClient.GetAudiobookAsync(id, market ?? _options.DefaultMarket, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Audiobook {AudiobookId} not found", id);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieves chapters for an audiobook.
    /// </summary>
    /// <param name="id">The Spotify ID for the audiobook.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged set of audiobook chapters.</returns>
    [HttpGet("audiobooks/{id}/chapters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifyChapter>>>> GetAudiobookChapters(
        string id,
        [FromQuery] string? market = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving chapters for audiobook {AudiobookId}", id);

        var result = await _spotifyClient.GetAudiobookChaptersAsync(
            id, market ?? _options.DefaultMarket, limit ?? _options.DefaultLimit, offset, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a list of audiobooks saved in the user's library.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged set of saved audiobooks.</returns>
    [HttpGet("me/audiobooks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifyAudiobook>>>> GetUsersSavedAudiobooks(
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving saved audiobooks for current user");

        var result = await _spotifyClient.GetUsersSavedAudiobooksAsync(
            limit ?? _options.DefaultLimit, offset, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves details for a specific audiobook chapter.
    /// </summary>
    /// <param name="id">The Spotify ID for the chapter.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Chapter details if found.</returns>
    [HttpGet("chapters/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpotifyResponse<SpotifyChapter>>> GetChapter(
        string id,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving chapter {ChapterId}", id);

        var result = await _spotifyClient.GetChapterAsync(id, market ?? _options.DefaultMarket, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Chapter {ChapterId} not found", id);
            return NotFound();
        }

        return Ok(result);
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
    public async Task<ActionResult<SpotifyResponse<SpotifyTrack>>> GetTrack(
        string id,
        [FromQuery] string? market = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving track {TrackId}", id);

        var result = await _spotifyClient.GetTrackAsync(id, market ?? _options.DefaultMarket, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Track {TrackId} not found", id);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Searches across Spotify catalog items matching a query string.
    /// </summary>
    /// <param name="query">The search term.</param>
    /// <param name="types">Item types to search across (e.g. track, artist, album).</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="includeExternal">Optional parameter to specify external content inclusion.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Consolidated search results.</returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifySearchResult>>> Search(
        [FromQuery, Required] string query,
        [FromQuery, Required] IEnumerable<string> types,
        [FromQuery] string? market = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        [FromQuery] string? includeExternal = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching Spotify catalog with query '{Query}'", query);

        var result = await _spotifyClient.SearchAsync(
            query, types, market ?? _options.DefaultMarket, limit ?? _options.DefaultLimit, offset, includeExternal, cancellationToken);
        return Ok(result);
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
    public async Task<ActionResult<SpotifyResponse<SpotifyPlaylist>>> GetPlaylist(
        string playlistId,
        [FromQuery] string? market = null,
        [FromQuery] string? fields = null,
        [FromQuery] string? additionalTypes = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving playlist {PlaylistId}", playlistId);

        var result = await _spotifyClient.GetPlaylistAsync(playlistId, market ?? _options.DefaultMarket, fields, additionalTypes, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Playlist {PlaylistId} not found", playlistId);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Changes details of an existing playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID for the playlist.</param>
    /// <param name="request">Playlist detail updates.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Operation result status.</returns>
    [HttpPut("playlists/{playlistId}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<object>>> ChangePlaylistDetails(
        string playlistId,
        [FromBody] ChangePlaylistDetailsRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating details for playlist {PlaylistId}", playlistId);

        var result = await _spotifyClient.ChangePlaylistDetailsAsync(playlistId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets full details of items contained in a playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID for the playlist.</param>
    /// <param name="market">Optional country code.</param>
    /// <param name="fields">Fields filter string.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="additionalTypes">Comma-separated item types (track, episode).</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged set of playlist items.</returns>
    [HttpGet("playlists/{playlistId}/tracks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifyPlaylistItem>>>> GetPlaylistItems(
        string playlistId,
        [FromQuery] string? market = null,
        [FromQuery] string? fields = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        [FromQuery] string? additionalTypes = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving items for playlist {PlaylistId}", playlistId);

        var result = await _spotifyClient.GetPlaylistItemsAsync(
            playlistId, market ?? _options.DefaultMarket, fields, limit ?? _options.DefaultLimit, offset, additionalTypes, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Adds items to an existing playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID for the playlist.</param>
    /// <param name="request">Payload detailing items to append/insert.</param>
    /// <param name="position">Zero-based insertion position.</param>
    /// <param name="uris">Collection of Spotify URIs.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Snapshot result identifier.</returns>
    [HttpPost("playlists/{playlistId}/tracks")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<SpotifyResponse<SpotifySnapshotResult>>> AddItemsToPlaylist(
        string playlistId,
        [FromBody] AddItemsToPlaylistRequest request,
        [FromQuery] int? position = null,
        [FromQuery] IEnumerable<string>? uris = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding items to playlist {PlaylistId}", playlistId);

        var result = await _spotifyClient.AddItemsToPlaylistAsync(playlistId, request, position, uris, cancellationToken);
        return CreatedAtAction(nameof(GetPlaylist), new { playlistId }, result);
    }

    /// <summary>
    /// Reorders or replaces items inside a playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID for the playlist.</param>
    /// <param name="request">Payload specifying reorder parameters.</param>
    /// <param name="uris">Optional Spotify URIs to replace playlist contents with.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Snapshot result identifier.</returns>
    [HttpPut("playlists/{playlistId}/tracks")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifySnapshotResult>>> ReorderOrReplacePlaylistItems(
        string playlistId,
        [FromBody] ReorderOrReplacePlaylistItemsRequest request,
        [FromQuery] IEnumerable<string>? uris = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reordering or replacing items in playlist {PlaylistId}", playlistId);

        var result = await _spotifyClient.ReorderOrReplacePlaylistItemsAsync(playlistId, request, uris, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Removes items from a playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID for the playlist.</param>
    /// <param name="request">Payload defining items to remove.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Snapshot result identifier.</returns>
    [HttpDelete("playlists/{playlistId}/tracks")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifySnapshotResult>>> RemoveItemsFromPlaylist(
        string playlistId,
        [FromBody] RemovePlaylistItemsRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing items from playlist {PlaylistId}", playlistId);

        var result = await _spotifyClient.RemoveItemsFromPlaylistAsync(playlistId, request, cancellationToken);
        return Ok(result);
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
    /// Creates a playlist for the current user.
    /// </summary>
    /// <param name="request">Details of the playlist to create.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The newly created playlist resource.</returns>
    [HttpPost("me/playlists")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPlaylist>>> CreatePlaylist(
        [FromBody] CreatePlaylistRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new playlist '{PlaylistName}'", request.Name);

        var result = await _spotifyClient.CreatePlaylistAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPlaylist), new { playlistId = result.Data?.Id }, result);
    }

    /// <summary>
    /// Saves items to the current user's library using Spotify URIs.
    /// </summary>
    /// <param name="uris">Collection of Spotify URIs to save.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Action result outcome.</returns>
    [HttpPut("me/library")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<object>>> SaveLibraryItems(
        [FromBody] IEnumerable<string> uris,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Saving items to user's library");

        var result = await _spotifyClient.SaveLibraryItemsAsync(uris, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Removes items from the current user's library using Spotify URIs.
    /// </summary>
    /// <param name="uris">Collection of Spotify URIs to remove.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Action result outcome.</returns>
    [HttpDelete("me/library")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<object>>> RemoveLibraryItems(
        [FromBody] IEnumerable<string> uris,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing items from user's library");

        var result = await _spotifyClient.RemoveLibraryItemsAsync(uris, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Checks if specific Spotify URIs are present in the user's saved library.
    /// </summary>
    /// <param name="uris">Collection of Spotify URIs to check.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Collection of boolean flags matching the order of input URIs.</returns>
    [HttpPost("me/library/check")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<IReadOnlyList<bool>>>> CheckLibraryContains(
        [FromBody] IEnumerable<string> uris,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking user library state for URIs");

        var result = await _spotifyClient.CheckLibraryContainsAsync(uris, cancellationToken);
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
            limit ?? _options.DefaultLimit, offset, market ?? _options.DefaultMarket, cancellationToken);
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
            market ?? _options.DefaultMarket, limit ?? _options.DefaultLimit, offset, cancellationToken);
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
            market ?? _options.DefaultMarket, limit ?? _options.DefaultLimit, offset, cancellationToken);
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

    /// <summary>
    /// Gets top artists calculated based on affinity for the current user.
    /// </summary>
    /// <param name="timeRange">Time frame window (long_term, medium_term, short_term).</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged list of top artists.</returns>
    [HttpGet("me/top/artists")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifyArtist>>>> GetUsersTopArtists(
        [FromQuery] string? timeRange = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving current user's top artists");

        var result = await _spotifyClient.GetUsersTopItemsAsync<SpotifyArtist>(
            "artists", timeRange, limit ?? _options.DefaultLimit, offset, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets top tracks calculated based on affinity for the current user.
    /// </summary>
    /// <param name="timeRange">Time frame window (long_term, medium_term, short_term).</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paged list of top tracks.</returns>
    [HttpGet("me/top/tracks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyPagedResult<SpotifyTrack>>>> GetUsersTopTracks(
        [FromQuery] string? timeRange = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving current user's top tracks");

        var result = await _spotifyClient.GetUsersTopItemsAsync<SpotifyTrack>(
            "tracks", timeRange, limit ?? _options.DefaultLimit, offset, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets artists followed by the current user.
    /// </summary>
    /// <param name="type">Target entity type ID, defaults to 'artist'.</param>
    /// <param name="after">Cursor key for the last item ID fetched.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Cursor-paged artist results.</returns>
    [HttpGet("me/following")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<SpotifyCursorPagedResult<SpotifyArtist>>>> GetFollowedArtists(
        [FromQuery] string type = "artist",
        [FromQuery] string? after = null,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving current user's followed artists");

        var result = await _spotifyClient.GetFollowedArtistsAsync(type, after, limit ?? _options.DefaultLimit, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Uploads a custom cover image to represent a target playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="base64JpegImage">Base64 encoded JPEG image data string.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Operation result status.</returns>
    [HttpPut("playlists/{playlistId}/images")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<object>>> UploadCustomPlaylistCover(
        string playlistId,
        [FromBody] string base64JpegImage,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading custom cover image for playlist {PlaylistId}", playlistId);

        var result = await _spotifyClient.UploadCustomPlaylistCoverAsync(playlistId, base64JpegImage, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets image details for a specific playlist cover image.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A list of playlist cover image metadata items.</returns>
    [HttpGet("playlists/{playlistId}/images")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpotifyResponse<IReadOnlyList<SpotifyImage>>>> GetPlaylistCoverImage(
        string playlistId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving cover images for playlist {PlaylistId}", playlistId);

        var result = await _spotifyClient.GetPlaylistCoverImageAsync(playlistId, cancellationToken);
        return Ok(result);
    }
}
