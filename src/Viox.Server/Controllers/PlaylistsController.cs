// File: PlaylistsController.cs
namespace Viox.Server.Controllers;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Viox.Core.Models;
using Viox.Core.Services;

/// <summary>
/// API controller for managing, searching, and reloading in-memory M3U playlists.
/// </summary>
[ApiController]
[Route("api/v1/media/playlists")]
public class PlaylistsController : ControllerBase
{
    private readonly IM3uPlaylistProvider _playlistProvider;
    private readonly ILogger<PlaylistsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistsController"/> class.
    /// </summary>
    /// <param name="playlistProvider">The M3U playlist provider service.</param>
    /// <param name="logger">The logger instance.</param>
    public PlaylistsController(
        IM3uPlaylistProvider playlistProvider,
        ILogger<PlaylistsController> logger)
    {
        _playlistProvider = playlistProvider ?? throw new ArgumentNullException(nameof(playlistProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all currently loaded playlists and their associated media items.
    /// </summary>
    /// <returns>A dictionary of playlist keys mapped to arrays of <see cref="MediaMetaData"/>.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyDictionary<string, MediaMetaData[]>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPlaylists()
    {
        var playlists = _playlistProvider.Playlists;
        if (playlists.Count() == 0)
        {
            await _playlistProvider.LoadAsync();
            playlists = _playlistProvider.Playlists;
        }

        return Ok(playlists);
    }

    /// <summary>
    /// Retrieves media items for a specific playlist key.
    /// </summary>
    /// <param name="key">The identifier key of the target playlist.</param>
    /// <returns>An array of <see cref="MediaMetaData"/> items associated with the given key.</returns>
    [HttpGet("{key}")]
    [ProducesResponseType(typeof(MediaMetaData[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlaylistByKey(string key)
    {
        if (_playlistProvider.Playlists.Count() == 0)
        {
            await _playlistProvider.LoadAsync();
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest("Playlist key cannot be null or empty.");
        }

        if (!_playlistProvider.Playlists.ContainsKey(key))
        {
            return NotFound($"Playlist with key '{key}' was not found.");
        }

        var items = _playlistProvider.GetPlaylist(key);
        return Ok(items);
    }

    /// <summary>
    /// Searches across all loaded playlists for a media item matching the specified URI string.
    /// </summary>
    /// <param name="uri">The URI string value to look up (Url, RawUri, or formatted Uri).</param>
    /// <returns>The matching <see cref="MediaMetaData"/> item if found; otherwise, 404 Not Found.</returns>
    [HttpGet("find")]
    [ProducesResponseType(typeof(MediaMetaData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FindByUri([FromQuery] string uri)
    {
        if (_playlistProvider.Playlists.Count() == 0)
        {
            await _playlistProvider.LoadAsync();
        }

        if (string.IsNullOrWhiteSpace(uri))
        {
            return BadRequest("The 'uri' query parameter is required.");
        }

        var item = _playlistProvider.FindByUri(uri);
        if (item is null)
        {
            return NotFound($"No media item found matching URI: '{uri}'.");
        }

        return Ok(item);
    }

    /// <summary>
    /// Loads and parses a dictionary of playlist sources into memory.
    /// </summary>
    /// <param name="sources">Key-value pairs where the key is a playlist identifier and the value is a file path, URL, or raw M3U string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Status indicating success and current count of loaded playlists.</returns>
    [HttpPost("load")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoadPlaylists(
        CancellationToken cancellationToken)
    {
        await _playlistProvider.LoadAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            Message = "Playlists loaded successfully.",
            PlaylistCount = _playlistProvider.Playlists.Count
        });
    }

    /// <summary>
    /// Reloads all currently registered playlist sources from their original locations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Status indicating success and total refreshed playlist count.</returns>
    [HttpPost("reload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReloadPlaylists(CancellationToken cancellationToken)
    {
        _logger.LogInformation("API request received to reload all registered playlist sources.");
        await _playlistProvider.ReloadAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            Message = "Playlists reloaded successfully.",
            PlaylistCount = _playlistProvider.Playlists.Count
        });
    }

    /// <summary>
    /// Clears all loaded playlist data and search indices from memory.
    /// </summary>
    /// <returns>Status indicating success.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ClearPlaylists()
    {
        _logger.LogInformation("API request received to clear all in-memory playlist data.");
        _playlistProvider.Clear();

        return Ok(new { Message = "All playlists have been cleared successfully." });
    }
}

