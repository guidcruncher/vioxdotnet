// File: PlaylistsController.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetPlaylistNamesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all playlist names.");
        var names = await _playlistService.GetPlaylistNamesAsync(cancellationToken);
        return Ok(names);
    }

    /// <summary>
    /// Loads a specific playlist by its name or file identifier.
    /// </summary>
    /// <param name="name">The name or identifier of the playlist.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The playlist metadata if found; otherwise, a 404 Not Found response.</returns>
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(MediaMetaDataPlaylist), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaMetaDataPlaylist>> LoadPlaylistAsync(string name, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loading playlist with name: {PlaylistName}", name);
        var playlist = await _playlistService.LoadPlaylistAsync(name, cancellationToken);
        
        if (playlist == null)
        {
            _logger.LogWarning("Playlist with name: {PlaylistName} was not found.", name);
            return NotFound();
        }

        return Ok(playlist);
    }

    /// <summary>
    /// Deletes a playlist by its name.
    /// </summary>
    /// <param name="name">The name of the playlist to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>No content if successfully deleted; otherwise, a 404 Not Found response.</returns>
    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePlaylistAsync(string name, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting playlist with name: {PlaylistName}", name);
        var deleted = await _playlistService.DeletePlaylistAsync(name, cancellationToken);

        if (!deleted)
        {
            _logger.LogWarning("Failed to delete playlist with name: {PlaylistName} as it was not found.", name);
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Adds a media item to a specified playlist.
    /// </summary>
    /// <param name="playlistName">The name of the target playlist.</param>
    /// <param name="item">The media metadata item to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>No content on success, or a bad request if the item is invalid.</returns>
    [HttpPost("{playlistName}/items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItemToPlaylistAsync(string playlistName, [FromBody] MediaMetaData item, CancellationToken cancellationToken)
    {
        if (item == null)
        {
            return BadRequest("Item metadata cannot be null.");
        }

        _logger.LogInformation("Adding item to playlist: {PlaylistName}", playlistName);
        await _playlistService.AddItemToPlaylistAsync(playlistName, item, cancellationToken);
        
        return NoContent();
    }

    /// <summary>
    /// Removes a media item from a specified playlist using its raw URI.
    /// </summary>
    /// <param name="playlistName">The name of the target playlist.</param>
    /// <param name="rawUri">The raw URI of the media item to remove.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{playlistName}/items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveItemFromPlaylistAsync(string playlistName, [FromQuery] string rawUri, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing item with URI {RawUri} from playlist: {PlaylistName}", rawUri, playlistName);
        await _playlistService.RemoveItemFromPlaylistAsync(playlistName, rawUri, cancellationToken);
        
        return NoContent();
    }
}
