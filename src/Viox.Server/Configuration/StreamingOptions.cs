namespace Viox.Server.Configuration;

public class StreamingOptions
{
    public const string SectionName = "StreamingOptions";

    public int BufferSize { get; set; } = 2048;

    public int TimeoutMinutes { get; set; } = 30;
    public string UserAgent { get; set; } = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/154 Version/11.1.1 Safari/605.1.15";
}
