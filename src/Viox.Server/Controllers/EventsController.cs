using System.Net.Mime;
using System.Net.ServerSentEvents;

using Microsoft.AspNetCore.Mvc;

using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Server.Controllers;

/// <summary>
/// Manages Server-Sent Events (SSE) streaming, broadcasting, and subscriber tracking.
/// </summary>
[ApiController]
[Route("api/v1/events")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Server Events")]
public sealed class EventsController : ControllerBase
{
    private readonly IServerEventPublisher _publisher;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IServerEventPublisher publisher,
        ILogger<EventsController> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Subscribe to real-time Server-Sent Events stream.
    /// </summary>
    /// <remarks>
    /// Establishes an HTTP text/event-stream connection. The client receives events as JSON payloads wrapped in standard SSE framing.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token supplied by client disconnection.</param>
    /// <returns>A continuous Server-Sent Event stream.</returns>
    /// <response code="200">Returns continuous SSE stream.</response>
    [HttpGet]
    [Produces("text/event-stream")]
    [ProducesResponseType(typeof(EventPayload), StatusCodes.Status200OK, "text/event-stream")]
    [EndpointSummary("Subscribe to Event Stream")]
    [EndpointDescription("Establishes a persistent Server-Sent Events connection for real-time notifications.")]
    public IResult StreamEvents(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Client initiated SSE connection on /api/v1/events.");

        async IAsyncEnumerable<SseItem<EventPayload>> GetEventEnumerable()
        {
            await foreach (EventPayload payload in _publisher.SubscribeAsync(cancellationToken))
            {
                yield return new SseItem<EventPayload>(payload, payload.EventType)
                {
                    EventId = payload.EventId
                };
            }
        }

        return TypedResults.ServerSentEvents(GetEventEnumerable());
    }

    /// <summary>
    /// Publish an event to all connected clients.
    /// </summary>
    /// <remarks>
    /// Broadcasts a custom event type and message payload to every active subscriber connected to the SSE stream.
    /// </remarks>
    /// <param name="request">The event payload details containing event type and message body.</param>
    /// <param name="cancellationToken">Token to monitor for operation cancellation.</param>
    /// <returns>Execution result and active subscriber count.</returns>
    /// <response code="200">Event successfully queued for delivery.</response>
    /// <response code="400">Request payload is invalid or missing required fields.</response>
    [HttpPost("emit")]
    [ProducesResponseType(typeof(EmitEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Publish Server Event")]
    [EndpointDescription("Broadcasts an event message payload to all currently connected subscribers.")]
    public async Task<IActionResult> EmitEvent(
        [FromBody] EmitEventRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EventType) || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Event Payload",
                Detail = "EventType and Message are required fields.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        await _publisher.PublishAsync(request.EventType, request.Message, cancellationToken);

        return Ok(new EmitEventResponse(
            Status: "Event Emitted Successfully",
            ActiveSubscribers: _publisher.ActiveSubscriberCount
        ));
    }

    /// <summary>
    /// Get the active subscriber count.
    /// </summary>
    /// <remarks>
    /// Returns the current number of clients actively holding an open SSE stream connection.
    /// </remarks>
    /// <returns>An object containing the current active subscriber count.</returns>
    /// <response code="200">Returns current subscriber statistics.</response>
    [HttpGet("subscribers")]
    [ProducesResponseType(typeof(SubscriberCountResponse), StatusCodes.Status200OK)]
    [EndpointSummary("Get Subscriber Count")]
    [EndpointDescription("Retrieves the total count of active connected SSE clients.")]
    public IActionResult GetSubscriberCount()
    {
        return Ok(new SubscriberCountResponse(ActiveSubscribers: _publisher.ActiveSubscriberCount));
    }
}
