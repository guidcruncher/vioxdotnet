namespace Viox.Server.Controllers;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Viox.Core.Models;
using Viox.Server.Abstraction;
using Viox.Server.Models;

/// <summary>
/// Provides REST API endpoints for managing media playback queue items, track navigation, and queue modes.
/// </summary>
[ApiController]
[Route("api/v1/queue")]
[Produces("application/json")]
[Tags("Playback Queue")]
public sealed class PlaybackQueueController : ControllerBase
{
    private readonly IPlaybackQueueService _queueService;
    private readonly ILogger<PlaybackQueueController> _logger;

    public PlaybackQueueController(
        IPlaybackQueueService queueService,
        ILogger<PlaybackQueueController> logger)
    {
        _queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// DTO representing current queue status for API responses.
    /// </summary>
    public sealed record QueueStatusResponse(
        IReadOnlyList<MediaMetaData> Items,
        int CurrentIndex,
        MediaMetaData? CurrentItem,
        bool IsShuffleEnabled,
        PlaybackRepeatMode RepeatMode);

    /// <summary>
    /// Gets the current state and contents of the playback queue.
    /// </summary>
    /// <returns>The active queue items, current track index, current item, and playback mode settings.</returns>
    /// <response code="200">Returns the full status of the playback queue.</response>
    [HttpGet]
    [ProducesResponseType(typeof(QueueStatusResponse), StatusCodes.Status200OK)]
    public IActionResult GetQueue()
    {
        QueueStatusResponse response = new(
            _queueService.Queue,
            _queueService.CurrentIndex,
            _queueService.CurrentItem,
            _queueService.IsShuffleEnabled,
            _queueService.RepeatMode);

        return Ok(response);
    }

    /// <summary>
    /// Enqueues a single media track to the end of the playback queue.
    /// </summary>
    /// <param name="item">The media metadata item to enqueue.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The media item was successfully enqueued.</response>
    /// <response code="400">The provided media metadata was null or invalid.</response>
    [HttpPost("enqueue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Enqueue([FromBody] MediaMetaData item, CancellationToken cancellationToken)
    {
        if (item is null)
        {
            return BadRequest("Media metadata item cannot be null.");
        }

        await _queueService.EnqueueAsync(item, cancellationToken);
        _logger.LogInformation("Enqueued track '{Title}' via API.", item.Title);
        return Ok();
    }

    /// <summary>
    /// Enqueues a collection of media tracks to the playback queue.
    /// </summary>
    /// <param name="items">Collection of media metadata items to enqueue.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The media items were successfully enqueued.</response>
    /// <response code="400">The provided collection was null or invalid.</response>
    [HttpPost("enqueue-batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnqueueBatch([FromBody] IEnumerable<MediaMetaData> items, CancellationToken cancellationToken)
    {
        if (items is null)
        {
            return BadRequest("Media metadata items collection cannot be null.");
        }

        await _queueService.EnqueueRangeAsync(items, cancellationToken);
        _logger.LogInformation("Enqueued batch of tracks via API.");
        return Ok();
    }

    /// <summary>
    /// Advances playback to the next track in the queue.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a next track was available and played; otherwise false.</returns>
    /// <response code="200">Successfully executed next navigation command.</response>
    [HttpPost("next")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> Next(CancellationToken cancellationToken)
    {
        bool result = await _queueService.PlayNextAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Moves playback to the previous track in the queue.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a previous track was available and played; otherwise false.</returns>
    /// <response code="200">Successfully executed previous navigation command.</response>
    [HttpPost("previous")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> Previous(CancellationToken cancellationToken)
    {
        bool result = await _queueService.PlayPreviousAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Jumps directly to a specific track index in the queue.
    /// </summary>
    /// <param name="index">The zero-based queue index to play.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Successfully skipped to the target index.</response>
    /// <response code="400">The target index was out of bounds.</response>
    [HttpPost("skip/{index:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SkipTo(int index, CancellationToken cancellationToken)
    {
        bool success = await _queueService.SkipToAsync(index, cancellationToken);
        if (!success)
        {
            return BadRequest($"Invalid queue index {index}. Index is out of range.");
        }

        return Ok();
    }

    /// <summary>
    /// Removes a media track at the specified zero-based index from the queue.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Item successfully removed or index was out of bounds.</response>
    [HttpDelete("{index:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveAt(int index, CancellationToken cancellationToken)
    {
        await _queueService.RemoveAtAsync(index, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Clears all tracks from the playback queue and stops active playback.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The playback queue was successfully cleared.</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        await _queueService.ClearAsync(cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Enables or disables queue shuffle mode.
    /// </summary>
    /// <param name="enable">True to enable shuffle, false to disable.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Shuffle mode state was updated.</response>
    [HttpPost("shuffle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetShuffle([FromBody] bool enable, CancellationToken cancellationToken)
    {
        await _queueService.SetShuffleAsync(enable, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Configures the queue repeat mode.
    /// </summary>
    /// <param name="repeatMode">The desired repeat mode (Off, One, All).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Repeat mode was updated.</response>
    [HttpPost("repeat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetRepeatMode([FromBody] PlaybackRepeatMode repeatMode, CancellationToken cancellationToken)
    {
        await _queueService.SetRepeatModeAsync(repeatMode, cancellationToken);
        return Ok();
    }
}
