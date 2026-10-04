// File: PlaylistsController.cs
using Microsoft.AspNetCore.Mvc;

using Viox.Core.Models;
using Viox.Core.Playlists;

namespace Viox.Api.Controllers;

/// <summary>
/// Manages playlist operations such as retrieval, item modification, and deletion.
/// </summary>
[ApiController]
[Route("api/v1/playlists")]
[Produces("application/json")]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistService _playlistService;
    private readonly ILogger<PlaylistsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistsController"/> class.
    /// </summary>
    /// <param name="playlistService">The playlist service implementation.</param>
    /// <param name="logger">The logging service instance.</param>
    public PlaylistsController(IPlaylistService playlistService, ILogger<PlaylistsController> logger)
    {
        _playlistService = playlistService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all stored playlist names.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An enumeration of playlist names.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetPlaylistNamesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all playlist names.");
        var names = await _playlistService.GetPlaylistNamesAsync(cancellationToken);
        return Ok(names);
    }

    /// <summary>
    /// Loads a specific playlist by its name or file identifier.
    /// </summary>
    /// <param name="id">The name or identifier of the playlist.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The playlist metadata if found; otherwise, a 404 Not Found response.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MediaMetaDataPlaylist), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaMetaDataPlaylist>> LoadPlaylistAsync(string id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loading playlist with name: {PlaylistName}", id);
        var playlist = await _playlistService.LoadPlaylistAsync(id, cancellationToken);

        if (playlist == null)
        {
            _logger.LogWarning("Playlist with id: {PlaylistName} was not found.", id);
            return NotFound();
        }

        return Ok(playlist);
    }

    /// <summary>
    /// Deletes a playlist by its id.
    /// </summary>
    /// <param name="id">The name of the playlist to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>No content if successfully deleted; otherwise, a 404 Not Found response.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePlaylistAsync(string id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting playlist with id: {Playlistid}", id);
        var deleted = await _playlistService.DeletePlaylistAsync(id, cancellationToken);

        if (!deleted)
        {
            _logger.LogWarning("Failed to delete playlist with id: {Playlistid} as it was not found.", id);
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Adds a media item to a new playlist.
    /// </summary>
    /// <param name="item">The media metadata item to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>No content on success, or a bad request if the item is invalid.</returns>
    [HttpPost("items")]
    [ProducesResponseType(typeof(MediaMetaDataPlaylist), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaMetaDataPlaylist>> AddItemToNewPlaylistAsync([FromQuery] string title, [FromBody] MediaMetaData item, CancellationToken cancellationToken)
    {
        if (item == null)
        {
            return BadRequest("Item metadata cannot be null.");
        }

        string id = await _playlistService.CreatePlaylistAsync(title, item, cancellationToken);
        _logger.LogInformation("Adding item to playlist: {id}", id);

        var playlist = await _playlistService.LoadPlaylistAsync(id, cancellationToken);

        if (playlist == null)
        {
            _logger.LogWarning("Playlist with id: {PlaylistName} was not found.", id);
            return BadRequest("Created Playlist could not be loaded.");
        }

        return playlist;
    }

    /// <summary>
    /// Adds a media item to a specified playlist.
    /// </summary>
    /// <param name="id">The name of the target playlist.</param>
    /// <param name="item">The media metadata item to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>No content on success, or a bad request if the item is invalid.</returns>
    [HttpPost("{id}/items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItemToPlaylistAsync(string id, [FromBody] MediaMetaData item, CancellationToken cancellationToken)
    {
        if (item == null)
        {
            return BadRequest("Item metadata cannot be null.");
        }

        _logger.LogInformation("Adding item to playlist: {id}", id);
        await _playlistService.AddItemToPlaylistAsync(id, item, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Removes a media item from a specified playlist using its raw URI.
    /// </summary>
    /// <param name="id">The name of the target playlist.</param>
    /// <param name="rawUri">The raw URI of the media item to remove.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{rawUri}/items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveItemFromPlaylistAsync(string rawUri, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing item with URI {RawUri}", rawUri);
        await _playlistService.RemoveItemFromPlaylistAsync(rawUri, cancellationToken);

        return NoContent();
    }
}

