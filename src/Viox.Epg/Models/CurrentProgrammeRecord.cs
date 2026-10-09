namespace Viox.Epg.Models;

public class CurrentProgrammeRecord
{
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string? ChannelIcon { get; set; }
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public long Start { get; set; }
    public long Stop { get; set; }
    public string? EpisodeNum { get; set; }
    public string? ProgrammeIcon { get; set; }
}
