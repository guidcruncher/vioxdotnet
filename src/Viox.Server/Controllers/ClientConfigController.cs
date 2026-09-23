using Microsoft.AspNetCore.Mvc;

using Viox.Server.Models;
using Viox.Server.Services;

namespace Viox.Server.Controllers;

[ApiController]
[Route("api/v1/client/config")]
public class ClientConfigController : ControllerBase
{
    private readonly IClientOptionsStore _clientOptionsStore;
    private readonly ILogger<ClientConfigController> _logger;

    public ClientConfigController(
        IClientOptionsStore clientOptionsStore,
        ILogger<ClientConfigController> logger)
    {
        ArgumentNullException.ThrowIfNull(clientOptionsStore);
        ArgumentNullException.ThrowIfNull(logger);

        _clientOptionsStore = clientOptionsStore;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ClientConfiguration>> GetConfig(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GET request for client configuration.");
        var config = await _clientOptionsStore.GetConfigurationAsync(cancellationToken);
        return Ok(config);
    }

    [HttpPost]
    public async Task<IActionResult> SaveConfig(
        [FromBody] ClientConfiguration configuration,
        CancellationToken cancellationToken)
    {
        if (configuration is null)
        {
            return BadRequest("Configuration payload cannot be null.");
        }

        _logger.LogInformation("Handling POST request to update client configuration.");
        await _clientOptionsStore.SaveConfigurationAsync(configuration, cancellationToken);
        return NoContent();
    }
}
