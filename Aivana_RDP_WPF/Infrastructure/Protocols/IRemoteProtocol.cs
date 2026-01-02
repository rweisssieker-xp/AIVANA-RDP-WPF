using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Infrastructure.Protocols;

/// <summary>
/// Core interface for all remote desktop protocols
/// </summary>
public interface IRemoteProtocol
{
    ProtocolType ProtocolType { get; }
    string ProtocolName { get; }
    
    /// <summary>
    /// Establishes a connection using the provided profile
    /// </summary>
    Task<ConnectionResult> ConnectAsync(ConnectionProfile profile, CancellationToken ct = default);
    
    /// <summary>
    /// Terminates the current connection
    /// </summary>
    Task DisconnectAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Gets the capabilities supported by this protocol
    /// </summary>
    Task<ProtocolCapabilities> GetCapabilitiesAsync();
    
    /// <summary>
    /// Gets current performance metrics for the active connection
    /// </summary>
    Task<PerformanceMetrics> GetMetricsAsync();
    
    /// <summary>
    /// Fired when performance metrics are updated
    /// </summary>
    event EventHandler<PerformanceMetrics>? MetricsUpdated;
    
    /// <summary>
    /// Fired when connection status changes
    /// </summary>
    event EventHandler<string>? StatusChanged;
    
    /// <summary>
    /// Gets whether the protocol is currently connected
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Gets the current connection session ID
    /// </summary>
    string? SessionId { get; }
}
