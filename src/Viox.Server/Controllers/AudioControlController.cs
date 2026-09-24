using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Viox.Server.Configuration;
using Viox.Server.Services;
using Viox.Server.Models;

namespace Viox.Server.Controllers;

/// <summary>
/// Controller for managing ALSA audio equalizer bands and mixer volume settings.
/// </summary>
[ApiController]
[Route("api/v1/audiocontrol")]
[Produces("application/json")]
public sealed class AudioControlController : ControllerBase
{
    private readonly IAlsaEqualizerService _equalizerService;
    private readonly ILogger<AudioControlController> _logger;

    public AudioControlController(
        IAlsaEqualizerService equalizerService,
        ILogger<AudioControlController> logger)
    {
        ArgumentNullException.ThrowIfNull(equalizerService);
        ArgumentNullException.ThrowIfNull(logger);

        _equalizerService = equalizerService;
        _logger = logger;
    }

    /// <summary>
    /// Gets current equalizer band details including indexes, control names, frequency labels, and percentages.
    /// </summary>
    [HttpGet("equalizer/bands")]
    [ProducesResponseType(typeof(IReadOnlyList<EqualizerBand>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEqualizerBands(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET request received for equalizer band settings.");
        IReadOnlyList<EqualizerBand> bands = await _equalizerService.GetBandsAsync(cancellationToken);
        return Ok(bands);
    }

    /// <summary>
    /// Sets a specific equalizer band level by band index.
    /// </summary>
    /// <param name="bandIndex">Index of the equalizer band (0-indexed).</param>
    /// <param name="request">Request body containing percentage level (0-100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("equalizer/bands/{bandIndex:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetEqualizerBand(
        [FromRoute] int bandIndex,
        [FromBody] SetLevelRequest request,
        CancellationToken cancellationToken)
    {
        if (bandIndex < 0)
        {
            return BadRequest("Band index must be non-negative.");
        }

        if (request.Percentage is < 0 or > 100)
        {
            return BadRequest("Percentage must be between 0 and 100.");
        }

        _logger.LogInformation("HTTP PUT request received for equalizer band {BandIndex} to {Percentage}%.", bandIndex, request.Percentage);
        await _equalizerService.SetBandLevelAsync(bandIndex, request.Percentage, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets all equalizer bands simultaneously.
    /// </summary>
    /// <param name="request">Request body containing sequential band level percentages.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("equalizer/bands")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetAllEqualizerBands(
        [FromBody] SetAllBandsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Percentages is null || !request.Percentages.Any())
        {
            return BadRequest("Percentages list cannot be empty.");
        }

        if (request.Percentages.Any(p => p is < 0 or > 100))
        {
            return BadRequest("All band percentages must be between 0 and 100.");
        }

        _logger.LogInformation("HTTP PUT request received for updating all equalizer bands.");
        await _equalizerService.SetAllBandsAsync(request.Percentages, cancellationToken);
        return NoContent();
    }

}

public sealed record SetLevelRequest(int Percentage);

/// <summary>
/// Request DTO for setting all equalizer bands.
/// </summary>
public sealed record SetAllBandsRequest(IEnumerable<int> Percentages);
