using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Tests.TestHelpers.Factories;

/// <summary>
/// Factory for creating test PerformanceMetrics instances
/// </summary>
public static class PerformanceMetricsFactory
{
    public static PerformanceMetrics Create(
        Guid sessionId,
        double latency = 50.0,
        double bandwidth = 10.0,
        double packetLoss = 0.0,
        double frameRate = 30.0,
        DateTime? timestamp = null)
    {
        return new PerformanceMetrics
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Timestamp = timestamp ?? DateTime.UtcNow,
            Latency = latency,
            Bandwidth = bandwidth,
            PacketLoss = packetLoss,
            FrameRate = frameRate
        };
    }

    public static List<PerformanceMetrics> CreateTimeSeries(
        Guid sessionId,
        int count = 60,
        TimeSpan? interval = null)
    {
        var metrics = new List<PerformanceMetrics>();
        var intervalTime = interval ?? TimeSpan.FromSeconds(1);
        var startTime = DateTime.UtcNow.AddMinutes(-count);

        for (int i = 0; i < count; i++)
        {
            metrics.Add(Create(
                sessionId,
                latency: 50.0 + (i % 10) * 5, // Varying latency
                bandwidth: 10.0 - (i % 5) * 0.5, // Varying bandwidth
                timestamp: startTime.Add(intervalTime * i)
            ));
        }

        return metrics;
    }
}

