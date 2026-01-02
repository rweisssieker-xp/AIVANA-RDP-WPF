using Aivana_RDP_WPF.Models.Analytics;
using Aivana_RDP_WPF.Services.Analytics;
using Aivana_RDP_WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Aivana_RDP_WPF.ViewModels;

/// <summary>
/// ViewModel for analytics dashboard
/// </summary>
public partial class AnalyticsViewModel : ObservableObject
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IReportingService _reportingService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private ConnectionAnalytics? _currentAnalytics;

    [ObservableProperty]
    private ObservableCollection<ConnectionTrend> _trends = new();

    [ObservableProperty]
    private ObservableCollection<PerformanceIssue> _performanceIssues = new();

    [ObservableProperty]
    private ObservableCollection<UsagePattern> _usagePatterns = new();

    [ObservableProperty]
    private ObservableCollection<SecurityIncident> _securityIncidents = new();

    [ObservableProperty]
    private ObservableCollection<RemediationSuggestion> _remediationSuggestions = new();

    [ObservableProperty]
    private AnalyticsPeriod _selectedPeriod = AnalyticsPeriod.Daily;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private Dictionary<string, object> _insights = new();

    [ObservableProperty]
    private DateTime _startDate = DateTime.UtcNow.AddDays(-7);

    [ObservableProperty]
    private DateTime _endDate = DateTime.UtcNow;

    // Additional properties for UI binding
    [ObservableProperty]
    private ObservableCollection<string> _analyticsPeriods = new()
    {
        "Hourly", "Daily", "Weekly", "Monthly", "Yearly"
    };

    [ObservableProperty]
    private ObservableCollection<object> _keyMetrics = new();

    public AnalyticsViewModel(
        IAnalyticsService analyticsService,
        IReportingService reportingService,
        INotificationService notificationService)
    {
        _analyticsService = analyticsService;
        _reportingService = reportingService;
        _notificationService = notificationService;

        _analyticsService.AnalyticsUpdated += OnAnalyticsUpdated;
        _analyticsService.PerformanceIssueDetected += OnPerformanceIssueDetected;
        _analyticsService.SecurityIncidentDetected += OnSecurityIncidentDetected;

        _ = Task.Run(LoadAnalyticsAsync);
    }

    [RelayCommand]
    private async Task RefreshAnalyticsAsync()
    {
        try
        {
            IsLoading = true;
            var analytics = await _analyticsService.GetAnalyticsAsync(SelectedPeriod, StartDate, EndDate);
            
            CurrentAnalytics = analytics;
            Insights = analytics.Insights;
            
            await LoadTrendsAsync();
            await LoadPerformanceIssuesAsync();
            await LoadUsagePatternsAsync();
            await LoadSecurityIncidentsAsync();

            UpdateRemediationSuggestions();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError(ex.Message, "Analytics Refresh Failed");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportAnalyticsAsync(string format)
    {
        try
        {
            await _analyticsService.ExportAnalyticsAsync(SelectedPeriod, format);
            _notificationService.ShowSuccess($"Exported analytics as {format.ToUpperInvariant()}.", "Analytics Export");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError(ex.Message, "Analytics Export Failed");
        }
    }

    [RelayCommand]
    private async Task GenerateReportAsync(ReportType reportType)
    {
        try
        {
            var reportData = await _reportingService.GenerateReportAsync(
                reportType, SelectedPeriod, StartDate, EndDate);
            
            // TODO: Save or display report
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
    }

    [RelayCommand]
    private async Task LoadPerformanceDetailsAsync(string? profileId = null)
    {
        try
        {
            IsLoading = true;
            var performanceAnalytics = await _analyticsService.GetPerformanceAnalyticsAsync(profileId);
            
            // TODO: Update UI with performance details
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadUsageDetailsAsync()
    {
        try
        {
            IsLoading = true;
            var usageAnalytics = await _analyticsService.GetUsageAnalyticsAsync(StartDate, EndDate);
            
            // TODO: Update UI with usage details
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadSecurityDetailsAsync()
    {
        try
        {
            IsLoading = true;
            var securityAnalytics = await _analyticsService.GetSecurityAnalyticsAsync(StartDate, EndDate);
            
            // TODO: Update UI with security details
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadAnalyticsAsync()
    {
        try
        {
            await RefreshAnalyticsAsync();
            UpdateKeyMetrics();
        }
        catch (Exception ex)
        {
            // TODO: Handle initialization error
        }
    }

    private void UpdateKeyMetrics()
    {
        if (CurrentAnalytics == null) return;

        KeyMetrics.Clear();
        KeyMetrics.Add(new { Title = "Total Connections", Value = CurrentAnalytics.Statistics.TotalConnections, Subtitle = "Last 24 hours" });
        KeyMetrics.Add(new { Title = "Success Rate", Value = $"{CurrentAnalytics.Statistics.SuccessRate:P1}", Subtitle = "Performance" });
        KeyMetrics.Add(new { Title = "Avg Latency", Value = $"{CurrentAnalytics.Performance.AverageLatency:F1}ms", Subtitle = "Network" });
        KeyMetrics.Add(new { Title = "Security Risk", Value = CurrentAnalytics.Security.OverallRisk.ToString(), Subtitle = "Assessment" });
    }

    private async Task LoadTrendsAsync()
    {
        try
        {
            var trends = await _analyticsService.GetTrendsAsync(SelectedPeriod, 30);
            
            App.Current.Dispatcher.Invoke(() =>
            {
                Trends.Clear();
                foreach (var trend in trends)
                {
                    Trends.Add(trend);
                }
            });
        }
        catch (Exception ex)
        {
            // TODO: Handle trends loading error
        }
    }

    private async Task LoadPerformanceIssuesAsync()
    {
        try
        {
            var issues = await _analyticsService.GetPerformanceIssuesAsync();
            
            App.Current.Dispatcher.Invoke(() =>
            {
                PerformanceIssues.Clear();
                foreach (var issue in issues)
                {
                    PerformanceIssues.Add(issue);
                }
            });

            UpdateRemediationSuggestions();
        }
        catch (Exception ex)
        {
            // TODO: Handle performance issues loading error
        }
    }

    private async Task LoadUsagePatternsAsync()
    {
        try
        {
            var patterns = await _analyticsService.GetUsagePatternsAsync();
            
            App.Current.Dispatcher.Invoke(() =>
            {
                UsagePatterns.Clear();
                foreach (var pattern in patterns)
                {
                    UsagePatterns.Add(pattern);
                }
            });
        }
        catch (Exception ex)
        {
            // TODO: Handle usage patterns loading error
        }
    }

    private async Task LoadSecurityIncidentsAsync()
    {
        try
        {
            var incidents = await _analyticsService.GetSecurityIncidentsAsync();
            
            App.Current.Dispatcher.Invoke(() =>
            {
                SecurityIncidents.Clear();
                foreach (var incident in incidents)
                {
                    SecurityIncidents.Add(incident);
                }
            });
        }
        catch (Exception ex)
        {
            // TODO: Handle security incidents loading error
        }
    }

    private void OnAnalyticsUpdated(object? sender, ConnectionAnalytics analytics)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            CurrentAnalytics = analytics;
            Insights = analytics.Insights;
            UpdateKeyMetrics();
            UpdateRemediationSuggestions();
        });
    }

    private void OnPerformanceIssueDetected(object? sender, PerformanceIssue issue)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            if (!PerformanceIssues.Any(i => i.Id == issue.Id))
            {
                PerformanceIssues.Insert(0, issue);
                
                while (PerformanceIssues.Count > 20)
                {
                    PerformanceIssues.RemoveAt(PerformanceIssues.Count - 1);
                }
            }

            UpdateRemediationSuggestions();
        });
    }

    private void UpdateRemediationSuggestions()
    {
        try
        {
            var suggestions = new List<RemediationSuggestion>();

            if (CurrentAnalytics != null)
            {
                if (CurrentAnalytics.Performance.AverageLatency >= 150)
                {
                    suggestions.Add(new RemediationSuggestion
                    {
                        Title = "High latency detected",
                        Description = $"Average latency is {CurrentAnalytics.Performance.AverageLatency:F0}ms. Consider reducing visual quality or switching protocol.",
                        Priority = RecommendationPriority.High,
                        Actions = new[]
                        {
                            "Reduce RDP visual experience / disable animations",
                            "Check VPN / Wi-Fi signal and packet loss",
                            "Try SSH tunnel or alternative protocol (SSH/VNC)"
                        }
                    });
                }

                if (CurrentAnalytics.Performance.AverageQualityScore <= 60)
                {
                    suggestions.Add(new RemediationSuggestion
                    {
                        Title = "Connection quality is degrading",
                        Description = $"Average quality score is {CurrentAnalytics.Performance.AverageQualityScore:F0}/100.",
                        Priority = RecommendationPriority.Medium,
                        Actions = new[]
                        {
                            "Lower resolution / color depth",
                            "Disable background image and font smoothing",
                            "Reconnect during off-peak hours"
                        }
                    });
                }
            }

            foreach (var issue in PerformanceIssues.Take(10))
            {
                switch (issue.Type)
                {
                    case IssueType.HighLatency:
                        suggestions.Add(new RemediationSuggestion
                        {
                            RelatedIssueId = issue.Id,
                            Title = "Remediate: High latency",
                            Description = issue.Description,
                            Priority = issue.Severity >= Severity.High ? RecommendationPriority.High : RecommendationPriority.Medium,
                            Actions = new[]
                            {
                                "Run Health Check (Ping + Port)",
                                "Check network path (VPN, routing)",
                                "Reduce bandwidth-heavy features (audio, clipboard, printing)"
                            }
                        });
                        break;
                    case IssueType.LowBandwidth:
                        suggestions.Add(new RemediationSuggestion
                        {
                            RelatedIssueId = issue.Id,
                            Title = "Remediate: Low bandwidth",
                            Description = issue.Description,
                            Priority = RecommendationPriority.Medium,
                            Actions = new[]
                            {
                                "Disable bitmap caching / reduce visual quality",
                                "Prefer wired connection",
                                "Verify QoS / throttling on the network"
                            }
                        });
                        break;
                    case IssueType.ConnectionDrops:
                        suggestions.Add(new RemediationSuggestion
                        {
                            RelatedIssueId = issue.Id,
                            Title = "Remediate: Connection drops",
                            Description = issue.Description,
                            Priority = RecommendationPriority.High,
                            Actions = new[]
                            {
                                "Check session timeouts and keep-alives",
                                "Verify firewall/NAT stability",
                                "Retry with different protocol or tunnel"
                            }
                        });
                        break;
                    case IssueType.QualityDegradation:
                        suggestions.Add(new RemediationSuggestion
                        {
                            RelatedIssueId = issue.Id,
                            Title = "Remediate: Quality degradation",
                            Description = issue.Description,
                            Priority = RecommendationPriority.Medium,
                            Actions = new[]
                            {
                                "Reduce resolution / color depth",
                                "Disable visual effects",
                                "Investigate CPU/memory pressure on host"
                            }
                        });
                        break;
                }
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                RemediationSuggestions.Clear();
                foreach (var s in suggestions
                             .GroupBy(x => x.Title)
                             .Select(g => g.First())
                             .OrderByDescending(x => x.Priority)
                             .Take(10))
                {
                    RemediationSuggestions.Add(s);
                }
            });
        }
        catch (Exception)
        {
            // Best-effort; keep UI stable
        }
    }

    private void OnSecurityIncidentDetected(object? sender, SecurityIncident incident)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            if (!SecurityIncidents.Any(i => i.Id == incident.Id))
            {
                SecurityIncidents.Insert(0, incident);
                
                while (SecurityIncidents.Count > 20)
                {
                    SecurityIncidents.RemoveAt(SecurityIncidents.Count - 1);
                }
            }
        });
    }

    partial void OnSelectedPeriodChanged(AnalyticsPeriod value)
    {
        if (!IsLoading)
        {
            _ = Task.Run(RefreshAnalyticsAsync);
        }
    }

    partial void OnStartDateChanged(DateTime value)
    {
        if (!IsLoading && value < EndDate)
        {
            _ = Task.Run(RefreshAnalyticsAsync);
        }
    }

    partial void OnEndDateChanged(DateTime value)
    {
        if (!IsLoading && value > StartDate)
        {
            _ = Task.Run(RefreshAnalyticsAsync);
        }
    }
}
