using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for performance monitoring.
/// </summary>
public interface IPerformanceMonitorService
{
    Task StartMonitoringAsync(int connectionProfileId, CancellationToken ct = default);
    Task StopMonitoringAsync(int connectionProfileId, CancellationToken ct = default);
    Task<PerformanceMetrics?> GetCurrentMetricsAsync(int connectionProfileId, CancellationToken ct = default);
    Task<List<PerformanceMetrics>> GetHistoricalMetricsAsync(int connectionProfileId, DateTime startTime, DateTime endTime, CancellationToken ct = default);
    event EventHandler<PerformanceMetrics>? MetricsUpdated;
}

