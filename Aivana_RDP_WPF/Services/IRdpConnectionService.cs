using System.Windows.Forms.Integration;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for RDP connection management.
/// </summary>
public interface IRdpConnectionService
{
    WindowsFormsHost? CreateConnectionHost(ConnectionProfile profile);
    void Connect(ConnectionProfile profile);
    void Disconnect(int profileId);
}

