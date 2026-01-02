using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;
using System.Collections.ObjectModel;

namespace Aivana_RDP_WPF.ViewModels.ConnectionManagement;

/// <summary>
/// ViewModel for managing an active RDP connection session.
/// </summary>
public partial class ConnectionSessionViewModel : ObservableObject
{
    private readonly IRdpConnectionService _rdpConnectionService;
    private readonly ICredentialService? _credentialService;
    private readonly IPerformanceMonitorService? _performanceMonitorService;
    private readonly IHealthCheckService? _healthCheckService;
    private readonly ISessionHistoryService? _sessionHistoryService;
    private readonly ILogger<ConnectionSessionViewModel> _logger;
    private ConnectionProfile? _profile;
    private int? _activeSessionHistoryId;

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

    [ObservableProperty]
    private bool _isHealthChecking;

    [ObservableProperty]
    private HealthCheckResult? _healthCheckResult;

    [ObservableProperty]
    private ObservableCollection<SessionHistory> _recentSessionHistory = new();

    public ConnectionSessionViewModel(
        IRdpConnectionService rdpConnectionService,
        ILogger<ConnectionSessionViewModel> logger,
        ICredentialService? credentialService = null,
        IPerformanceMonitorService? performanceMonitorService = null,
        IHealthCheckService? healthCheckService = null,
        ISessionHistoryService? sessionHistoryService = null)
    {
        _rdpConnectionService = rdpConnectionService;
        _credentialService = credentialService;
        _performanceMonitorService = performanceMonitorService;
        _healthCheckService = healthCheckService;
        _sessionHistoryService = sessionHistoryService;
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

        _ = Task.Run(LoadRecentHistoryAsync);
    }

    private async Task LoadRecentHistoryAsync()
    {
        try
        {
            if (_profile == null || _sessionHistoryService == null || _profile.Id <= 0)
                return;

            var items = await _sessionHistoryService.GetRecentAsync(_profile.Id, 25);
            App.Current.Dispatcher.Invoke(() =>
            {
                RecentSessionHistory.Clear();
                foreach (var item in items)
                {
                    RecentSessionHistory.Add(item);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load session history");
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

            if (_sessionHistoryService != null && _profile.Id > 0)
            {
                var history = await _sessionHistoryService.StartSessionAsync(_profile.Id);
                _activeSessionHistoryId = history.Id;
            }
            
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

            _ = Task.Run(LoadRecentHistoryAsync);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to {Server}:{Port}", _profile.ServerAddress, _profile.Port);
            StatusMessage = $"Connection failed: {ex.Message}";

            if (_sessionHistoryService != null && _activeSessionHistoryId.HasValue)
            {
                await _sessionHistoryService.CompleteSessionAsync(_activeSessionHistoryId.Value, "Error", ex.Message);
                _activeSessionHistoryId = null;
                _ = Task.Run(LoadRecentHistoryAsync);
            }
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

            if (_sessionHistoryService != null && _activeSessionHistoryId.HasValue)
            {
                await _sessionHistoryService.CompleteSessionAsync(_activeSessionHistoryId.Value, "Disconnected", null);
                _activeSessionHistoryId = null;
                _ = Task.Run(LoadRecentHistoryAsync);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from {Server}:{Port}", _profile.ServerAddress, _profile.Port);
            StatusMessage = $"Disconnect error: {ex.Message}";

            if (_sessionHistoryService != null && _activeSessionHistoryId.HasValue)
            {
                await _sessionHistoryService.CompleteSessionAsync(_activeSessionHistoryId.Value, "Error", ex.Message);
                _activeSessionHistoryId = null;
                _ = Task.Run(LoadRecentHistoryAsync);
            }
        }
    }

    [RelayCommand]
    private async Task RunHealthCheckAsync()
    {
        if (_profile == null || _healthCheckService == null)
            return;

        try
        {
            IsHealthChecking = true;
            HealthCheckResult = null;
            var result = await _healthCheckService.CheckAsync(_profile);
            HealthCheckResult = result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            HealthCheckResult = new HealthCheckResult { ErrorMessage = ex.Message, Port = _profile.Port };
        }
        finally
        {
            IsHealthChecking = false;
        }
    }
}

