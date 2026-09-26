using Microsoft.AspNetCore.Mvc;

using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Server.Controllers;

/// <summary>
/// RESTful API Controller handling favorite items management operations.
/// </summary>
[ApiController]
[Route("api/v1/favourites")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoritesService _favoritesService;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(
        IFavoritesService favoritesService,
        ILogger<FavoritesController> logger)
    {
        _favoritesService = favoritesService ?? throw new ArgumentNullException(nameof(favoritesService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all media items marked as favorites.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MediaMetaData>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var items = _favoritesService.GetAllFavorites();
        return Ok(items);
    }

    /// <summary>
    /// Finds a favorite by its exact RawUri string.
    /// </summary>
    [HttpGet("find")]
    [ProducesResponseType(typeof(MediaMetaData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetByRawUri([FromQuery] string rawUri)
    {
        if (string.IsNullOrWhiteSpace(rawUri))
        {
            return BadRequest("RawUri parameter is required.");
        }

        var item = _favoritesService.GetByRawUri(rawUri);
        return item is not null ? Ok(item) : NotFound();
    }

    /// <summary>
    /// Searches stored favorites matching a RawUri string query.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<MediaMetaData>), StatusCodes.Status200OK)]
    public IActionResult Search([FromQuery] string query)
    {
        var results = _favoritesService.SearchFavorites(query);
        return Ok(results);
    }

    /// <summary>
    /// Adds or updates a favorite item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] MediaMetaData item, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        bool result = await _favoritesService.AddFavoriteAsync(item, cancellationToken);
        return result ? Ok() : BadRequest("Failed to add favorite. Ensure valid RawUri identification.");
    }

    /// <summary>
    /// Deletes a favorite item using its RawUri.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove([FromQuery] string rawUri, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawUri))
        {
            return BadRequest("RawUri query parameter is required.");
        }

        bool removed = await _favoritesService.RemoveFavoriteAsync(rawUri, cancellationToken);
        return removed ? Ok() : NotFound();
    }

    /// <summary>
    /// Removes all stored favorites.
    /// </summary>
    [HttpDelete("clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        await _favoritesService.ClearFavoritesAsync(cancellationToken);
        return Ok();
    }
}
