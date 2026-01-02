namespace Aivana_RDP_WPF.Models.AI;

/// <summary>
/// Detected user behavior pattern
/// </summary>
public class UserBehaviorPattern 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PatternType { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> PatternData { get; set; } = new();
    public DateTime FirstObserved { get; set; } = DateTime.UtcNow;
    public DateTime LastObserved { get; set; } = DateTime.UtcNow;
    public int OccurrenceCount { get; set; }
    public List<string> AssociatedProfiles { get; set; } = new();
    public PatternFrequency Frequency { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Frequency of pattern occurrence
/// </summary>
public enum PatternFrequency 
{
    Hourly,
    Daily,
    Weekly,
    Monthly,
    Sporadic
}
