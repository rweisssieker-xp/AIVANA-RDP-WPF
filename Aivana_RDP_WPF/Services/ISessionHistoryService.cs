using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

public interface ISessionHistoryService
{
    Task<SessionHistory> StartSessionAsync(int connectionProfileId, CancellationToken ct = default);
    Task CompleteSessionAsync(int sessionHistoryId, string status, string? errorMessage, CancellationToken ct = default);
    Task<List<SessionHistory>> GetRecentAsync(int connectionProfileId, int take = 50, CancellationToken ct = default);
    Task<List<SessionHistory>> GetRecentGlobalAsync(int take = 200, CancellationToken ct = default);
}
