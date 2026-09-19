namespace Viox.Server.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Viox.Core.Models;
using Viox.Core.Services;

[ApiController]
[Route("api/v1/library")]
[Tags("Library")]
[Produces("application/json")]
public class LibraryController : ControllerBase
{
    private readonly LibraryResolver _resolver;
    private readonly ILogger<LibraryController> _logger;

    public LibraryController(LibraryResolver resolver, ILogger<LibraryController> logger)
    {
        _resolver = resolver;
        _logger = logger;
    }

    [HttpGet("{source}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MediaMetaData>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<MediaMetaData>>> GetLibrary(
        [FromRoute] string source,
        CancellationToken cancellationToken)
    {
        ILibrary? library = _resolver.ResolveLibrary(source);

        if (library is null)
        {
            return BadRequest("Could not load source");
        }

        Dictionary<string, object> filters = new();
        var query = HttpContext.Request.Query;

        foreach (var kv in query)
        {
            filters[kv.Key] = kv.Value.ToString();
        }

        IList<MediaMetaData> res = await library.ReadAsync(filters, cancellationToken);

        if (res is null || res.Count == 0)
        {
            return NotFound();
        }

        return Ok(res);
    }
}
