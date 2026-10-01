using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Viox.Client.Youtube.Configuration;
using Viox.Client.Youtube.Models;

namespace Viox.Client.Youtube.Services;

/// <summary>
/// Service implementation for extracting direct playable HTTP stream URLs using yt-dlp CLI.
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

        _logger.LogInformation("Executing yt-dlp to resolve stream URL for: {Url}", youtubeUrl);

        var startInfo = new ProcessStartInfo
        {
            FileName = _options.ExecutablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Output strictly the direct stream URL
        startInfo.ArgumentList.Add("-g");
        startInfo.ArgumentList.Add("--js-runtimes=deno");
        startInfo.ArgumentList.Add("-f");
        startInfo.ArgumentList.Add("ba[protocol^=http]/bestaudio[protocol^=http]");
        startInfo.ArgumentList.Add(youtubeUrl);

        using var process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();

            Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await Task.WhenAll(outputTask, errorTask);
            await process.WaitForExitAsync(cancellationToken);

            string output = outputTask.Result;
            string error = errorTask.Result;

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
            {
                _logger.LogError("yt-dlp URL extraction failed with exit code {ExitCode}. Error output: {Error}", process.ExitCode, error);
                throw new InvalidOperationException($"Failed to extract stream URL from yt-dlp: {error}");
            }

            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string directUrl = lines.FirstOrDefault(line => line.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                                                            line.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidDataException($"Could not locate a valid HTTP/HTTPS media stream URL in yt-dlp output. Raw output: {output}");

            _logger.LogInformation("Successfully extracted direct stream URL.");

            // Fallback User-Agent string provided since yt-dlp is configured to return only the URL
            const string fallbackUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

            return new YtDlpMediaStream(directUrl, fallbackUserAgent, TimeSpan.Zero);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Stream extraction operation was cancelled or timed out.");
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
            throw;
        }
        catch (Exception ex) when (ex is not InvalidOperationException && ex is not InvalidDataException)
        {
            _logger.LogError(ex, "An unhandled exception occurred during yt-dlp execution.");
            throw;
        }
    }
}
