namespace Viox.Core.Models;

public record PodcastEpisode
{
    public string Uri { get; init; } = string.Empty;
    public string PodcastId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset? PublishedDate { get; init; }
    public string AudioUrl { get; init; } = string.Empty;
    public string? AudioMimeType { get; init; }
    public long? AudioSizeBytes { get; init; }
    public string? Duration { get; init; }
    public double? DurationSeconds { get; init; }
    public string? Guid { get; init; }
    public string? Link { get; init; }
    public string? ImageUrl { get; init; }
}
