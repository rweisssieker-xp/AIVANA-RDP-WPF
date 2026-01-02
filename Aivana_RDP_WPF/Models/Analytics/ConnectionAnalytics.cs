namespace Aivana_RDP_WPF.Models.Analytics;

/// <summary>
/// Comprehensive connection analytics data
/// </summary>
public class ConnectionAnalytics 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public AnalyticsPeriod Period { get; set; }
    public ConnectionStatistics Statistics { get; set; } = new();
    public PerformanceAnalytics Performance { get; set; } = new();
    public UsageAnalytics Usage { get; set; } = new();
    public SecurityAnalytics Security { get; set; } = new();
    public List<ConnectionTrend> Trends { get; set; } = new();
    public Dictionary<string, object> Insights { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Analytics period types
/// </summary>
public enum AnalyticsPeriod 
{
    Hourly,
    Daily,
    Weekly,
    Monthly,
    Yearly
}

/// <summary>
/// Severity levels for issues and incidents
/// </summary>
public enum Severity 
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Connection statistics and metrics
/// </summary>
public class ConnectionStatistics 
{
    public int TotalConnections { get; set; }
    public int SuccessfulConnections { get; set; }
    public int FailedConnections { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan AverageConnectionDuration { get; set; }
    public TimeSpan TotalConnectionTime { get; set; }
    public int UniqueProfiles { get; set; }
    public Dictionary<string, int> ProtocolUsage { get; set; } = new();
    public Dictionary<string, int> HostUsage { get; set; } = new();
}

/// <summary>
/// Performance analytics data
/// </summary>
public class PerformanceAnalytics 
{
    public double AverageLatency { get; set; }
    public double MinLatency { get; set; }
    public double MaxLatency { get; set; }
    public double AverageBandwidth { get; set; }
    public double AverageQualityScore { get; set; }
    public List<PerformanceIssue> Issues { get; set; } = new();
    public Models.AI.PerformanceTrend Trend { get; set; }
    public Dictionary<string, double> MetricsByProtocol { get; set; } = new();
}

/// <summary>
/// Usage analytics and patterns
/// </summary>
public class UsageAnalytics 
{
    public Dictionary<string, TimeSpan> UsageByProfile { get; set; } = new();
    public Dictionary<string, int> ConnectionsByHour { get; set; } = new();
    public Dictionary<string, int> ConnectionsByDay { get; set; } = new();
    public List<UsagePattern> Patterns { get; set; } = new();
    public TimeSpan PeakUsageTime { get; set; }
    public int PeakConnectionsCount { get; set; }
    public double AverageSessionsPerDay { get; set; }
}

/// <summary>
/// Security analytics data
/// </summary>
public class SecurityAnalytics 
{
    public int SecurityEvents { get; set; }
    public List<SecurityIncident> Incidents { get; set; } = new();
    public Dictionary<string, int> EventTypes { get; set; } = new();
    public Models.AI.SecurityRiskLevel OverallRisk { get; set; }
    public List<SecurityRecommendation> Recommendations { get; set; } = new();
}

/// <summary>
/// Connection trend data
/// </summary>
public class ConnectionTrend 
{
    public DateTime Timestamp { get; set; }
    public int ConnectionCount { get; set; }
    public double SuccessRate { get; set; }
    public double AverageLatency { get; set; }
    public TrendDirection Direction { get; set; }
    public double ChangePercentage { get; set; }
}

/// <summary>
/// Trend direction indicators
/// </summary>
public enum TrendDirection 
{
    Up,
    Down,
    Stable
}

/// <summary>
/// Performance issue details
/// </summary>
public class PerformanceIssue 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public IssueType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public Severity Severity { get; set; }
    public DateTime FirstDetected { get; set; } = DateTime.UtcNow;
    public int OccurrenceCount { get; set; }
    public double ImpactScore { get; set; }
    public List<string> AffectedProfiles { get; set; } = new();
}

/// <summary>
/// Performance issue types
/// </summary>
public enum IssueType 
{
    HighLatency,
    LowBandwidth,
    ConnectionDrops,
    QualityDegradation,
    SecurityBreach
}

/// <summary>
/// Usage pattern information
/// </summary>
public class UsagePattern 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public PatternType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime FirstObserved { get; set; } = DateTime.UtcNow;
    public int OccurrenceCount { get; set; }
    public List<string> AssociatedProfiles { get; set; } = new();
}

/// <summary>
/// Usage pattern types
/// </summary>
public enum PatternType 
{
    RecurrentConnection,
    PeakUsage,
    ProtocolPreference,
    HostPreference,
    TimeBasedPattern
}

/// <summary>
/// Security incident details
/// </summary>
public class SecurityIncident 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public SecurityEventType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public Severity Severity { get; set; }
    public string ProfileId { get; set; } = string.Empty;
    public string HostAddress { get; set; } = string.Empty;
    public bool Resolved { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Security event types
/// </summary>
public enum SecurityEventType 
{
    UnauthorizedAccess,
    BruteForceAttempt,
    SuspiciousActivity,
    ConfigurationChange,
    DataTransferAnomaly
}

/// <summary>
/// Security recommendation details
/// </summary>
public class SecurityRecommendation 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationPriority Priority { get; set; }
    public List<string> Actions { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Recommendation priority levels
/// </summary>
public enum RecommendationPriority 
{
    Low,
    Medium,
    High,
    Critical
}
