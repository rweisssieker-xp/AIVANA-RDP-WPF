using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Enhanced service interface for real performance monitoring
/// </summary>
public interface IPerformanceMonitorService
{
    Task StartMonitoringAsync(int connectionProfileId, CancellationToken ct = default);
    Task StopMonitoringAsync(int connectionProfileId, CancellationToken ct = default);
    Task<PerformanceMetrics?> GetCurrentMetricsAsync(int connectionProfileId, CancellationToken ct = default);
    Task<List<PerformanceMetrics>> GetHistoricalMetricsAsync(int connectionProfileId, DateTime startTime, DateTime endTime, CancellationToken ct = default);
    Task<List<PerformanceMetrics>> GetMetricsHistoryAsync(int connectionProfileId, TimeSpan period, CancellationToken ct = default);
    event EventHandler<PerformanceMetrics>? MetricsUpdated;
    event EventHandler<ConnectionQuality>? QualityChanged;
}

