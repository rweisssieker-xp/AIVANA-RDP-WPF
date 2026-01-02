using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for SSH tunnel functionality
/// </summary>
public interface ISshTunnelService 
{
    Task<TunnelResult> CreateTunnelAsync(SshTunnelConfiguration config, CancellationToken ct = default);
    Task CloseTunnelAsync(string tunnelId, CancellationToken ct = default);
    Task<bool> IsTunnelActiveAsync(string tunnelId, CancellationToken ct = default);
    Task<List<TunnelInfo>> GetActiveTunnelsAsync(CancellationToken ct = default);
}
