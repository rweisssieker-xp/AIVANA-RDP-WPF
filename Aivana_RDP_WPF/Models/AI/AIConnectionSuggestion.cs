namespace Aivana_RDP_WPF.Models.AI;

/// <summary>
/// AI-powered connection suggestion with confidence scoring
/// </summary>
public class AIConnectionSuggestion 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ConnectionProfile Profile { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public SuggestionReason Reason { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public DateTime SuggestedAt { get; set; } = DateTime.UtcNow;
    public List<string> ContextualFactors { get; set; } = new();
    public SuggestionPriority Priority { get; set; }
    public bool IsActionable { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Reason for AI suggestion
/// </summary>
public enum SuggestionReason 
{
    FrequentUsage,
    TimeBasedPattern,
    LocationBased,
    RecentFailure,
    PerformanceOptimal,
    SecurityRecommendation,
    WorkflowOptimization,
    UserPreference
}

/// <summary>
/// Priority level of suggestions
/// </summary>
public enum SuggestionPriority 
{
    Low,
    Medium,
    High,
    Critical
}
