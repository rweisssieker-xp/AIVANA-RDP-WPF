using System.Windows.Forms.Integration;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for RDP connection management.
/// </summary>
public interface IRdpConnectionService
{
    WindowsFormsHost? CreateConnectionHost(ConnectionProfile profile);
    void Connect(ConnectionProfile profile, string? password = null);
    Task ConnectAsync(ConnectionProfile profile, string? password = null);
    void Disconnect(int profileId);
    bool IsConnected(int profileId);
    void SetFullScreen(int profileId, bool fullScreen);
    void RefreshScaling(int profileId);
}

