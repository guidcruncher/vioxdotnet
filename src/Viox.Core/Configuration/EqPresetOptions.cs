namespace Viox.Core.Configuration;

public sealed class EqPresetOptions
{
    public const string SectionName = "EqPresets";

    public string FilePath { get; set; } = "/etc/eq.json";

}
