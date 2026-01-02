using Aivana_RDP_WPF.Models.Analytics;

namespace Aivana_RDP_WPF.Services.Analytics;

/// <summary>
/// Service interface for reporting functionality
/// </summary>
public interface IReportingService 
{
    Task<byte[]> GenerateReportAsync(ReportType type, AnalyticsPeriod period, DateTime? startDate = null, DateTime? endDate = null);
    Task<string> GenerateHtmlReportAsync(ConnectionAnalytics analytics);
    Task<byte[]> GeneratePdfReportAsync(ConnectionAnalytics analytics);
    Task<byte[]> GenerateExcelReportAsync(ConnectionAnalytics analytics);
    Task<List<ReportTemplate>> GetReportTemplatesAsync();
    Task SaveReportTemplateAsync(ReportTemplate template);
    Task ScheduleReportAsync(ScheduledReport report);
    Task<List<ScheduledReport>> GetScheduledReportsAsync();
}

/// <summary>
/// Report types
/// </summary>
public enum ReportType 
{
    Summary,
    Detailed,
    Performance,
    Security,
    Usage,
    Custom
}

/// <summary>
/// Report template configuration
/// </summary>
public class ReportTemplate 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDefault { get; set; }
}

/// <summary>
/// Scheduled report configuration
/// </summary>
public class ScheduledReport 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public AnalyticsPeriod Period { get; set; }
    public string Recipients { get; set; } = string.Empty;
    public DateTime NextRun { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
