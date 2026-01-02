using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Connection suggestion for quick connect functionality
/// </summary>
public class ConnectionSuggestion 
{
    public ConnectionProfile? Profile { get; set; }
    public string MatchedText { get; set; } = string.Empty;
    public SuggestionType Type { get; set; }
    public DateTime LastConnectedAt { get; set; }
    public int ConnectionCount { get; set; }
    public double RelevanceScore { get; set; }
}

/// <summary>
/// Types of connection suggestions
/// </summary>
public enum SuggestionType 
{
    RecentlyUsed,
    Favorite,
    FrequentlyUsed,
    NameMatch,
    HostMatch,
    TagMatch
}
