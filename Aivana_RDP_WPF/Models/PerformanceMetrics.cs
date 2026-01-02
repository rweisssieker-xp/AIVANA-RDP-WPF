using System.ComponentModel.DataAnnotations;

namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Performance metrics for remote connections
/// </summary>
public class PerformanceMetrics
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ConnectionProfileId { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Network metrics
    public double LatencyMs { get; set; }
    public double BandwidthMbps { get; set; }
    public double PacketLossPercent { get; set; }
    
    // Quality metrics
    public double FrameRate { get; set; }
    public int QualityScore { get; set; } // 0-100
    
    // Resource usage
    public double CpuUsagePercent { get; set; }
    public double MemoryUsageMB { get; set; }
    public double NetworkUsageMbps { get; set; }

    public ConnectionQuality Quality => QualityScore switch
    {
        >= 90 => ConnectionQuality.Excellent,
        >= 75 => ConnectionQuality.Good,
        >= 60 => ConnectionQuality.Fair,
        >= 40 => ConnectionQuality.Poor,
        _ => ConnectionQuality.VeryPoor
    };

    // Navigation property
    public virtual ConnectionProfile? ConnectionProfile { get; set; }
}

/// <summary>
/// Connection quality levels
/// </summary>
public enum ConnectionQuality
{
    VeryPoor,
    Poor,
    Fair,
    Good,
    Excellent
}

