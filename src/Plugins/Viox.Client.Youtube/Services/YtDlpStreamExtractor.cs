using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Youtube.Configuration;
using Viox.Client.Youtube.Models;

namespace Viox.Client.Youtube.Services;

/// <summary>
/// Extracts direct playable HTTP stream URLs via yt-dlp CLI and controls MPD over raw TCP protocol.
/// </summary>
public sealed class YtDlpStreamExtractor : IYtDlpStreamExtractor
{
    private readonly YtDlpOptions _options;
    private readonly ILogger<YtDlpStreamExtractor> _logger;

    public YtDlpStreamExtractor(IOptions<YtDlpOptions> options, ILogger<YtDlpStreamExtractor> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
    }

    public async Task<YtDlpMediaStream> ExtractStreamAsync(string youtubeUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(youtubeUrl);

        // --get-url (-g) fetches direct stream URL
        // --user-agent fetches exact User-Agent string used during request
        string arguments = $"-g --user-agent -f \"{youtubeUrl}\"";

        _logger.LogInformation("Executing yt-dlp to resolve stream URL for: {Url}", youtubeUrl);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _options.ExecutablePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            string error = await process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
            {
                _logger.LogError("yt-dlp URL extraction failed. Error: {Error}", error);
                throw new InvalidOperationException($"Failed to extract stream URL from yt-dlp: {error}");
            }

            // Standard stdout gives User-Agent on line 1, URL on line 2 (or vice versa based on version)
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
            {
                _logger.LogError("Unexpected stdout format from yt-dlp: {Output}", output);
                throw new InvalidDataException("yt-dlp did not return both stream URL and User-Agent.");
            }

            string userAgent = lines[0].StartsWith("Mozilla/", StringComparison.OrdinalIgnoreCase) ? lines[0] : lines[1];
            string directUrl = lines[0].StartsWith("http", StringComparison.OrdinalIgnoreCase) ? lines[0] : lines[1];

            _logger.LogInformation("Successfully extracted direct stream URL.");

            return new YtDlpMediaStream(directUrl.Trim(), userAgent.Trim(), TimeSpan.Zero);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Stream extraction timed out or was cancelled.");
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
            throw;
        }
    }

}
