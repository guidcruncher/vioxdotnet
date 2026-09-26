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

    public async Task<string> ExecuteAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(arguments);

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };

        string formattedArgs = string.Join(' ', arguments);
        _logger.LogDebug("Executing ALSA process: {FileName} {Arguments}", fileName, formattedArgs);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_options.CommandTimeoutMs);

        try
        {
            process.Start();

            Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cts.Token);
            Task<string> errorTask = process.StandardError.ReadToEndAsync(cts.Token);

            await Task.WhenAll(outputTask, errorTask);
            await process.WaitForExitAsync(cts.Token);

            string output = await outputTask;
            string error = await errorTask;

            if (process.ExitCode != 0)
            {
                _logger.LogWarning("Error Executing ALSA process: {FileName} {Arguments}", fileName, formattedArgs);
                _logger.LogError("ALSA process execution failed with code {ExitCode}. Error: {Error}", process.ExitCode, error);
                throw new InvalidOperationException($"ALSA command failed with exit code {process.ExitCode}: {error}");
            }

            return output;
        }
        catch (OperationCanceledException ex)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError("ALSA process execution timed out after {Timeout}ms", _options.CommandTimeoutMs);
                throw new TimeoutException($"ALSA command '{fileName} {formattedArgs}' timed out.", ex);
            }

            throw;
        }
    }
}
