using Aivana_RDP_WPF.Models.AI;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;
using Aivana_RDP_WPF.Infrastructure.Protocols;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Aivana_RDP_WPF.Services.AI;

/// <summary>
/// Implementation of AI assistant service
/// </summary>
public class AIAssistantService : IAIAssistantService 
{
    private readonly ILogger<AIAssistantService> _logger;
    private readonly IConnectionProfileService _profileService;
    private readonly IMachineLearningService _mlService;
    private readonly IPerformanceMonitorService _performanceService;
    private readonly List<UserActivity> _userActivities = new();
    private readonly List<UserBehaviorPattern> _detectedPatterns = new();
    private readonly Dictionary<string, PredictiveMetrics> _predictions = new();

    public event EventHandler<AIConnectionSuggestion>? SuggestionAvailable;
    public event EventHandler<PredictiveAlert>? AlertGenerated;
    public event EventHandler<UserBehaviorPattern>? PatternDetected;

    public AIAssistantService(
        ILogger<AIAssistantService> logger,
        IConnectionProfileService profileService,
        IMachineLearningService mlService,
        IPerformanceMonitorService performanceService)
    {
        _logger = logger;
        _profileService = profileService;
        _mlService = mlService;
        _performanceService = performanceService;
        
        _ = Task.Run(InitializeAsync);
    }

    public async Task<List<AIConnectionSuggestion>> GetConnectionSuggestionsAsync(string? context = null)
    {
        try
        {
            var suggestions = new List<AIConnectionSuggestion>();
            var allProfiles = await _profileService.GetAllProfilesAsync();
            var currentTime = DateTime.UtcNow;
            
            foreach (var profile in allProfiles)
            {
                var suggestion = await GenerateSuggestionAsync(profile, currentTime, context);
                if (suggestion != null && suggestion.ConfidenceScore > 0.3)
                {
                    suggestions.Add(suggestion);
                }
            }

            suggestions = suggestions
                .OrderByDescending(s => s.Priority)
                .ThenByDescending(s => s.ConfidenceScore)
                .Take(5)
                .ToList();

            _logger.LogInformation("Generated {Count} AI connection suggestions", suggestions.Count);
            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating connection suggestions");
            return new List<AIConnectionSuggestion>();
        }
    }

    public async Task<AIConnectionSuggestion?> GetBestSuggestionAsync(string? context = null)
    {
        var suggestions = await GetConnectionSuggestionsAsync(context);
        return suggestions.FirstOrDefault();
    }

    public async Task TrainOnUserBehaviorAsync(ConnectionProfile profile, UserAction action)
    {
        try
        {
            var activity = new UserActivity
            {
                Action = action,
                Profile = profile,
                Timestamp = DateTime.UtcNow,
                Context = GetCurrentContext(),
                Success = true
            };

            _userActivities.Add(activity);

            if (_userActivities.Count > 1000)
            {
                _userActivities.RemoveAt(0);
            }

            var trainingData = new TrainingData
            {
                Profile = profile,
                Action = action,
                Features = ExtractFeatures(activity),
                Outcome = action == UserAction.Connect ? 1.0 : 0.0,
                ContextualData = activity.Context.Values.Select(v => v.ToString()).ToList()
            };

            await _mlService.TrainModelAsync(new List<TrainingData> { trainingData });

            await AnalyzePatternsAsync();

            _logger.LogDebug("Trained on user behavior: {Action} for profile {ProfileName}", 
                action, profile.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training on user behavior");
        }
    }

    public async Task<List<UserBehaviorPattern>> AnalyzeUserPatternsAsync()
    {
        try
        {
            var patterns = await _mlService.DetectPatternsAsync(_userActivities);
            
            _detectedPatterns.Clear();
            foreach (var pattern in patterns)
            {
                _detectedPatterns.Add(pattern);
                PatternDetected?.Invoke(this, pattern);
            }

            _logger.LogInformation("Detected {Count} user behavior patterns", patterns.Count);
            return patterns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing user patterns");
            return new List<UserBehaviorPattern>();
        }
    }

    public async Task<PredictiveMetrics> GetPredictiveMetricsAsync(string profileId)
    {
        try
        {
            if (_predictions.TryGetValue(profileId, out var cachedMetrics) && 
                DateTime.UtcNow - cachedMetrics.PredictionTime < TimeSpan.FromMinutes(5))
            {
                return cachedMetrics;
            }

            var profile = (await _profileService.GetAllProfilesAsync())
                .FirstOrDefault(p => p.Id.ToString() == profileId);
            
            if (profile == null)
            {
                throw new ArgumentException($"Profile {profileId} not found");
            }

            var metrics = new PredictiveMetrics
            {
                ProfileId = profileId,
                PredictionTime = DateTime.UtcNow,
                ExpectedLatency = await _mlService.PredictPerformanceAsync(profile),
                ExpectedBandwidth = 100.0,
                ReliabilityScore = await _mlService.PredictConnectionSuccessAsync(profile, DateTime.UtcNow),
                SecurityRisk = await _mlService.AssessSecurityRiskAsync(profile),
                PerformanceTrend = AnalyzePerformanceTrend(profileId),
                FeatureImportance = await _mlService.GetFeatureImportanceAsync(profileId)
            };

            var alerts = await GeneratePredictiveAlertsAsync(metrics);
            metrics.Alerts = alerts;

            _predictions[profileId] = metrics;

            foreach (var alert in alerts)
            {
                AlertGenerated?.Invoke(this, alert);
            }

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting predictive metrics for profile {ProfileId}", profileId);
            return new PredictiveMetrics { ProfileId = profileId };
        }
    }

    public async Task<List<PredictiveAlert>> GetPredictiveAlertsAsync()
    {
        var allAlerts = new List<PredictiveAlert>();
        
        foreach (var prediction in _predictions.Values)
        {
            allAlerts.AddRange(prediction.Alerts);
        }

        return await Task.FromResult(allAlerts
            .OrderBy(a => a.TimeToEvent)
            .ThenByDescending(a => a.Severity)
            .ToList());
    }

    public async Task<bool> ShouldSuggestConnectionAsync(string profileId, DateTime time)
    {
        try
        {
            var profile = (await _profileService.GetAllProfilesAsync())
                .FirstOrDefault(p => p.Id.ToString() == profileId);
            
            if (profile == null) return false;

            var patterns = _detectedPatterns.Where(p => 
                p.AssociatedProfiles.Contains(profileId) && p.IsActive).ToList();

            foreach (var pattern in patterns)
            {
                if (await MatchesPatternAsync(pattern, time))
                {
                    return true;
                }
            }

            var recentActivities = _userActivities
                .Where(a => a.Profile?.Id.ToString() == profileId)
                .ToList();

            if (recentActivities.Any())
            {
                var avgTimeOfDay = recentActivities
                    .GroupBy<UserActivity, TimeSpan>(a => a.TimeOfDay())
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key;

                if (avgTimeOfDay.HasValue && 
                    Math.Abs((time.TimeOfDay - avgTimeOfDay.Value).TotalMinutes) < 30)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if should suggest connection");
            return false;
        }
    }

    public async Task<SmartRecommendation> GetWorkflowRecommendationAsync(string currentTask)
    {
        try
        {
            var recommendation = new SmartRecommendation
            {
                Type = RecommendationType.WorkflowImprovement,
                Title = $"Optimize {currentTask}",
                Description = "Based on your usage patterns, here are suggestions to improve your workflow",
                Confidence = 0.8,
                GeneratedAt = DateTime.UtcNow,
                ValidUntil = TimeSpan.FromHours(24)
            };

            switch (currentTask.ToLowerInvariant())
            {
                case "development":
                    recommendation.RecommendedActions.AddRange(new[]
                    {
                        "Connect to development server first",
                        "Start IDE and terminal in parallel",
                        "Enable file synchronization"
                    });
                    break;
                case "testing":
                    recommendation.RecommendedActions.AddRange(new[]
                    {
                        "Use test environment profile",
                        "Enable performance monitoring",
                        "Set up automated test workflows"
                    });
                    break;
                case "maintenance":
                    recommendation.RecommendedActions.AddRange(new[]
                    {
                        "Connect with elevated privileges",
                        "Enable session recording",
                        "Prepare backup workflows"
                    });
                    break;
                default:
                    recommendation.RecommendedActions.Add("Consider creating a custom workflow for this task");
                    break;
            }

            return recommendation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow recommendation for task {Task}", currentTask);
            return new SmartRecommendation
            {
                Type = RecommendationType.WorkflowImprovement,
                Title = "General Optimization",
                Description = "Review your workflows for potential improvements",
                Confidence = 0.5
            };
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            await _mlService.InitializeModelAsync();
            _logger.LogInformation("AI Assistant service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing AI Assistant service");
        }
    }

    private async Task<AIConnectionSuggestion?> GenerateSuggestionAsync(ConnectionProfile profile, DateTime time, string? context)
    {
        try
        {
            var confidence = await _mlService.PredictConnectionSuccessAsync(profile, time);
            var performance = await _mlService.PredictPerformanceAsync(profile);
            var securityRisk = await _mlService.AssessSecurityRiskAsync(profile);

            if (confidence < 0.3) return null;

            var suggestion = new AIConnectionSuggestion
            {
                Profile = profile,
                ConfidenceScore = confidence,
                Reason = DetermineReason(profile, time, confidence),
                Explanation = GenerateExplanation(profile, confidence, performance, securityRisk),
                Priority = DeterminePriority(confidence, securityRisk),
                IsActionable = confidence > 0.5 && securityRisk != SecurityRiskLevel.Critical,
                ContextualFactors = GetContextualFactors(profile, time, context)
            };

            return suggestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating suggestion for profile {ProfileName}", profile.Name);
            return null;
        }
    }

    private SuggestionReason DetermineReason(ConnectionProfile profile, DateTime time, double confidence)
    {
        if (confidence > 0.8) return SuggestionReason.PerformanceOptimal;
        
        var recentUsage = _userActivities
            .Where(a => a.Profile?.Id == profile.Id && 
                       a.Timestamp > DateTime.UtcNow.AddHours(-24))
            .Count();

        if (recentUsage > 5) return SuggestionReason.FrequentUsage;
        
        var sameTimeLastWeek = _userActivities
            .Where(a => a.Profile?.Id == profile.Id &&
                       Math.Abs((a.Timestamp - time).TotalDays) < 7.5 &&
                       Math.Abs((a.Timestamp - time).TotalDays) > 6.5)
            .Any();

        if (sameTimeLastWeek) return SuggestionReason.TimeBasedPattern;

        return SuggestionReason.UserPreference;
    }

    private string GenerateExplanation(ConnectionProfile profile, double confidence, double performance, SecurityRiskLevel securityRisk)
    {
        var explanations = new List<string>();

        if (confidence > 0.8)
        {
            explanations.Add($"High success rate ({confidence:P1}) based on historical data");
        }

        if (performance > 80)
        {
            explanations.Add($"Expected excellent performance ({performance:F1}% quality)");
        }

        if (securityRisk == SecurityRiskLevel.Low)
        {
            explanations.Add("Low security risk detected");
        }

        var recentUsage = _userActivities
            .Where(a => a.Profile?.Id == profile.Id && 
                       a.Timestamp > DateTime.UtcNow.AddHours(-24))
            .Count();

        if (recentUsage > 3)
        {
            explanations.Add($"Used {recentUsage} times in the last 24 hours");
        }

        return string.Join("; ", explanations);
    }

    private SuggestionPriority DeterminePriority(double confidence, SecurityRiskLevel securityRisk)
    {
        if (securityRisk == SecurityRiskLevel.Critical) return SuggestionPriority.Low;
        if (confidence > 0.9) return SuggestionPriority.High;
        if (confidence > 0.7) return SuggestionPriority.Medium;
        return SuggestionPriority.Low;
    }

    private List<string> GetContextualFactors(ConnectionProfile profile, DateTime time, string? context)
    {
        var factors = new List<string>();

        if (!string.IsNullOrEmpty(context))
        {
            factors.Add($"Context: {context}");
        }

        factors.Add($"Time: {time:HH:mm}");
        factors.Add($"Day: {time:dddd}");
        factors.Add($"Protocol: {profile.ProtocolType}");

        var recentFailures = _userActivities
            .Where(a => a.Profile?.Id == profile.Id && 
                       a.Action == UserAction.Error &&
                       a.Timestamp > DateTime.UtcNow.AddHours(-24))
            .Count();

        if (recentFailures > 0)
        {
            factors.Add($"Recent failures: {recentFailures}");
        }

        return factors;
    }

    private Dictionary<string, object> ExtractFeatures(UserActivity activity)
    {
        return new Dictionary<string, object>
        {
            ["HourOfDay"] = activity.Timestamp.Hour,
            ["DayOfWeek"] = (int)activity.Timestamp.DayOfWeek,
            ["ActionType"] = activity.Action.ToString(),
            ["ProfileProtocol"] = activity.Profile?.ProtocolType.ToString() ?? string.Empty,
            ["Duration"] = activity.Duration,
            ["Success"] = activity.Success
        };
    }

    private async Task AnalyzePatternsAsync()
    {
        try
        {
            var patterns = await _mlService.DetectPatternsAsync(_userActivities);
            
            foreach (var pattern in patterns)
            {
                if (!_detectedPatterns.Any(p => p.Id == pattern.Id))
                {
                    _detectedPatterns.Add(pattern);
                    PatternDetected?.Invoke(this, pattern);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing patterns");
        }
    }

    private PerformanceTrend AnalyzePerformanceTrend(string profileId)
    {
        return PerformanceTrend.Stable;
    }

    private async Task<List<PredictiveAlert>> GeneratePredictiveAlertsAsync(PredictiveMetrics metrics)
    {
        var alerts = new List<PredictiveAlert>();

        if (metrics.ExpectedLatency > 200)
        {
            alerts.Add(new PredictiveAlert
            {
                Type = AlertType.PerformanceDegradation,
                Message = $"High latency expected: {metrics.ExpectedLatency:F1}ms",
                Severity = AlertSeverity.Warning,
                TimeToEvent = TimeSpan.FromMinutes(30),
                Confidence = 0.7
            });
        }

        if (metrics.ReliabilityScore < 0.7)
        {
            alerts.Add(new PredictiveAlert
            {
                Type = AlertType.ConnectionFailure,
                Message = $"Low reliability score: {metrics.ReliabilityScore:P1}",
                Severity = AlertSeverity.Warning,
                TimeToEvent = TimeSpan.FromHours(1),
                Confidence = 0.8
            });
        }

        if (metrics.SecurityRisk >= SecurityRiskLevel.High)
        {
            alerts.Add(new PredictiveAlert
            {
                Type = AlertType.SecurityIssue,
                Message = $"Security risk detected: {metrics.SecurityRisk}",
                Severity = AlertSeverity.Error,
                TimeToEvent = TimeSpan.Zero,
                Confidence = 0.9
            });
        }

        return await Task.FromResult(alerts);
    }

    private Dictionary<string, object> GetCurrentContext()
    {
        return new Dictionary<string, object>
        {
            ["TimeOfDay"] = DateTime.UtcNow.Hour,
            ["DayOfWeek"] = (int)DateTime.UtcNow.DayOfWeek,
            ["MachineName"] = Environment.MachineName,
            ["UserName"] = Environment.UserName
        };
    }

    private async Task<bool> MatchesPatternAsync(UserBehaviorPattern pattern, DateTime time)
    {
        return await Task.FromResult(false);
    }
}

public static class UserActivityExtensions
{
    public static TimeSpan TimeOfDay(this UserActivity activity) => 
        activity.Timestamp.TimeOfDay;
}
