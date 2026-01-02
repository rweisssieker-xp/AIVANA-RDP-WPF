using Aivana_RDP_WPF.Models.Analytics;

namespace Aivana_RDP_WPF.Services.Analytics;

/// <summary>
/// Service interface for comprehensive analytics
/// </summary>
public interface IAnalyticsService 
{
    Task<ConnectionAnalytics> GetAnalyticsAsync(AnalyticsPeriod period, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<ConnectionTrend>> GetTrendsAsync(AnalyticsPeriod period, int dataPoints = 30);
    Task<PerformanceAnalytics> GetPerformanceAnalyticsAsync(string? profileId = null);
    Task<UsageAnalytics> GetUsageAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<SecurityAnalytics> GetSecurityAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<PerformanceIssue>> GetPerformanceIssuesAsync();
    Task<List<UsagePattern>> GetUsagePatternsAsync();
    Task<List<SecurityIncident>> GetSecurityIncidentsAsync();
    Task<Dictionary<string, object>> GetInsightsAsync();
    Task ExportAnalyticsAsync(AnalyticsPeriod period, string format);
    event EventHandler<ConnectionAnalytics>? AnalyticsUpdated;
    event EventHandler<PerformanceIssue>? PerformanceIssueDetected;
    event EventHandler<SecurityIncident>? SecurityIncidentDetected;
}
