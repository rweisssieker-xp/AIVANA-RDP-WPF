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
    private ConnectionProfile? _profile;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isConnecting;

    [ObservableProperty]
    private string _statusMessage = "Ready to connect";

    [ObservableProperty]
    private System.Windows.Forms.Integration.WindowsFormsHost? _rdpHost;

    public ConnectionProfile? Profile => _profile;

    public ConnectionSessionViewModel(
        IRdpConnectionService rdpConnectionService,
        ILogger<ConnectionSessionViewModel> logger)
    {
        _rdpConnectionService = rdpConnectionService;
        _logger = logger;
    }

    public void LoadProfile(ConnectionProfile profile)
    {
        _profile = profile;
        OnPropertyChanged(nameof(Profile));
        StatusMessage = $"Ready to connect to {profile.ServerAddress}";
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (_profile == null)
        {
            StatusMessage = "No profile selected";
            return;
        }

        if (IsConnected || IsConnecting)
            return;

        IsConnecting = true;
        StatusMessage = "Connecting...";

        try
        {
            _logger.LogInformation("Connecting to {Server}:{Port}", _profile.ServerAddress, _profile.Port);
            
            // Create RDP host if not exists
            if (RdpHost == null)
            {
                RdpHost = _rdpConnectionService.CreateConnectionHost(_profile);
                OnPropertyChanged(nameof(RdpHost));
            }
            
            // Connect
            await _rdpConnectionService.ConnectAsync(_profile);
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
        if (_profile == null || !IsConnected)
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

