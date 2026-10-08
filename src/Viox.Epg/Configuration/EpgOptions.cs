namespace Viox.Epg.Configuration;

public class EpgOptions
{
    public const string SectionName = "Epg";

    public string SourceUrl { get; set; } = string.Empty;
    public string SqliteConnectionString { get; set; } = "Data Source=epg.db";
}
