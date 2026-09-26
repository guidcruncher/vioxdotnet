namespace Viox.Server.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Viox.Core.Services;

[ApiController]
[Route("api/v1/core")]
[Tags("Core")]
[Produces("application/json")]
public class CoreController : ControllerBase
{
    private readonly ILogger<CoreController> _logger;

    public CoreController(ILogger<CoreController> logger)
    {
        _logger = logger;
    }

    private static bool IsNumeric(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return double.TryParse(value, out _);
    }

    [HttpGet("countrys")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Dictionary<string, string>>> GetCountries(
        CancellationToken cancellationToken)
    {
        Dictionary<string, string> source = IsoCountryCodes.GetIso3166Codes();

        Dictionary<string, string> sortedSequence = source
            .Where(kvp => !IsNumeric(kvp.Key))
            .OrderBy(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return await Task.FromResult(Ok(sortedSequence));
    }
}
