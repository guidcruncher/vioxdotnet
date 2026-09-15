namespace Viox.Server.Configuration;

public class StreamingOptions
{
    public const string SectionName = "StreamingOptions";

    public int BufferSize { get; set; } = 2048;

    public int TimeoutMinutes { get; set; } = 30;
    public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";
}
