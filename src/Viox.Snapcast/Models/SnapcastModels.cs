using System.Text.Json;
using System.Text.Json.Serialization;

namespace Viox.Snapcast.Models;

#region JSON-RPC Core Wire Models

public record JsonRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("method")]
    public string Method { get; init; } = string.Empty;

    [JsonPropertyName("params")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Params { get; init; }
}

public record JsonRpcResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("result")]
    public JsonElement? Result { get; set; }

    [JsonPropertyName("error")]
    public JsonRpcError? Error { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("params")]
    public JsonElement? Params { get; set; }
}

public record JsonRpcError
{
    [JsonPropertyName("code")]
    public int Code { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("data")]
    public JsonElement? Data { get; init; }
}

#endregion

#region Snapcast Domain Entities

public record RpcVersion(
    [property: JsonPropertyName("major")] int Major,
    [property: JsonPropertyName("minor")] int Minor,
    [property: JsonPropertyName("patch")] int Patch
);

public record VolumeState(
    [property: JsonPropertyName("percent")] int Percent,
    [property: JsonPropertyName("muted")] bool Muted
);

public record ClientConfig(
    [property: JsonPropertyName("instance")] int Instance,
    [property: JsonPropertyName("latency")] int Latency,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("volume")] VolumeState Volume
);

public record ClientHost(
    [property: JsonPropertyName("arch")] string Arch,
    [property: JsonPropertyName("ip")] string Ip,
    [property: JsonPropertyName("mac")] string Mac,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("os")] string Os
);

public record SnapClient(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("connected")] bool Connected,
    [property: JsonPropertyName("config")] ClientConfig Config,
    [property: JsonPropertyName("host")] ClientHost Host,
    [property: JsonPropertyName("lastSeen")] SnapTime LastSeen
);

public record SnapTime(
    [property: JsonPropertyName("sec")] long Sec,
    [property: JsonPropertyName("usec")] long Usec
);

public record SnapGroup(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("muted")] bool Muted,
    [property: JsonPropertyName("stream_id")] string StreamId,
    [property: JsonPropertyName("clients")] List<SnapClient> Clients
);

public record SnapStream(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("uri")] JsonElement Uri
);

public record SnapServer(
    [property: JsonPropertyName("host")] ClientHost Host,
    [property: JsonPropertyName("groups")] List<SnapGroup> Groups,
    [property: JsonPropertyName("streams")] List<SnapStream> Streams
);

public record ServerStatusContainer(
    [property: JsonPropertyName("server")] SnapServer Server
);

#endregion

#region Event Notification Payloads

public record ClientConnectEventArgs(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("client")] SnapClient Client
);

public record ClientDisconnectEventArgs(
    [property: JsonPropertyName("id")] string Id
);

public record ClientVolumeChangedEventArgs(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("volume")] VolumeState Volume
);

public record GroupMuteEventArgs(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("mute")] bool Mute
);

public record GroupStreamChangedEventArgs(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("stream_id")] string StreamId
);

public record ServerUpdateEventArgs(
    [property: JsonPropertyName("server")] SnapServer Server
);

#endregion
