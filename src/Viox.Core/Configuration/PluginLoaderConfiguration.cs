namespace Viox.Core.Configuration;

public class PluginLoaderConfiguration
{
    public const string SectionName = "PluginLoader";

    public string PluginFolderPath { get; set; } = "/plugins";

    public string SearchPattern { get; set; } = "*.dll";
}
