namespace Viox.Epg.Models;

public class ChannelRecord
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? IconUrl { get; set; }
}
