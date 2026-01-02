namespace Aivana_RDP_WPF.Models.AI;

/// <summary>
/// Training data for machine learning models
/// </summary>
public class TrainingData 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public ConnectionProfile Profile { get; set; } = new();
    public UserAction Action { get; set; }
    public Dictionary<string, object> Features { get; set; } = new();
    public double Outcome { get; set; }
    public List<string> ContextualData { get; set; } = new();
}

/// <summary>
/// User activity for pattern analysis
/// </summary>
public class UserActivity 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public UserAction Action { get; set; }
    public ConnectionProfile? Profile { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    public double Duration { get; set; }
    public bool Success { get; set; }
}

/// <summary>
/// Types of user actions
/// </summary>
public enum UserAction 
{
    Connect,
    Disconnect,
    TransferFile,
    ExecuteCommand,
    StartApplication,
    StopApplication,
    Screenshot,
    Error
}

/// <summary>
/// Smart recommendation from AI
/// </summary>
public class SmartRecommendation 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public RecommendationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> RecommendedActions { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ValidUntil { get; set; }
}

/// <summary>
/// Types of smart recommendations
/// </summary>
public enum RecommendationType 
{
    ConnectionOptimization,
    WorkflowImprovement,
    SecurityEnhancement,
    PerformanceTuning,
    AutomationSuggestion
}
