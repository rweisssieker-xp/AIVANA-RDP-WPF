namespace Aivana_RDP_WPF.Models.AI;

/// <summary>
/// Predictive metrics for connection performance
/// </summary>
public class PredictiveMetrics 
{
    public string ProfileId { get; set; } = string.Empty;
    public DateTime PredictionTime { get; set; } = DateTime.UtcNow;
    public double ExpectedLatency { get; set; }
    public double ExpectedBandwidth { get; set; }
    public double ReliabilityScore { get; set; }
    public SecurityRiskLevel SecurityRisk { get; set; }
    public PerformanceTrend PerformanceTrend { get; set; }
    public List<PredictiveAlert> Alerts { get; set; } = new();
    public Dictionary<string, double> FeatureImportance { get; set; } = new();
    public TimeSpan PredictionHorizon { get; set; } = TimeSpan.FromHours(1);
}

/// <summary>
/// Security risk assessment level
/// </summary>
public enum SecurityRiskLevel 
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Performance trend prediction
/// </summary>
public enum PerformanceTrend 
{
    Improving,
    Stable,
    Degrading,
    Unknown
}

/// <summary>
/// Predictive alert for potential issues
/// </summary>
public class PredictiveAlert 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public AlertType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan TimeToEvent { get; set; }
    public double Confidence { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Types of predictive alerts
/// </summary>
public enum AlertType 
{
    PerformanceDegradation,
    ConnectionFailure,
    SecurityIssue,
    ResourceExhaustion,
    MaintenanceRequired
}

/// <summary>
/// Alert severity levels
/// </summary>
public enum AlertSeverity 
{
    Info,
    Warning,
    Error,
    Critical
}
