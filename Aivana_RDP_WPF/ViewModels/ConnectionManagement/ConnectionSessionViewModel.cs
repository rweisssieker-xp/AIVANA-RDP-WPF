using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;

namespace Aivana_RDP_WPF.ViewModels.ConnectionManagement;

/// <summary>
/// ViewModel for managing an active RDP connection session.
/// </summary>
public partial class ConnectionSessionViewModel : ObservableObject
{
    private readonly IRdpConnectionService _rdpConnectionService;
    private readonly ILogger<ConnectionSessionViewModel> _logger;
    private readonly ConnectionProfile _profile;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isConnecting;

    [ObservableProperty]
    private string _statusMessage = "Ready to connect";

    public ConnectionProfile Profile => _profile;

    public ConnectionSessionViewModel(
        ConnectionProfile profile,
        IRdpConnectionService rdpConnectionService,
        ILogger<ConnectionSessionViewModel> logger)
    {
        _profile = profile;
        _rdpConnectionService = rdpConnectionService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (IsConnected || IsConnecting)
            return;

        IsConnecting = true;
        StatusMessage = "Connecting...";

        try
        {
            _logger.LogInformation("Connecting to {Server}:{Port}", _profile.ServerAddress, _profile.Port);
            _rdpConnectionService.Connect(_profile);
            IsConnected = true;
            StatusMessage = "Connected";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to {Server}:{Port}", _profile.ServerAddress, _profile.Port);
            StatusMessage = $"Connection failed: {ex.Message}";
        }
        finally
        {
            IsConnecting = false;
        }
    }

    [RelayCommand]
    private void Disconnect()
    {
        if (!IsConnected)
            return;

        try
        {
            _logger.LogInformation("Disconnecting from {Server}:{Port}", _profile.ServerAddress, _profile.Port);
            _rdpConnectionService.Disconnect(_profile.Id);
            IsConnected = false;
            StatusMessage = "Disconnected";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from {Server}:{Port}", _profile.ServerAddress, _profile.Port);
            StatusMessage = $"Disconnect error: {ex.Message}";
        }
    }
}

