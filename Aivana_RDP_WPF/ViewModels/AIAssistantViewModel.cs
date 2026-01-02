using Aivana_RDP_WPF.Models.AI;
using Aivana_RDP_WPF.Services.AI;
using Aivana_RDP_WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Aivana_RDP_WPF.ViewModels;

/// <summary>
/// ViewModel for AI assistant functionality
/// </summary>
public partial class AIAssistantViewModel : ObservableObject
{
    private readonly IAIAssistantService _aiAssistant;
    private readonly IConnectionProfileService _profileService;

    [ObservableProperty]
    private ObservableCollection<AIConnectionSuggestion> _suggestions = new();

    [ObservableProperty]
    private AIConnectionSuggestion? _selectedSuggestion;

    [ObservableProperty]
    private ObservableCollection<PredictiveAlert> _alerts = new();

    [ObservableProperty]
    private ObservableCollection<UserBehaviorPattern> _patterns = new();

    [ObservableProperty]
    private ObservableCollection<SmartRecommendation> _recommendations = new();

    [ObservableProperty]
    private bool _isAnalyzing;

    [ObservableProperty]
    private string _currentContext = string.Empty;

    [ObservableProperty]
    private PredictiveMetrics? _selectedProfileMetrics;

    public AIAssistantViewModel(
        IAIAssistantService aiAssistant,
        IConnectionProfileService profileService)
    {
        _aiAssistant = aiAssistant;
        _profileService = profileService;

        _aiAssistant.SuggestionAvailable += OnSuggestionAvailable;
        _aiAssistant.AlertGenerated += OnAlertGenerated;
        _aiAssistant.PatternDetected += OnPatternDetected;

        _ = Task.Run(LoadAIDataAsync);
    }

    [RelayCommand]
    private async Task RefreshSuggestionsAsync()
    {
        try
        {
            IsAnalyzing = true;
            var suggestions = await _aiAssistant.GetConnectionSuggestionsAsync(CurrentContext);
            
            App.Current.Dispatcher.Invoke(() =>
            {
                Suggestions.Clear();
                foreach (var suggestion in suggestions)
                {
                    Suggestions.Add(suggestion);
                }
            });
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAIDataAsync()
    {
        try
        {
            IsAnalyzing = true;
            await LoadAIDataAsync();
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task ConnectToSuggestionAsync(AIConnectionSuggestion suggestion)
    {
        if (suggestion?.Profile == null) return;

        try
        {
            // Train AI on this action
            await _aiAssistant.TrainOnUserBehaviorAsync(suggestion.Profile, UserAction.Connect);
            
            // TODO: Actually connect to the profile
            // This would integrate with the existing connection service
            
            SelectedSuggestion = suggestion;
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
    }

    [RelayCommand]
    private async Task IgnoreSuggestionAsync(AIConnectionSuggestion suggestion)
    {
        if (suggestion?.Profile == null) return;

        try
        {
            // Train AI that this suggestion was ignored
            await _aiAssistant.TrainOnUserBehaviorAsync(suggestion.Profile, UserAction.Error);
            
            // Remove from suggestions
            Suggestions.Remove(suggestion);
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
    }

    [RelayCommand]
    private async Task AnalyzePatternsAsync()
    {
        try
        {
            IsAnalyzing = true;
            var patterns = await _aiAssistant.AnalyzeUserPatternsAsync();
            
            App.Current.Dispatcher.Invoke(() =>
            {
                Patterns.Clear();
                foreach (var pattern in patterns)
                {
                    Patterns.Add(pattern);
                }
            });
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task GetWorkflowRecommendationAsync(string task)
    {
        try
        {
            var recommendation = await _aiAssistant.GetWorkflowRecommendationAsync(task);
            
            App.Current.Dispatcher.Invoke(() =>
            {
                Recommendations.Clear();
                Recommendations.Add(recommendation);
            });
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
    }

    [RelayCommand]
    private async Task GetPredictiveMetricsAsync(string profileId)
    {
        try
        {
            IsAnalyzing = true;
            var metrics = await _aiAssistant.GetPredictiveMetricsAsync(profileId);
            
            SelectedProfileMetrics = metrics;
            
            // Update alerts
            var alerts = await _aiAssistant.GetPredictiveAlertsAsync();
            App.Current.Dispatcher.Invoke(() =>
            {
                Alerts.Clear();
                foreach (var alert in alerts)
                {
                    Alerts.Add(alert);
                }
            });
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    private async Task LoadAIDataAsync()
    {
        try
        {
            // Load initial suggestions
            await RefreshSuggestionsAsync();
            
            // Load patterns
            await AnalyzePatternsAsync();
            
            // Load alerts
            var alerts = await _aiAssistant.GetPredictiveAlertsAsync();
            App.Current.Dispatcher.Invoke(() =>
            {
                Alerts.Clear();
                foreach (var alert in alerts)
                {
                    Alerts.Add(alert);
                }
            });
        }
        catch (Exception ex)
        {
            // TODO: Handle initialization error
        }
    }

    private void OnSuggestionAvailable(object? sender, AIConnectionSuggestion suggestion)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            if (!Suggestions.Any(s => s.Id == suggestion.Id))
            {
                Suggestions.Insert(0, suggestion);
                
                // Keep only top 10 suggestions
                while (Suggestions.Count > 10)
                {
                    Suggestions.RemoveAt(Suggestions.Count - 1);
                }
            }
        });
    }

    private void OnAlertGenerated(object? sender, PredictiveAlert alert)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            if (!Alerts.Any(a => a.Id == alert.Id))
            {
                Alerts.Insert(0, alert);
                
                // Keep only top 20 alerts
                while (Alerts.Count > 20)
                {
                    Alerts.RemoveAt(Alerts.Count - 1);
                }
            }
        });
    }

    private void OnPatternDetected(object? sender, UserBehaviorPattern pattern)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            if (!Patterns.Any(p => p.Id == pattern.Id))
            {
                Patterns.Insert(0, pattern);
                
                // Keep only top 15 patterns
                while (Patterns.Count > 15)
                {
                    Patterns.RemoveAt(Patterns.Count - 1);
                }
            }
        });
    }

    partial void OnCurrentContextChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            _ = Task.Run(RefreshSuggestionsAsync);
        }
    }
}
