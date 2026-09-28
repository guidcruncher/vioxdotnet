namespace Viox.Core.Services;

/// <summary>
/// Defines a service for retrieving a static User-Agent string pinned for the application lifecycle.
/// </summary>
public interface IUserAgentProvider
{
    /// <summary>
    /// Gets the sticky User-Agent string assigned to this application instance.
    /// </summary>
    /// <returns>A valid User-Agent string.</returns>
    string GetUserAgent();
}
