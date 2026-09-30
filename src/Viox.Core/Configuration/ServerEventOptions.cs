namespace Viox.Core.Configuration;

public sealed class ServerEventOptions
{
    public const string SectionName = "ServerEvents";

    public string EndpointPath { get; set; } = "/api/v1/events";
    public int ChannelCapacity { get; set; } = 1000;
}
