// File: LibraryController.cs
namespace Viox.Server.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
    private readonly MediaSourceResolverService _resolver;
    private readonly ILogger<LibraryController> _logger;

    public LibraryController(MediaSourceResolverService resolver, ILogger<LibraryController> logger)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{source}/{rawUri}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MediaMetaData))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaMetaData>> GetLibraryItem(
        [FromRoute] string source,
        [FromRoute] string rawUri,
        CancellationToken cancellationToken)
    {
        IMediaSource? library = _resolver.ResolveMediaSource(source);
        if (library is null)
        {
            return BadRequest("Could not load source");
        }

        MediaUri? uri = MediaUriParser.ParseMediaUriValue(rawUri);
        if (uri is null)
        {
            return BadRequest("Invalid URI");
        }

        MediaMetaData? res = await library.ResolveMetaData(uri, cancellationToken);
        if (res is null)
        {
            return NotFound();
        }

        return res;
    }

    [HttpGet("{source}/{rawUri}/items")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MediaMetaData>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<MediaMetaData>>> GetLibraryItemChildItems(
        [FromRoute] string source,
        [FromRoute] string rawUri,
        CancellationToken cancellationToken)
    {
        IMediaSource? library = _resolver.ResolveMediaSource(source);
        if (library is null)
        {
            return BadRequest("Could not load source");
        }

        MediaUri? uri = MediaUriParser.ParseMediaUriValue(rawUri);
        if (uri is null)
        {
            return BadRequest("Invalid URI");
        }

        MediaMetaData? res = await library.ResolveMetaData(uri, cancellationToken);
        if (res is null)
        {
            return NotFound();
        }

        IList<MediaMetaData> childItems = await library.ResolveChildItems(uri, string.Empty, cancellationToken);
        List<MediaMetaData> items = childItems.ToList();

        if (items.Count == 0)
        {
            return NotFound();
        }

        return Ok(items);
    }

    [HttpGet("{source}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MediaMetaData>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<MediaMetaData>>> GetLibrary(
        [FromRoute] string source,
        CancellationToken cancellationToken)
    {
        IMediaSource? library = _resolver.ResolveMediaSource(source);
        if (library is null)
        {
            return BadRequest("Could not load source");
        }

        Dictionary<string, object> filters = new();
        IQueryCollection query = HttpContext.Request.Query;
        foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> kv in query)
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
