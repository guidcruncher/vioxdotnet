using System.Diagnostics;

using Microsoft.Extensions.Options;

using Viox.Server.Configuration;

namespace Viox.Server.Services;

/// <summary>
/// Executes system commands for ALSA tools (e.g., amixer).
/// </summary>
public sealed class AlsaProcessExecutor : IAlsaProcessExecutor
{
    private readonly AlsaControlOptions _options;
    private readonly ILogger<AlsaProcessExecutor> _logger;

    public AlsaProcessExecutor(
        IOptions<AlsaControlOptions> options,
        ILogger<AlsaProcessExecutor> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(string fileName, string arguments, CancellationToken cancellationToken = default)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _logger.LogDebug("Executing ALSA process: {FileName} {Arguments}", fileName, arguments);

        try
        {
            process.Start();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_options.CommandTimeoutMs);

            string output = await process.StandardOutput.ReadToEndAsync(cts.Token);
            string error = await process.StandardError.ReadToEndAsync(cts.Token);

            await process.WaitForExitAsync(cts.Token);

            if (process.ExitCode != 0)
            {
                _logger.LogError("ALSA process execution failed with code {ExitCode}. Error: {Error}", process.ExitCode, error);
                throw new InvalidOperationException($"ALSA command failed with exit code {process.ExitCode}: {error}");
            }

            return output;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("ALSA process execution timed out after {Timeout}ms", _options.CommandTimeoutMs);
            throw new TimeoutException($"ALSA command '{fileName} {arguments}' timed out.", ex);
        }
    }
}
