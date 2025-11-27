namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Represents performance metrics for an RDP connection.
/// </summary>
public class PerformanceMetrics
{
    public int ConnectionProfileId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Network metrics
    public double LatencyMs { get; set; }
    public double BandwidthMbps { get; set; }
    public int PacketLossPercent { get; set; }
    
    // Quality metrics
    public double FrameRate { get; set; }
    public int QualityScore { get; set; } // 0-100
    
    // Resource usage
    public double CpuUsagePercent { get; set; }
    public double MemoryUsageMB { get; set; }
    public double NetworkUsageMbps { get; set; }
}

