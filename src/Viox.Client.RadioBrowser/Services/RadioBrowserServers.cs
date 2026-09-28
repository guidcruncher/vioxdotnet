using System.Net;

namespace Viox.Client.RadioBrowser.Services;

/// <summary>
/// Known Radio Browser mirrors and DNS-based discovery helpers.
/// </summary>
/// <remarks>
/// Official guidance is to resolve <c>all.api.radio-browser.info</c>, randomize the resulting
/// hosts, and talk to one HTTPS origin. Well-known host names are kept as a fallback when DNS
/// is unavailable.
/// </remarks>
public static class RadioBrowserServers
{
    /// <summary>
    /// DNS name that resolves to every currently advertised API mirror.
    /// </summary>
    public const string DiscoveryHost = "all.api.radio-browser.info";

    /// <summary>
    /// Well-known public mirrors published by the Radio Browser project.
    /// Any of these may disappear; prefer <see cref="DiscoverHostNamesAsync"/> at startup.
    /// </summary>
    public static readonly IReadOnlyList<string> WellKnownHosts =
    [
        "de1.api.radio-browser.info",
        "nl1.api.radio-browser.info",
        "at1.api.radio-browser.info"
    ];

    /// <summary>
    /// Builds an HTTPS origin URI for a mirror host name.
    /// </summary>
    /// <param name="hostName">Host such as <c>de1.api.radio-browser.info</c>.</param>
    /// <returns>Origin URI with a trailing slash omitted.</returns>
    public static Uri CreateBaseAddress(string hostName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostName);
        return new Uri($"https://{hostName.Trim().TrimEnd('/')}");
    }

    /// <summary>
    /// Resolves <see cref="DiscoveryHost"/> and returns distinct host names for available mirrors.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the DNS lookup.</param>
    /// <returns>Distinct host names; empty when DNS yields nothing.</returns>
    public static async Task<IReadOnlyList<string>> DiscoverHostNamesAsync(
        CancellationToken cancellationToken = default)
    {
        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(DiscoveryHost, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            return [];
        }

        var hosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var address in addresses)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var entry = await Dns.GetHostEntryAsync(address.ToString(), cancellationToken)
                    .ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(entry.HostName))
                {
                    hosts.Add(entry.HostName.TrimEnd('.'));
                }
            }
            catch (Exception)
            {
                // Reverse lookup is best-effort; the A/AAAA record is still a usable endpoint
                // when the host name cannot be recovered.
            }
        }

        return [.. hosts];
    }
}
