using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for performance monitoring.
/// </summary>
public class PerformanceMonitorService : IPerformanceMonitorService
{
    private readonly ILogger<PerformanceMonitorService> _logger;
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _monitoringTasks = new();
    private readonly ConcurrentDictionary<int, PerformanceMetrics> _currentMetrics = new();
    private readonly ConcurrentDictionary<int, List<PerformanceMetrics>> _historicalMetrics = new();

    public event EventHandler<PerformanceMetrics>? MetricsUpdated;
    public event EventHandler<ConnectionQuality>? QualityChanged;

    public PerformanceMonitorService(ILogger<PerformanceMonitorService> logger)
    {
        _logger = logger;
    }

    public Task StartMonitoringAsync(int connectionProfileId, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting performance monitoring for connection {ConnectionId}", connectionProfileId);
        
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _monitoringTasks[connectionProfileId] = cts;
        
        _ = Task.Run(async () => await MonitorPerformanceAsync(connectionProfileId, cts.Token), cts.Token);
        
        return Task.CompletedTask;
    }

    public Task StopMonitoringAsync(int connectionProfileId, CancellationToken ct = default)
    {
        _logger.LogInformation("Stopping performance monitoring for connection {ConnectionId}", connectionProfileId);
        
        if (_monitoringTasks.TryRemove(connectionProfileId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
        
        return Task.CompletedTask;
    }

    public Task<PerformanceMetrics?> GetCurrentMetricsAsync(int connectionProfileId, CancellationToken ct = default)
    {
        _currentMetrics.TryGetValue(connectionProfileId, out var metrics);
        return Task.FromResult(metrics);
    }

    public Task<List<PerformanceMetrics>> GetHistoricalMetricsAsync(int connectionProfileId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        if (_historicalMetrics.TryGetValue(connectionProfileId, out var history))
        {
            var filtered = history
                .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
                .OrderBy(m => m.Timestamp)
                .ToList();
            return Task.FromResult(filtered);
        }
        return Task.FromResult(new List<PerformanceMetrics>());
    }

    public Task<List<PerformanceMetrics>> GetMetricsHistoryAsync(int connectionProfileId, TimeSpan period, CancellationToken ct = default)
    {
        var endTime = DateTime.UtcNow;
        var startTime = endTime - period;
        return GetHistoricalMetricsAsync(connectionProfileId, startTime, endTime, ct);
    }

    private async Task MonitorPerformanceAsync(int connectionProfileId, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var metrics = new PerformanceMetrics
                {
                    ConnectionProfileId = connectionProfileId,
                    Timestamp = DateTime.UtcNow
                };

                // Measure latency (simplified - ping localhost)
                try
                {
                    var ping = new Ping();
                    var reply = await ping.SendPingAsync("127.0.0.1", 1000);
                    metrics.LatencyMs = reply.RoundtripTime;
                }
                catch { metrics.LatencyMs = 0; }

                // Get CPU usage
                var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                metrics.CpuUsagePercent = cpuCounter.NextValue();

                // Get memory usage
                var process = Process.GetCurrentProcess();
                metrics.MemoryUsageMB = process.WorkingSet64 / (1024.0 * 1024.0);

                // Simulate other metrics (in real implementation, get from RDP session)
                metrics.BandwidthMbps = 10.0 + new Random().NextDouble() * 5.0;
                metrics.FrameRate = 30.0 + new Random().NextDouble() * 30.0;
                metrics.QualityScore = 80 + new Random().Next(0, 20);
                metrics.NetworkUsageMbps = metrics.BandwidthMbps * 0.7;

                _currentMetrics[connectionProfileId] = metrics;
                
                var history = _historicalMetrics.GetOrAdd(connectionProfileId, _ => new List<PerformanceMetrics>());
                history.Add(metrics);
                
                // Keep only last 1000 metrics
                if (history.Count > 1000)
                {
                    history.RemoveAt(0);
                }

                MetricsUpdated?.Invoke(this, metrics);
                
                await Task.Delay(1000, ct); // Update every second
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring performance for connection {ConnectionId}", connectionProfileId);
                await Task.Delay(5000, ct);
            }
        }
    }
}

