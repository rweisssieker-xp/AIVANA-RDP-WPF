using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for Wake-on-LAN functionality
/// </summary>
public interface IWakeOnLanService 
{
    Task<WakeResult> SendWakePacketAsync(string macAddress, string broadcastAddress, CancellationToken ct = default);
    Task<bool> WaitForHostAsync(string hostAddress, TimeSpan timeout, CancellationToken ct = default);
    Task<WakeResult> WakeConnectionAsync(ConnectionProfile profile, CancellationToken ct = default);
    Task<bool> ValidateMacAddressAsync(string macAddress);
}
