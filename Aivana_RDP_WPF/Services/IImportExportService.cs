using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for importing and exporting connection profiles.
/// </summary>
public interface IImportExportService
{
    Task<IEnumerable<ConnectionProfile>> ImportFromRdpFileAsync(string filePath, CancellationToken ct = default);
    Task<IEnumerable<ConnectionProfile>> ImportFromJsonFileAsync(string filePath, CancellationToken ct = default);
    Task<IEnumerable<ConnectionProfile>> ImportFromCsvFileAsync(string filePath, CancellationToken ct = default);
    Task ExportToRdpFileAsync(ConnectionProfile profile, string filePath, CancellationToken ct = default);
    Task ExportToJsonFileAsync(IEnumerable<ConnectionProfile> profiles, string filePath, CancellationToken ct = default);
    Task ExportToCsvFileAsync(IEnumerable<ConnectionProfile> profiles, string filePath, CancellationToken ct = default);
}

