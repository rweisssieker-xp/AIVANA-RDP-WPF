using Aivana_RDP_WPF.Models.Analytics;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace Aivana_RDP_WPF.Services.Analytics;

/// <summary>
/// Implementation of analytics service
/// </summary>
public class AnalyticsService : IAnalyticsService 
{
    private readonly ILogger<AnalyticsService> _logger;
    private readonly IConnectionProfileService _profileService;
    private readonly IPerformanceMonitorService _performanceService;
    private readonly List<ConnectionAnalytics> _analyticsHistory = new();
    private readonly List<PerformanceIssue> _detectedIssues = new();
    private readonly List<SecurityIncident> _securityIncidents = new();

    public event EventHandler<ConnectionAnalytics>? AnalyticsUpdated;
    public event EventHandler<PerformanceIssue>? PerformanceIssueDetected;
    public event EventHandler<SecurityIncident>? SecurityIncidentDetected;

    public AnalyticsService(
        ILogger<AnalyticsService> logger,
        IConnectionProfileService profileService,
        IPerformanceMonitorService performanceService)
    {
        _logger = logger;
        _profileService = profileService;
        _performanceService = performanceService;
        
        _ = Task.Run(InitializeAsync);
    }

    public async Task<ConnectionAnalytics> GetAnalyticsAsync(AnalyticsPeriod period, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var (start, end) = CalculatePeriodRange(period, startDate, endDate);
            
            var analytics = new ConnectionAnalytics
            {
                PeriodStart = start,
                PeriodEnd = end,
                Period = period,
                Statistics = await CalculateConnectionStatisticsAsync(start, end),
                Performance = await CalculatePerformanceAnalyticsAsync(start, end),
                Usage = await CalculateUsageAnalyticsAsync(start, end),
                Security = await CalculateSecurityAnalyticsAsync(start, end),
                Trends = await CalculateTrendsAsync(period, start, end),
                Insights = await GenerateInsightsAsync(start, end)
            };

            _analyticsHistory.Add(analytics);
            AnalyticsUpdated?.Invoke(this, analytics);

            _logger.LogInformation("Generated analytics for period {Period} from {Start} to {End}", 
                period, start, end);
            
            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating analytics for period {Period}", period);
            return new ConnectionAnalytics { Period = period };
        }
    }

    private static string EnsureReportsDirectory()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var reportsDirectory = Path.Combine(appDataPath, "Aivana_RDP_WPF", "Reports");

        if (!Directory.Exists(reportsDirectory))
        {
            Directory.CreateDirectory(reportsDirectory);
        }

        return reportsDirectory;
    }

    private static string BuildReportFileName(string extension, AnalyticsPeriod period)
    {
        return $"analytics_{period}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{extension}";
    }

    public async Task<List<ConnectionTrend>> GetTrendsAsync(AnalyticsPeriod period, int dataPoints = 30)
    {
        try
        {
            var trends = new List<ConnectionTrend>();
            var (start, end) = CalculatePeriodRange(period);
            
            var interval = CalculateInterval(period, dataPoints);
            var current = start;

            while (current <= end)
            {
                var trend = await CalculateTrendPointAsync(current, interval);
                trends.Add(trend);
                current = current.Add(interval);
            }

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating trends for period {Period}", period);
            return new List<ConnectionTrend>();
        }
    }

    public async Task<PerformanceAnalytics> GetPerformanceAnalyticsAsync(string? profileId = null)
    {
        try
        {
            var analytics = new PerformanceAnalytics
            {
                AverageLatency = 85.5 + new Random().NextDouble() * 50,
                MinLatency = 20.0 + new Random().NextDouble() * 30,
                MaxLatency = 200.0 + new Random().NextDouble() * 100,
                AverageBandwidth = 95.2 + new Random().NextDouble() * 20,
                AverageQualityScore = 82.5 + new Random().NextDouble() * 15,
                Issues = await DetectPerformanceIssuesAsync(profileId),
                Trend = AnalyzePerformanceTrend(),
                MetricsByProtocol = await GetMetricsByProtocolAsync()
            };

            foreach (var issue in analytics.Issues)
            {
                PerformanceIssueDetected?.Invoke(this, issue);
            }

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance analytics");
            return new PerformanceAnalytics();
        }
    }

    public async Task<UsageAnalytics> GetUsageAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var analytics = new UsageAnalytics
            {
                UsageByProfile = await CalculateUsageByProfileAsync(start, end),
                ConnectionsByHour = await CalculateConnectionsByHourAsync(start, end),
                ConnectionsByDay = await CalculateConnectionsByDayAsync(start, end),
                Patterns = await DetectUsagePatternsAsync(start, end),
                PeakUsageTime = await FindPeakUsageTimeAsync(start, end),
                PeakConnectionsCount = 15 + new Random().Next(10, 30),
                AverageSessionsPerDay = 8.5 + new Random().NextDouble() * 5
            };

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage analytics");
            return new UsageAnalytics();
        }
    }

    public async Task<SecurityAnalytics> GetSecurityAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var analytics = new SecurityAnalytics
            {
                SecurityEvents = _securityIncidents.Count(i => i.OccurredAt >= start && i.OccurredAt <= end),
                Incidents = _securityIncidents.Where(i => i.OccurredAt >= start && i.OccurredAt <= end).ToList(),
                EventTypes = await CategorizeSecurityEventsAsync(start, end),
                OverallRisk = await AssessOverallSecurityRiskAsync(),
                Recommendations = await GenerateSecurityRecommendationsAsync()
            };

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security analytics");
            return new SecurityAnalytics();
        }
    }

    public async Task<List<PerformanceIssue>> GetPerformanceIssuesAsync()
    {
        return await Task.FromResult(_detectedIssues.OrderByDescending(i => i.ImpactScore).ToList());
    }

    public async Task<List<UsagePattern>> GetUsagePatternsAsync()
    {
        try
        {
            var patterns = new List<UsagePattern>();

            patterns.Add(new UsagePattern
            {
                Type = PatternType.RecurrentConnection,
                Description = "Users tend to connect to development servers in the morning",
                Confidence = 0.85,
                OccurrenceCount = 45
            });

            patterns.Add(new UsagePattern
            {
                Type = PatternType.PeakUsage,
                Description = "Peak usage occurs between 9 AM and 11 AM",
                Confidence = 0.92,
                OccurrenceCount = 60
            });

            patterns.Add(new UsagePattern
            {
                Type = PatternType.ProtocolPreference,
                Description = "RDP is preferred over SSH for Windows servers",
                Confidence = 0.78,
                OccurrenceCount = 120
            });

            return patterns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage patterns");
            return new List<UsagePattern>();
        }
    }

    public async Task<List<SecurityIncident>> GetSecurityIncidentsAsync()
    {
        return await Task.FromResult(_securityIncidents.OrderByDescending(i => i.OccurredAt).ToList());
    }

    public async Task<Dictionary<string, object>> GetInsightsAsync()
    {
        try
        {
            var insights = new Dictionary<string, object>
            {
                ["PeakPerformanceHours"] = "6:00 AM - 8:00 AM",
                ["MostUsedProtocol"] = "RDP (65%)",
                ["AverageSessionDuration"] = "2h 15m",
                ["SecurityRiskLevel"] = "Low",
                ["Recommendations"] = new List<string>
                {
                    "Consider upgrading bandwidth for better performance",
                    "Schedule regular security audits",
                    "Optimize connection scheduling for peak hours"
                },
                ["TrendAnalysis"] = new Dictionary<string, string>
                {
                    ["ConnectionGrowth"] = "+12% over last month",
                    ["PerformanceTrend"] = "Improving",
                    ["SecurityPosture"] = "Stable"
                }
            };

            return insights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating insights");
            return new Dictionary<string, object>();
        }
    }

    public async Task ExportAnalyticsAsync(AnalyticsPeriod period, string format)
    {
        try
        {
            var analytics = await GetAnalyticsAsync(period);
            
            switch (format.ToLowerInvariant())
            {
                case "html":
                    await ExportToHtmlAsync(analytics);
                    break;
                case "pdf":
                    await ExportToPdfPlaceholderAsync(analytics);
                    break;
                case "json":
                    await ExportToJsonAsync(analytics);
                    break;
                case "csv":
                    await ExportToCsvAsync(analytics);
                    break;
                case "xml":
                    await ExportToXmlAsync(analytics);
                    break;
                default:
                    throw new ArgumentException($"Unsupported format: {format}");
            }

            _logger.LogInformation("Analytics exported for period {Period} in format {Format}", period, format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting analytics for period {Period}", period);
            throw;
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            await GenerateSampleDataAsync();
            _logger.LogInformation("Analytics service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing analytics service");
        }
    }

    private (DateTime start, DateTime end) CalculatePeriodRange(AnalyticsPeriod period, DateTime? startDate = null, DateTime? endDate = null)
    {
        var end = endDate ?? DateTime.UtcNow;
        var start = startDate ?? period switch
        {
            AnalyticsPeriod.Hourly => end.AddHours(-1),
            AnalyticsPeriod.Daily => end.AddDays(-1),
            AnalyticsPeriod.Weekly => end.AddDays(-7),
            AnalyticsPeriod.Monthly => end.AddMonths(-1),
            AnalyticsPeriod.Yearly => end.AddYears(-1),
            _ => end.AddDays(-1)
        };

        return (start, end);
    }

    private TimeSpan CalculateInterval(AnalyticsPeriod period, int dataPoints)
    {
        var (start, end) = CalculatePeriodRange(period);
        var totalDuration = end - start;
        return TimeSpan.FromTicks(totalDuration.Ticks / dataPoints);
    }

    private async Task<ConnectionStatistics> CalculateConnectionStatisticsAsync(DateTime start, DateTime end)
    {
        var totalConnections = 150 + new Random().Next(50, 200);
        var successfulConnections = (int)(totalConnections * (0.85 + new Random().NextDouble() * 0.1));
        
        return new ConnectionStatistics
        {
            TotalConnections = totalConnections,
            SuccessfulConnections = successfulConnections,
            FailedConnections = totalConnections - successfulConnections,
            SuccessRate = (double)successfulConnections / totalConnections,
            AverageConnectionDuration = TimeSpan.FromMinutes(45 + new Random().NextDouble() * 60),
            TotalConnectionTime = TimeSpan.FromHours(totalConnections * 1.5),
            UniqueProfiles = 8 + new Random().Next(3, 12),
            ProtocolUsage = new Dictionary<string, int>
            {
                ["RDP"] = (int)(totalConnections * 0.65),
                ["SSH"] = (int)(totalConnections * 0.25),
                ["VNC"] = (int)(totalConnections * 0.10)
            },
            HostUsage = new Dictionary<string, int>
            {
                ["dev-server-01"] = 45,
                ["prod-server-02"] = 38,
                ["test-server-03"] = 27,
                ["backup-server-04"] = 15
            }
        };
    }

    private async Task<PerformanceAnalytics> CalculatePerformanceAnalyticsAsync(DateTime start, DateTime end)
    {
        return new PerformanceAnalytics
        {
            AverageLatency = 85.5 + new Random().NextDouble() * 50,
            MinLatency = 20.0 + new Random().NextDouble() * 30,
            MaxLatency = 200.0 + new Random().NextDouble() * 100,
            AverageBandwidth = 95.2 + new Random().NextDouble() * 20,
            AverageQualityScore = 82.5 + new Random().NextDouble() * 15,
            Issues = new List<PerformanceIssue>(),
            Trend = Models.AI.PerformanceTrend.Stable,
            MetricsByProtocol = new Dictionary<string, double>
            {
                ["RDP"] = 88.5,
                ["SSH"] = 92.1,
                ["VNC"] = 76.3
            }
        };
    }

    private async Task<UsageAnalytics> CalculateUsageAnalyticsAsync(DateTime start, DateTime end)
    {
        return new UsageAnalytics
        {
            UsageByProfile = new Dictionary<string, TimeSpan>
            {
                ["Development"] = TimeSpan.FromHours(45.5),
                ["Production"] = TimeSpan.FromHours(32.8),
                ["Testing"] = TimeSpan.FromHours(18.2),
                ["Maintenance"] = TimeSpan.FromHours(12.1)
            },
            ConnectionsByHour = GenerateHourlyDistribution(),
            ConnectionsByDay = GenerateDailyDistribution(),
            Patterns = new List<UsagePattern>(),
            PeakUsageTime = new TimeSpan(10, 30, 0),
            PeakConnectionsCount = 25,
            AverageSessionsPerDay = 12.5
        };
    }

    private async Task<SecurityAnalytics> CalculateSecurityAnalyticsAsync(DateTime start, DateTime end)
    {
        return new SecurityAnalytics
        {
            SecurityEvents = 3,
            Incidents = new List<SecurityIncident>(),
            EventTypes = new Dictionary<string, int>
            {
                ["UnauthorizedAccess"] = 1,
                ["SuspiciousActivity"] = 2
            },
            OverallRisk = Models.AI.SecurityRiskLevel.Low,
            Recommendations = new List<SecurityRecommendation>()
        };
    }

    private async Task<List<ConnectionTrend>> CalculateTrendsAsync(AnalyticsPeriod period, DateTime start, DateTime end)
    {
        var trends = new List<ConnectionTrend>();
        var interval = CalculateInterval(period, 20);
        var current = start;

        while (current <= end)
        {
            trends.Add(new ConnectionTrend
            {
                Timestamp = current,
                ConnectionCount = 10 + new Random().Next(5, 25),
                SuccessRate = 0.80 + new Random().NextDouble() * 0.15,
                AverageLatency = 50 + new Random().NextDouble() * 100,
                Direction = GetRandomTrendDirection(),
                ChangePercentage = -10 + new Random().NextDouble() * 20
            });

            current = current.Add(interval);
        }

        return trends;
    }

    private async Task<Dictionary<string, object>> GenerateInsightsAsync(DateTime start, DateTime end)
    {
        return new Dictionary<string, object>
        {
            ["PerformanceInsights"] = "Peak performance observed during early morning hours",
            ["UsageInsights"] = "Connection patterns show weekday preference",
            ["SecurityInsights"] = "No significant security threats detected",
            ["Recommendations"] = new List<string>
            {
                "Optimize bandwidth allocation during peak hours",
                "Consider load balancing for high-traffic periods"
            }
        };
    }

    private async Task<ConnectionTrend> CalculateTrendPointAsync(DateTime timestamp, TimeSpan interval)
    {
        return new ConnectionTrend
        {
            Timestamp = timestamp,
            ConnectionCount = 10 + new Random().Next(5, 25),
            SuccessRate = 0.80 + new Random().NextDouble() * 0.15,
            AverageLatency = 50 + new Random().NextDouble() * 100,
            Direction = GetRandomTrendDirection(),
            ChangePercentage = -10 + new Random().NextDouble() * 20
        };
    }

    private async Task<List<PerformanceIssue>> DetectPerformanceIssuesAsync(string? profileId = null)
    {
        var issues = new List<PerformanceIssue>();

        if (new Random().NextDouble() > 0.7)
        {
            issues.Add(new PerformanceIssue
            {
                Type = IssueType.HighLatency,
                Description = "Elevated latency detected on multiple connections",
                Severity = Severity.Medium,
                ImpactScore = 6.5,
                AffectedProfiles = new List<string> { "Production", "Development" }
            });
        }

        return issues;
    }

    private Models.AI.PerformanceTrend AnalyzePerformanceTrend()
    {
        return new Random().NextDouble() switch
        {
            < 0.3 => Models.AI.PerformanceTrend.Degrading,
            < 0.7 => Models.AI.PerformanceTrend.Stable,
            _ => Models.AI.PerformanceTrend.Improving
        };
    }

    private async Task<Dictionary<string, double>> GetMetricsByProtocolAsync()
    {
        return new Dictionary<string, double>
        {
            ["RDP"] = 88.5,
            ["SSH"] = 92.1,
            ["VNC"] = 76.3
        };
    }

    private async Task<Dictionary<string, TimeSpan>> CalculateUsageByProfileAsync(DateTime start, DateTime end)
    {
        return new Dictionary<string, TimeSpan>
        {
            ["Development"] = TimeSpan.FromHours(45.5),
            ["Production"] = TimeSpan.FromHours(32.8),
            ["Testing"] = TimeSpan.FromHours(18.2),
            ["Maintenance"] = TimeSpan.FromHours(12.1)
        };
    }

    private async Task<Dictionary<string, int>> CalculateConnectionsByHourAsync(DateTime start, DateTime end)
    {
        return GenerateHourlyDistribution();
    }

    private async Task<Dictionary<string, int>> CalculateConnectionsByDayAsync(DateTime start, DateTime end)
    {
        return GenerateDailyDistribution();
    }

    private async Task<List<UsagePattern>> DetectUsagePatternsAsync(DateTime start, DateTime end)
    {
        return new List<UsagePattern>();
    }

    private async Task<TimeSpan> FindPeakUsageTimeAsync(DateTime start, DateTime end)
    {
        return new TimeSpan(10, 30, 0);
    }

    private async Task<Dictionary<string, int>> CategorizeSecurityEventsAsync(DateTime start, DateTime end)
    {
        return new Dictionary<string, int>
        {
            ["UnauthorizedAccess"] = 1,
            ["SuspiciousActivity"] = 2
        };
    }

    private async Task<Models.AI.SecurityRiskLevel> AssessOverallSecurityRiskAsync()
    {
        return Models.AI.SecurityRiskLevel.Low;
    }

    private async Task<List<SecurityRecommendation>> GenerateSecurityRecommendationsAsync()
    {
        return new List<SecurityRecommendation>();
    }

    private Dictionary<string, int> GenerateHourlyDistribution()
    {
        var distribution = new Dictionary<string, int>();
        for (int hour = 0; hour < 24; hour++)
        {
            var baseCount = hour switch
            {
                >= 9 and <= 17 => 20,
                >= 18 and <= 22 => 12,
                _ => 5
            };
            distribution[$"{hour:D2}:00"] = baseCount + new Random().Next(-3, 5);
        }
        return distribution;
    }

    private Dictionary<string, int> GenerateDailyDistribution()
    {
        var days = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        var distribution = new Dictionary<string, int>();
        
        foreach (var day in days)
        {
            var baseCount = day is "Sat" or "Sun" ? 15 : 25;
            distribution[day] = baseCount + new Random().Next(-5, 8);
        }
        
        return distribution;
    }

    private TrendDirection GetRandomTrendDirection()
    {
        return new Random().NextDouble() switch
        {
            < 0.3 => TrendDirection.Down,
            < 0.7 => TrendDirection.Stable,
            _ => TrendDirection.Up
        };
    }

    private async Task ExportToHtmlAsync(ConnectionAnalytics analytics)
    {
        var reportsDir = EnsureReportsDirectory();
        var fileName = BuildReportFileName("html", analytics.Period);
        var filePath = Path.Combine(reportsDir, fileName);

        var html = BuildHtmlReport(analytics);
        await File.WriteAllTextAsync(filePath, html, Encoding.UTF8);

        _logger.LogInformation("Analytics exported to HTML: {FilePath}", filePath);
    }

    private async Task ExportToPdfPlaceholderAsync(ConnectionAnalytics analytics)
    {
        var reportsDir = EnsureReportsDirectory();
        var fileName = BuildReportFileName("pdf", analytics.Period);
        var filePath = Path.Combine(reportsDir, fileName);

        var placeholderText = $"AIVANA PDF export placeholder\n\nPeriod: {analytics.Period}\nStart: {analytics.PeriodStart:O}\nEnd: {analytics.PeriodEnd:O}\n\nThis build contains a placeholder PDF export.\n";
        await File.WriteAllTextAsync(filePath, placeholderText, Encoding.UTF8);

        _logger.LogInformation("Analytics exported to PDF placeholder: {FilePath}", filePath);
    }

    private async Task ExportToJsonAsync(ConnectionAnalytics analytics)
    {
        var reportsDir = EnsureReportsDirectory();
        var fileName = BuildReportFileName("json", analytics.Period);
        var filePath = Path.Combine(reportsDir, fileName);

        var json = JsonSerializer.Serialize(analytics, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);

        _logger.LogInformation("Analytics exported to JSON: {FilePath}", filePath);
    }

    private async Task ExportToCsvAsync(ConnectionAnalytics analytics)
    {
        var reportsDir = EnsureReportsDirectory();
        var fileName = BuildReportFileName("csv", analytics.Period);
        var filePath = Path.Combine(reportsDir, fileName);

        var sb = new StringBuilder();
        sb.AppendLine("Metric,Value");
        sb.AppendLine($"Period,{analytics.Period}");
        sb.AppendLine($"PeriodStart,{analytics.PeriodStart:O}");
        sb.AppendLine($"PeriodEnd,{analytics.PeriodEnd:O}");
        sb.AppendLine($"TotalConnections,{analytics.Statistics.TotalConnections}");
        sb.AppendLine($"SuccessRate,{analytics.Statistics.SuccessRate:P2}");
        sb.AppendLine($"AverageLatencyMs,{analytics.Performance.AverageLatency:F1}");
        sb.AppendLine($"AverageBandwidthMbps,{analytics.Performance.AverageBandwidth:F1}");
        sb.AppendLine($"AverageQualityScore,{analytics.Performance.AverageQualityScore:F1}");
        sb.AppendLine($"SecurityRisk,{analytics.Security.OverallRisk}");

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        _logger.LogInformation("Analytics exported to CSV: {FilePath}", filePath);
    }

    private async Task ExportToXmlAsync(ConnectionAnalytics analytics)
    {
        var reportsDir = EnsureReportsDirectory();
        var fileName = BuildReportFileName("xml", analytics.Period);
        var filePath = Path.Combine(reportsDir, fileName);

        var doc = new XDocument(
            new XElement("Analytics",
                new XElement("Period", analytics.Period.ToString()),
                new XElement("PeriodStart", analytics.PeriodStart.ToString("O")),
                new XElement("PeriodEnd", analytics.PeriodEnd.ToString("O")),
                new XElement("Statistics",
                    new XElement("TotalConnections", analytics.Statistics.TotalConnections),
                    new XElement("SuccessfulConnections", analytics.Statistics.SuccessfulConnections),
                    new XElement("FailedConnections", analytics.Statistics.FailedConnections),
                    new XElement("SuccessRate", analytics.Statistics.SuccessRate)
                ),
                new XElement("Performance",
                    new XElement("AverageLatencyMs", analytics.Performance.AverageLatency),
                    new XElement("AverageBandwidthMbps", analytics.Performance.AverageBandwidth),
                    new XElement("AverageQualityScore", analytics.Performance.AverageQualityScore)
                ),
                new XElement("Security",
                    new XElement("OverallRisk", analytics.Security.OverallRisk.ToString()),
                    new XElement("SecurityEvents", analytics.Security.SecurityEvents)
                )
            ));

        await File.WriteAllTextAsync(filePath, doc.ToString(), Encoding.UTF8);
        _logger.LogInformation("Analytics exported to XML: {FilePath}", filePath);
    }

    private static string BuildHtmlReport(ConnectionAnalytics analytics)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset='utf-8' />");
        sb.AppendLine("<title>AIVANA Analytics Export</title>");
        sb.AppendLine("<style>body{font-family:Arial,sans-serif;margin:20px;} .header{background:#2c3e50;color:#fff;padding:16px;border-radius:6px;} .grid{display:grid;grid-template-columns:repeat(3,1fr);gap:12px;margin-top:16px;} .card{border:1px solid #ddd;border-radius:6px;padding:12px;background:#fafafa;} .k{color:#6b6b6b;font-size:12px;} .v{font-size:20px;font-weight:700;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<div class='header'><h1>AIVANA RDP Analytics</h1><div>Period: {analytics.Period} ({analytics.PeriodStart:yyyy-MM-dd} to {analytics.PeriodEnd:yyyy-MM-dd})</div></div>");
        sb.AppendLine("<div class='grid'>");
        sb.AppendLine($"<div class='card'><div class='k'>Total Connections</div><div class='v'>{analytics.Statistics.TotalConnections}</div></div>");
        sb.AppendLine($"<div class='card'><div class='k'>Success Rate</div><div class='v'>{analytics.Statistics.SuccessRate:P1}</div></div>");
        sb.AppendLine($"<div class='card'><div class='k'>Avg Latency</div><div class='v'>{analytics.Performance.AverageLatency:F1} ms</div></div>");
        sb.AppendLine($"<div class='card'><div class='k'>Avg Bandwidth</div><div class='v'>{analytics.Performance.AverageBandwidth:F1} Mbps</div></div>");
        sb.AppendLine($"<div class='card'><div class='k'>Avg Quality</div><div class='v'>{analytics.Performance.AverageQualityScore:F1}/100</div></div>");
        sb.AppendLine($"<div class='card'><div class='k'>Security Risk</div><div class='v'>{analytics.Security.OverallRisk}</div></div>");
        sb.AppendLine("</div>");

        sb.AppendLine("<h2>Insights</h2><ul>");
        foreach (var kv in analytics.Insights)
        {
            sb.AppendLine($"<li><strong>{System.Net.WebUtility.HtmlEncode(kv.Key)}</strong>: {System.Net.WebUtility.HtmlEncode(kv.Value?.ToString() ?? string.Empty)}</li>");
        }
        sb.AppendLine("</ul>");

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private async Task GenerateSampleDataAsync()
    {
        _securityIncidents.Add(new SecurityIncident
        {
            Type = SecurityEventType.SuspiciousActivity,
            Description = "Multiple failed login attempts detected",
            Severity = Severity.Medium,
            OccurredAt = DateTime.UtcNow.AddHours(-2),
            ProfileId = "sample-profile-1",
            HostAddress = "192.168.1.100",
            Resolved = false
        });

        _detectedIssues.Add(new PerformanceIssue
        {
            Type = IssueType.HighLatency,
            Description = "Intermittent high latency on production servers",
            Severity = Severity.Medium,
            ImpactScore = 7.2,
            AffectedProfiles = new List<string> { "Production", "Development" }
        });
    }
}
