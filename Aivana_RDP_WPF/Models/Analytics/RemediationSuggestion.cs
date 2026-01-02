using System;

namespace Aivana_RDP_WPF.Models.Analytics;

public class RemediationSuggestion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationPriority Priority { get; set; } = RecommendationPriority.Medium;
    public string[] Actions { get; set; } = Array.Empty<string>();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? RelatedIssueId { get; set; }
}
