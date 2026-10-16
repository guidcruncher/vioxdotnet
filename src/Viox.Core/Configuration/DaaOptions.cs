namespace Viox.Core.Configuration;

public class DataOptions
{
    public const string SectionName = "Data";

    public string SqliteConnectionString { get; set; } = "Data Source=/data/system.db";
}
