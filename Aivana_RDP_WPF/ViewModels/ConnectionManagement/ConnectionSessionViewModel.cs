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
    private readonly ICredentialService? _credentialService;
    private readonly IPerformanceMonitorService? _performanceMonitorService;
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

    [ObservableProperty]
    private bool _showHealthMetrics;

    [ObservableProperty]
    private ConnectionHealthViewModel? _healthViewModel;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isFullScreen;

    public ConnectionSessionViewModel(
        IRdpConnectionService rdpConnectionService,
        ILogger<ConnectionSessionViewModel> logger,
        ICredentialService? credentialService = null,
        IPerformanceMonitorService? performanceMonitorService = null)
    {
        _rdpConnectionService = rdpConnectionService;
        _credentialService = credentialService;
        _performanceMonitorService = performanceMonitorService;
        _logger = logger;
    }

    public void LoadProfile(ConnectionProfile profile)
    {
        _profile = profile;
        OnPropertyChanged(nameof(Profile));
        StatusMessage = $"Ready to connect to {profile.Name} ({profile.ServerAddress})";
        
        // Initialize health view model
        if (_performanceMonitorService != null)
        {
            var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
            HealthViewModel = new ConnectionHealthViewModel(_performanceMonitorService, 
                loggerFactory.CreateLogger<ConnectionHealthViewModel>());
            HealthViewModel.LoadMetrics(profile.Id);
        }
    }

    [RelayCommand]
    private void ToggleHealthMetrics()
    {
        ShowHealthMetrics = !ShowHealthMetrics;
    }

    [RelayCommand]
    private void ToggleFullScreen()
    {
        if (_profile == null || !IsConnected)
            return;

        IsFullScreen = !IsFullScreen;
        _rdpConnectionService.SetFullScreen(_profile.Id, IsFullScreen);
        _logger.LogInformation("Full screen toggled to {FullScreen}", IsFullScreen);
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
            
            // Get credentials if available
            string? password = null;
            if (_credentialService != null && _profile.Id > 0)
            {
                var credentials = await _credentialService.GetCredentialsAsync(_profile.Id);
                password = credentials?.Password;
            }
            
            // Connect
            await _rdpConnectionService.ConnectAsync(_profile, password);
            IsConnected = true;
            StatusMessage = "Connected";
            
            // Start performance monitoring
            if (_performanceMonitorService != null && _profile.Id > 0)
            {
                await _performanceMonitorService.StartMonitoringAsync(_profile.Id);
            }
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
    private async Task DisconnectAsync()
    {
        if (_profile == null || !IsConnected)
            return;

        try
        {
            _logger.LogInformation("Disconnecting from {Server}:{Port}", _profile.ServerAddress, _profile.Port);
            
            // Stop performance monitoring
            if (_performanceMonitorService != null && _profile.Id > 0)
            {
                await _performanceMonitorService.StopMonitoringAsync(_profile.Id);
            }
            
            _rdpConnectionService.Disconnect(_profile.Id);
            IsConnected = false;
            IsFullScreen = false; // Reset full screen state
            StatusMessage = "Disconnected";
            RdpHost = null; // Clear the host when disconnected
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from {Server}:{Port}", _profile.ServerAddress, _profile.Port);
            StatusMessage = $"Disconnect error: {ex.Message}";
        }
    }
}

