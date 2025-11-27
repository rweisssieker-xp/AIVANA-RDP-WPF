using System.Windows.Forms.Integration;
using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Infrastructure.Rdp;

/// <summary>
/// Wrapper for MSTSC ActiveX Control (RDP client).
/// Note: MSTSC ActiveX control requires COM interop and will be implemented with proper COM references.
/// </summary>
public class RdpClientWrapper : IDisposable
{
    private readonly ILogger<RdpClientWrapper> _logger;
    private WindowsFormsHost? _host;

    public RdpClientWrapper(ILogger<RdpClientWrapper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a WindowsFormsHost containing the RDP control.
    /// RDP ActiveX control integration will be implemented in Story 3.1.
    /// </summary>
    public WindowsFormsHost CreateHost()
    {
        _logger.LogInformation("RDP client wrapper - CreateHost called (implementation pending)");
        _host = new WindowsFormsHost();
        // RDP ActiveX control will be added here in Story 3.1
        return _host;
    }

    /// <summary>
    /// Connects to the remote desktop.
    /// </summary>
    public void Connect(string server, int port, string username, string? domain = null)
    {
        _logger.LogInformation("Connecting to {Server}:{Port} as {Username} (implementation pending)", server, port, username);
        // RDP connection logic will be implemented in Story 3.1
    }

    /// <summary>
    /// Disconnects from the remote desktop.
    /// </summary>
    public void Disconnect()
    {
        _logger.LogInformation("Disconnecting from RDP session (implementation pending)");
        // RDP disconnection logic will be implemented in Story 3.1
    }

    public void Dispose()
    {
        Disconnect();
        _host?.Dispose();
    }
}

