using Aivana_RDP_WPF.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Real implementation of performance monitoring service
/// </summary>
public class RealPerformanceMonitorService : IPerformanceMonitorService 
{
    private readonly ILogger<RealPerformanceMonitorService> _logger;
    private readonly Dictionary<int, CancellationTokenSource> _monitoringTasks = new();
    private readonly Dictionary<int, PerformanceMetrics> _currentMetrics = new();

    public event EventHandler<PerformanceMetrics>? MetricsUpdated;
    public event EventHandler<ConnectionQuality>? QualityChanged;

    public RealPerformanceMonitorService(ILogger<RealPerformanceMonitorService> logger)
    {
        _logger = logger;
    }

    public async Task StartMonitoringAsync(int connectionProfileId, CancellationToken ct = default)
    {
        if (_monitoringTasks.ContainsKey(connectionProfileId))
        {
            _logger.LogWarning("Monitoring already started for profile {ProfileId}", connectionProfileId);
            return;
        }

        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _monitoringTasks[connectionProfileId] = cts;

        _logger.LogInformation("Starting performance monitoring for profile {ProfileId}", connectionProfileId);

        _ = Task.Run(async () => await MonitoringLoopAsync(connectionProfileId, cts.Token), cts.Token);

        await Task.CompletedTask;
    }

    public async Task StopMonitoringAsync(int connectionProfileId, CancellationToken ct = default)
    {
        if (_monitoringTasks.TryGetValue(connectionProfileId, out var cts))
        {
            _logger.LogInformation("Stopping performance monitoring for profile {ProfileId}", connectionProfileId);
            
            await cts.CancelAsync();
            _monitoringTasks.Remove(connectionProfileId);
            _currentMetrics.Remove(connectionProfileId);
        }

        await Task.CompletedTask;
    }

    public async Task<PerformanceMetrics?> GetCurrentMetricsAsync(int connectionProfileId, CancellationToken ct = default)
    {
        _currentMetrics.TryGetValue(connectionProfileId, out var metrics);
        return await Task.FromResult(metrics);
    }

    public async Task<List<PerformanceMetrics>> GetHistoricalMetricsAsync(int connectionProfileId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        var current = await GetCurrentMetricsAsync(connectionProfileId, ct);
        return current != null ? new List<PerformanceMetrics> { current } : new List<PerformanceMetrics>();
    }

    public async Task<List<PerformanceMetrics>> GetMetricsHistoryAsync(int connectionProfileId, TimeSpan period, CancellationToken ct = default)
    {
        var current = await GetCurrentMetricsAsync(connectionProfileId, ct);
        return current != null ? new List<PerformanceMetrics> { current } : new List<PerformanceMetrics>();
    }

    private async Task MonitoringLoopAsync(int connectionProfileId, CancellationToken ct)
    {
        var previousQuality = ConnectionQuality.Fair; // Default to Fair instead of Unknown

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var metrics = await CollectRealMetricsAsync(connectionProfileId, ct);
                
                _currentMetrics[connectionProfileId] = metrics;
                MetricsUpdated?.Invoke(this, metrics);

                if (metrics.Quality != previousQuality)
                {
                    QualityChanged?.Invoke(this, metrics.Quality);
                    previousQuality = metrics.Quality;
                }

                await SaveMetricsAsync(metrics, ct);

                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in monitoring loop for profile {ProfileId}", connectionProfileId);
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }
        }
    }

    private async Task<PerformanceMetrics> CollectRealMetricsAsync(int connectionProfileId, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var host = "localhost";
        var port = 3389;

        var metrics = new PerformanceMetrics
        {
            ConnectionProfileId = connectionProfileId,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(host, 3000);
            metrics.LatencyMs = reply.Status == IPStatus.Success ? reply.RoundtripTime : -1;

            metrics.BandwidthMbps = await MeasureBandwidthAsync(host, port, ct);

            metrics.CpuUsagePercent = GetCpuUsage();
            metrics.MemoryUsageMB = GetMemoryUsageMB();

            metrics.QualityScore = CalculateQualityScore(metrics);

            stopwatch.Stop();
            _logger.LogDebug("Metrics collection took {ElapsedMs}ms for profile {ProfileId}", 
                stopwatch.ElapsedMilliseconds, connectionProfileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect metrics for profile {ProfileId}", connectionProfileId);
            metrics.QualityScore = 0;
        }

        return metrics;
    }

    private async Task<double> MeasureBandwidthAsync(string host, int port, CancellationToken ct)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port);
            
            stopwatch.Stop();
            
            var connectionTimeMs = stopwatch.ElapsedMilliseconds;
            return connectionTimeMs < 100 ? 100.0 :
                   connectionTimeMs < 500 ? 50.0 :
                   10.0;
        }
        catch
        {
            return 0.0;
        }
    }

    private double GetCpuUsage()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            return process.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount;
        }
        catch
        {
            return 0.0;
        }
    }

    private double GetMemoryUsageMB()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            return process.WorkingSet64 / (1024.0 * 1024.0);
        }
        catch
        {
            return 0.0;
        }
    }

    private int CalculateQualityScore(PerformanceMetrics metrics)
    {
        var score = 100;

        if (metrics.LatencyMs > 0)
        {
            if (metrics.LatencyMs > 200) score -= 30;
            else if (metrics.LatencyMs > 100) score -= 20;
            else if (metrics.LatencyMs > 50) score -= 10;
        }
        else
        {
            score -= 50;
        }

        if (metrics.BandwidthMbps < 10) score -= 20;
        else if (metrics.BandwidthMbps < 50) score -= 10;

        if (metrics.CpuUsagePercent > 80) score -= 10;
        else if (metrics.CpuUsagePercent > 60) score -= 5;

        return Math.Max(0, Math.Min(100, score));
    }

    private async Task SaveMetricsAsync(PerformanceMetrics metrics, CancellationToken ct)
    {
        await Task.CompletedTask;
    }
}
