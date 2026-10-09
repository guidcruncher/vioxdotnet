namespace Viox.Epg.Configuration;

public class EpgOptions
{
    public const string SectionName = "Epg";

    public string SourceUrl { get; set; } = "https://epgshare01.online/epgshare01/epg_ripper_UK1.xml.gz";
    public string SqliteConnectionString { get; set; } = "Data Source=/data/epg.db";

    public bool RunOnStartup { get; set; } = true;
    public int PollIntervalHours { get; set; } = 24;
}
