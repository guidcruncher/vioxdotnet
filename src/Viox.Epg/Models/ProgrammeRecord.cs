namespace Viox.Epg.Models;

public class ProgrammeRecord
{
    public string ChannelId { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string Stop { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public string? EpisodeNum { get; set; }
    public string? IconUrl { get; set; }
    public List<string> Categories { get; set; } = [];
}
