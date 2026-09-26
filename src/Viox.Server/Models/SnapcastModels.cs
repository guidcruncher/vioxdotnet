using System.ComponentModel.DataAnnotations;

namespace Viox.Server.Models;

/// <summary>
/// Data transfer object for updating a client's friendly display name.
/// </summary>
public sealed record SetClientNameRequest
{
    /// <summary>
    /// The new display name for the client.
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name cannot be empty.")]
    public required string Name { get; init; }
}
/// <summary>
/// Data transfer object for updating a client's latency offset.
/// </summary>
public sealed record SetClientLatencyRequest
{
    /// <summary>
    /// The latency offset in milliseconds.
    /// </summary>
    public required int Latency { get; init; }
}
/// <summary>
/// Data transfer object for toggling group mute status.
/// </summary>
public sealed record SetGroupMuteRequest
{
    /// <summary>
    /// Indicates whether the group should be muted.
    /// </summary>
    public required bool Mute { get; init; }
}
/// <summary>
/// Data transfer object for assigning a stream to a group.
/// </summary>
public sealed record SetGroupStreamRequest
{
    /// <summary>
    /// The target audio stream ID.
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "StreamId cannot be empty.")]
    public required string StreamId { get; init; }
}
/// <summary>
/// Data transfer object for updating the list of clients assigned to a group.
/// </summary>
public sealed record SetGroupClientsRequest
{
    /// <summary>
    /// The collection of client IDs to assign to the group.
    /// </summary>
    [Required]
    public required IEnumerable<string> ClientIds { get; init; }
}
