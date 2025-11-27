using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;

namespace Aivana_RDP_WPF.ViewModels.ConnectionManagement;

/// <summary>
/// ViewModel for connection health metrics display.
/// </summary>
public partial class ConnectionHealthViewModel : ObservableObject
{
    private readonly IPerformanceMonitorService _performanceMonitorService;
    private readonly ILogger<ConnectionHealthViewModel> _logger;
    private int _connectionProfileId;

    [ObservableProperty]
    private PerformanceMetrics? _currentMetrics;

    public ConnectionHealthViewModel(
        IPerformanceMonitorService performanceMonitorService,
        ILogger<ConnectionHealthViewModel> logger)
    {
        _performanceMonitorService = performanceMonitorService;
        _logger = logger;
        
        _performanceMonitorService.MetricsUpdated += OnMetricsUpdated;
    }

    public void LoadMetrics(int connectionProfileId)
    {
        _connectionProfileId = connectionProfileId;
        _logger.LogInformation("Loading health metrics for connection {ConnectionId}", connectionProfileId);
        
        _ = Task.Run(async () =>
        {
            await _performanceMonitorService.StartMonitoringAsync(connectionProfileId);
            var metrics = await _performanceMonitorService.GetCurrentMetricsAsync(connectionProfileId);
            if (metrics != null)
            {
                CurrentMetrics = metrics;
            }
        });
    }

    private void OnMetricsUpdated(object? sender, PerformanceMetrics metrics)
    {
        if (metrics.ConnectionProfileId == _connectionProfileId)
        {
            CurrentMetrics = metrics;
        }
    }

    [RelayCommand]
    private async Task RefreshMetricsAsync()
    {
        var metrics = await _performanceMonitorService.GetCurrentMetricsAsync(_connectionProfileId);
        if (metrics != null)
        {
            CurrentMetrics = metrics;
        }
    }
}

