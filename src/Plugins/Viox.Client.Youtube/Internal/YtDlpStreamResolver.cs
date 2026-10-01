using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Youtube.Configuration;

namespace Viox.Client.Youtube.Internal;

internal sealed class YtDlpStreamResolver
{
    private readonly YoutubeMusicOptions _options;
    private readonly ILogger<YtDlpStreamResolver> _logger;

    public YtDlpStreamResolver(IOptions<YoutubeMusicOptions> options, ILogger<YtDlpStreamResolver> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string?> TryResolveAsync(string trackUrl, CancellationToken cancellationToken)
    {
        if (!_options.EnableYtDlpFallback)
        {
            return null;
        }

        var tool = string.IsNullOrWhiteSpace(_options.YtDlpPath) ? "yt-dlp" : _options.YtDlpPath;
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = tool,
                    ArgumentList = { "-f", "bestaudio/best", "-g", "--no-playlist", trackUrl },
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (!process.Start())
            {
                return null;
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_options.RequestTimeout);

            try
            {
                await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                TryKill(process);
                _logger.LogWarning("yt-dlp timed out while resolving {Url}", trackUrl);
                return null;
            }

            var stdout = (await stdoutTask.ConfigureAwait(false)).Trim();
            var stderr = await stderrTask.ConfigureAwait(false);

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(stdout))
            {
                _logger.LogDebug("yt-dlp failed for {Url} (exit {Exit}): {Error}", trackUrl, process.ExitCode, stderr.Trim());
                return null;
            }

            var url = stdout.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return string.IsNullOrWhiteSpace(url) ? null : url;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or FileNotFoundException or InvalidOperationException)
        {
            _logger.LogDebug(ex, "yt-dlp is not available at {Path}", tool);
            return null;
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // ignored
        }
    }
}
