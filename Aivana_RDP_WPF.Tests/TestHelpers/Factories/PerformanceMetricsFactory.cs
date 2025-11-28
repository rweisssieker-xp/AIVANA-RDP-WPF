using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Tests.TestHelpers.Factories;

/// <summary>
/// Factory for creating test PerformanceMetrics instances
/// </summary>
public static class PerformanceMetricsFactory
{
    public static PerformanceMetrics Create(
        int connectionProfileId = 1,
        double latencyMs = 50.0,
        double bandwidthMbps = 10.0,
        int packetLossPercent = 0,
        double frameRate = 30.0,
        int qualityScore = 80,
        double cpuUsagePercent = 25.0,
        double memoryUsageMB = 512.0,
        double networkUsageMbps = 5.0,
        DateTime? timestamp = null)
    {
        return new PerformanceMetrics
        {
            ConnectionProfileId = connectionProfileId,
            Timestamp = timestamp ?? DateTime.UtcNow,
            LatencyMs = latencyMs,
            BandwidthMbps = bandwidthMbps,
            PacketLossPercent = packetLossPercent,
            FrameRate = frameRate,
            QualityScore = qualityScore,
            CpuUsagePercent = cpuUsagePercent,
            MemoryUsageMB = memoryUsageMB,
            NetworkUsageMbps = networkUsageMbps
        };
    }

    public static List<PerformanceMetrics> CreateTimeSeries(
        int connectionProfileId = 1,
        int count = 60,
        TimeSpan? interval = null)
    {
        var metrics = new List<PerformanceMetrics>();
        var intervalTime = interval ?? TimeSpan.FromSeconds(1);
        var startTime = DateTime.UtcNow.AddMinutes(-count);

        for (int i = 0; i < count; i++)
        {
            metrics.Add(Create(
                connectionProfileId: connectionProfileId,
                latencyMs: 50.0 + (i % 10) * 5, // Varying latency
                bandwidthMbps: 10.0 - (i % 5) * 0.5, // Varying bandwidth
                timestamp: startTime.Add(intervalTime * i)
            ));
        }

        return metrics;
    }
}
