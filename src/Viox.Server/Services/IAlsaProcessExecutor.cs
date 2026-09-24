namespace Viox.Server.Services;

/// <summary>
/// Abstraction for executing underlying process commands.
/// </summary>
public interface IAlsaProcessExecutor
{
    /// <summary>
    /// Executes a system command asynchronously and returns the standard output.
    /// </summary>
    /// <param name="fileName">The executable name or path.</param>
    /// <param name="arguments">The argument list to pass to the executable.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standard output string resulting from execution.</returns>
    Task<string> ExecuteAsync(string fileName, string arguments, CancellationToken cancellationToken = default);
}
