using System.Net;
using System.Net.NetworkInformation;

using Microsoft.Extensions.Logging;

namespace Viox.Client.RadioBrowser.Services;

/// <summary>
/// Provides utility methods for resolving the optimal RadioBrowser API server endpoint.
/// </summary>
public static class RadioBrowserApiServerResolver
{
    private const string BaseUrl = "all.api.radio-browser.info";
    private const string FallbackUrl = "de2.api.radio-browser.info";
    private const int DefaultPingTimeoutMs = 1000;

    /// <summary>
    /// Asynchronously determines the fastest available RadioBrowser API server based on ICMP ping response times.
    /// </summary>
    /// <param name="pingTimeoutMs">Maximum duration in milliseconds to wait for a ping reply per IP address.</param>
    /// <param name="logger">Optional logger instance for recording diagnostic messages.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for task completion.</param>
    /// <returns>The fully qualified hostname or IP address of the fastest server, or the fallback address if resolution fails.</returns>
    public static async Task<string> GetFastestApiUrlAsync(
        int pingTimeoutMs = DefaultPingTimeoutMs,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        logger?.LogDebug("Resolving DNS addresses for base URL: {BaseUrl}", BaseUrl);

        IPAddress[] ips;
        try
        {
            ips = await Dns.GetHostAddressesAsync(BaseUrl, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to resolve DNS for {BaseUrl}. Falling back to {FallbackUrl}", BaseUrl, FallbackUrl);
            return FallbackUrl;
        }

        if (ips.Length == 0)
        {
            logger?.LogWarning("No IP addresses returned for {BaseUrl}. Falling back to {FallbackUrl}", BaseUrl, FallbackUrl);
            return FallbackUrl;
        }

        long minRoundtripTime = long.MaxValue;
        string bestAddress = FallbackUrl;

        using var pinger = new Ping();

        foreach (var ipAddress in ips)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                logger?.LogTrace("Pinging IP address: {IPAddress}", ipAddress);
                var reply = await pinger.SendPingAsync(ipAddress, pingTimeoutMs).ConfigureAwait(false);

                if (reply is { Status: IPStatus.Success } && reply.RoundtripTime < minRoundtripTime)
                {
                    minRoundtripTime = reply.RoundtripTime;
                    bestAddress = ipAddress.ToString();
                    logger?.LogTrace("New fastest candidate: {IPAddress} with {RoundtripTime}ms", bestAddress, minRoundtripTime);
                }
            }
            catch (PingException ex)
            {
                logger?.LogDebug(ex, "Ping request failed for IP address: {IPAddress}", ipAddress);
            }
        }

        if (bestAddress == FallbackUrl)
        {
            logger?.LogWarning("Failed to obtain a successful ping from resolved addresses. Falling back to {FallbackUrl}", FallbackUrl);
            return FallbackUrl;
        }

        try
        {
            logger?.LogDebug("Performing reverse DNS lookup for fastest IP: {BestAddress}", bestAddress);
            var hostEntry = await Dns.GetHostEntryAsync(bestAddress, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(hostEntry.HostName))
            {
                logger?.LogInformation("Resolved fastest RadioBrowser host: {HostName} ({RoundtripTime}ms)", hostEntry.HostName, minRoundtripTime);
                return hostEntry.HostName;
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Reverse DNS lookup failed for IP {BestAddress}. Returning raw IP string.", bestAddress);
        }

        return bestAddress;
    }
}
