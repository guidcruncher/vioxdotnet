namespace Viox.Core.Configuration;

public class CacheOptions
{
    public const string SectionName = "MemoryCacheService";

    public double DefaultAbsoluteExpirationHours { get; set; } = 24.0;

    public double? DefaultSlidingExpirationMinutes { get; set; } = 60.0;

    public long? SizeLimit { get; set; }
}
