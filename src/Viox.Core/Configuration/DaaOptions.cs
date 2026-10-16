namespace Viox.Core.Configuration;

public class DataOptions
{
    public const string SectionName = "Data";

    public string SystemConnectionString { get; set; } = "Data Source=/data/system.db";

    public string StatsConnectionString { get; set; } = "Data Source=/data/stats.db";
}
