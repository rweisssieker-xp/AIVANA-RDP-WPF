using Aivana_RDP_WPF.Models.AI;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services.AI;

/// <summary>
/// Service interface for AI assistant functionality
/// </summary>
public interface IAIAssistantService 
{
    Task<List<AIConnectionSuggestion>> GetConnectionSuggestionsAsync(string? context = null);
    Task<AIConnectionSuggestion?> GetBestSuggestionAsync(string? context = null);
    Task TrainOnUserBehaviorAsync(ConnectionProfile profile, UserAction action);
    Task<List<UserBehaviorPattern>> AnalyzeUserPatternsAsync();
    Task<PredictiveMetrics> GetPredictiveMetricsAsync(string profileId);
    Task<List<PredictiveAlert>> GetPredictiveAlertsAsync();
    Task<bool> ShouldSuggestConnectionAsync(string profileId, DateTime time);
    Task<SmartRecommendation> GetWorkflowRecommendationAsync(string currentTask);
    event EventHandler<AIConnectionSuggestion>? SuggestionAvailable;
    event EventHandler<PredictiveAlert>? AlertGenerated;
    event EventHandler<UserBehaviorPattern>? PatternDetected;
}
