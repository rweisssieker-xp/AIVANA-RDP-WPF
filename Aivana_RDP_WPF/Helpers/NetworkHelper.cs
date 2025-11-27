using System.Net.NetworkInformation;
using System.Net;

namespace Aivana_RDP_WPF.Helpers;

/// <summary>
/// Helper class for network operations.
/// </summary>
public static class NetworkHelper
{
    /// <summary>
    /// Pings a server to check connectivity.
    /// </summary>
    public static async Task<bool> PingAsync(string hostnameOrAddress, int timeoutMs = 3000)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(hostnameOrAddress, timeoutMs);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Resolves a hostname to IP address.
    /// </summary>
    public static async Task<string?> ResolveHostnameAsync(string hostname)
    {
        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(hostname);
            return hostEntry.AddressList.FirstOrDefault()?.ToString();
        }
        catch
        {
            return null;
        }
    }
}

