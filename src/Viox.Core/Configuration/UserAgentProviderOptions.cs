namespace Viox.Core.Configuration;

/// <summary>
/// Options for configuring the <see cref="UserAgentProvider"/> service.
/// </summary>
public sealed class UserAgentOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "UserAgentProvider";

    /// <summary>
    /// A collection of user-defined custom User-Agent strings that extend or override the default pool.
    /// </summary>
    public string[] CustomUserAgents { get; set; } = [];
}
