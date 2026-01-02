using Aivana_RDP_WPF.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Implementation of Wake-on-LAN service
/// </summary>
public class WakeOnLanService : IWakeOnLanService 
{
    private readonly ILogger<WakeOnLanService> _logger;

    public WakeOnLanService(ILogger<WakeOnLanService> logger)
    {
        _logger = logger;
    }

    public async Task<WakeResult> SendWakePacketAsync(string macAddress, string broadcastAddress, CancellationToken ct = default)
    {
        try
        {
            if (!await ValidateMacAddressAsync(macAddress))
            {
                return WakeResult.Failed("Invalid MAC address format", macAddress, broadcastAddress);
            }

            _logger.LogInformation("Sending Wake-on-LAN packet to {MacAddress} via {BroadcastAddress}", 
                macAddress, broadcastAddress);

            var macBytes = ParseMacAddress(macAddress);
            
            var packet = new byte[6 + 16 * 6];
            
            for (int i = 0; i < 6; i++)
            {
                packet[i] = 0xFF;
            }
            
            for (int i = 0; i < 16; i++)
            {
                Array.Copy(macBytes, 0, packet, 6 + i * 6, 6);
            }

            using var udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            
            await udpClient.SendAsync(packet, packet.Length, broadcastAddress, 9);
            await udpClient.SendAsync(packet, packet.Length, broadcastAddress, 7);

            _logger.LogInformation("Wake-on-LAN packet sent successfully to {MacAddress}", macAddress);

            return WakeResult.Successful(macAddress, broadcastAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Wake-on-LAN packet to {MacAddress}", macAddress);
            return WakeResult.Failed($"Failed to send wake packet: {ex.Message}", macAddress, broadcastAddress);
        }
    }

    public async Task<bool> WaitForHostAsync(string hostAddress, TimeSpan timeout, CancellationToken ct = default)
    {
        _logger.LogInformation("Waiting for host {HostAddress} to come online (timeout: {Timeout}s)", 
            hostAddress, timeout.TotalSeconds);

        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(hostAddress, 2000);
                
                if (reply.Status == IPStatus.Success)
                {
                    _logger.LogInformation("Host {HostAddress} is online after {ElapsedMs}ms", 
                        hostAddress, (DateTime.UtcNow - startTime).TotalMilliseconds);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Ping failed for {HostAddress}: {Error}", hostAddress, ex.Message);
            }

            await Task.Delay(2000, ct);
        }

        _logger.LogWarning("Host {HostAddress} did not come online within timeout", hostAddress);
        return false;
    }

    public async Task<WakeResult> WakeConnectionAsync(ConnectionProfile profile, CancellationToken ct = default)
    {
        if (!profile.EnableWakeOnLan || string.IsNullOrEmpty(profile.MacAddress))
        {
            return WakeResult.Failed("Wake-on-LAN not enabled or MAC address not configured", 
                profile.MacAddress ?? "", "");
        }

        try
        {
            var broadcastAddress = GetBroadcastAddress(profile.ServerAddress);
            
            var wakeResult = await SendWakePacketAsync(profile.MacAddress, broadcastAddress, ct);
            
            if (!wakeResult.Success)
            {
                return wakeResult;
            }

            var timeout = TimeSpan.FromSeconds(profile.WakeTimeoutSeconds);
            var hostCameOnline = await WaitForHostAsync(profile.ServerAddress, timeout, ct);
            
            if (hostCameOnline)
            {
                return WakeResult.Successful(profile.MacAddress, broadcastAddress, 
                    $"Host {profile.ServerAddress} is online and ready for connection");
            }
            else
            {
                return WakeResult.Failed($"Host {profile.ServerAddress} did not respond within {profile.WakeTimeoutSeconds} seconds", 
                    profile.MacAddress, broadcastAddress);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to wake connection for profile {ProfileName}", profile.Name);
            return WakeResult.Failed($"Wake-on-LAN failed: {ex.Message}", profile.MacAddress ?? "", "");
        }
    }

    public async Task<bool> ValidateMacAddressAsync(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
            return false;

        var patterns = new[]
        {
            @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$",
            @"^([0-9A-Fa-f]{4}\.){2}([0-9A-Fa-f]{4})$",
            @"^[0-9A-Fa-f]{12}$"
        };

        foreach (var pattern in patterns)
        {
            if (Regex.IsMatch(macAddress, pattern))
            {
                return await Task.FromResult(true);
            }
        }

        return await Task.FromResult(false);
    }

    private byte[] ParseMacAddress(string macAddress)
    {
        var cleanMac = Regex.Replace(macAddress, @"[^0-9A-Fa-f]", "");
        
        if (cleanMac.Length != 12)
        {
            throw new ArgumentException("Invalid MAC address format");
        }

        var macBytes = new byte[6];
        for (int i = 0; i < 6; i++)
        {
            macBytes[i] = Convert.ToByte(cleanMac.Substring(i * 2, 2), 16);
        }

        return macBytes;
    }

    private string GetBroadcastAddress(string hostAddress)
    {
        try
        {
            var hostEntry = Dns.GetHostEntry(hostAddress);
            var ipAddress = hostEntry.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            
            if (ipAddress != null)
            {
                var bytes = ipAddress.GetAddressBytes();
                
                if (IPAddress.IsLoopback(ipAddress))
                    return "127.0.0.255";
                
                if (bytes[0] == 192 && bytes[1] == 168)
                {
                    return $"192.168.{bytes[2]}.255";
                }
                
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                {
                    return $"172.{bytes[1]}.255.255";
                }
                
                if (bytes[0] == 10)
                {
                    return $"10.255.255.255";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Could not determine broadcast address for {HostAddress}: {Error}", 
                hostAddress, ex.Message);
        }

        return "255.255.255.255";
    }
}
