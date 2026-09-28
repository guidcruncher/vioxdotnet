namespace Viox.Core.Configuration;

public class ClientOptionsStoreOptions
{
    public const string SectionName = "ClientOptionsStore";

    public string FilePath { get; set; } = "/data/client-config.json";
}
