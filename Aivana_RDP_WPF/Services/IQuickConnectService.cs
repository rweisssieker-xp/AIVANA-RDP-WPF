using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Infrastructure.Protocols;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service for quick connection functionality
/// </summary>
public interface IQuickConnectService 
{
    Task<List<ConnectionSuggestion>> GetSuggestionsAsync(string query, CancellationToken ct = default);
    Task<ConnectionProfile> CreateProfileFromQuickConnectAsync(string connectionString, CancellationToken ct = default);
    Task<ConnectionResult> QuickConnectAsync(string connectionString, CancellationToken ct = default);
    Task<List<string>> GetRecentQueriesAsync(int count = 10);
    Task AddRecentQueryAsync(string query);
}
