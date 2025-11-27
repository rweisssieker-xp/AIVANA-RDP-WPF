using System.Text.RegularExpressions;

namespace Aivana_RDP_WPF.Helpers;

/// <summary>
/// Helper class for validation operations.
/// </summary>
public static class ValidationHelper
{
    private static readonly Regex IpAddressRegex = new(@"^(\d{1,3}\.){3}\d{1,3}$", RegexOptions.Compiled);
    private static readonly Regex HostnameRegex = new(@"^([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$|^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?$", RegexOptions.Compiled);

    /// <summary>
    /// Validates a server address (IP or hostname).
    /// </summary>
    public static bool IsValidServerAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;

        return IpAddressRegex.IsMatch(address) || HostnameRegex.IsMatch(address);
    }

    /// <summary>
    /// Validates a port number.
    /// </summary>
    public static bool IsValidPort(int port)
    {
        return port >= 1 && port <= 65535;
    }

    /// <summary>
    /// Validates a port number from string.
    /// </summary>
    public static bool IsValidPort(string? portString, out int port)
    {
        port = 0;
        if (string.IsNullOrWhiteSpace(portString))
            return false;

        if (!int.TryParse(portString, out port))
            return false;

        return IsValidPort(port);
    }
}

