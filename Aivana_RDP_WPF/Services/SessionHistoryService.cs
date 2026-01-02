using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aivana_RDP_WPF.Infrastructure.Database;
using Aivana_RDP_WPF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Services;

public class SessionHistoryService : ISessionHistoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SessionHistoryService> _logger;

    public SessionHistoryService(ApplicationDbContext context, ILogger<SessionHistoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SessionHistory> StartSessionAsync(int connectionProfileId, CancellationToken ct = default)
    {
        var entry = new SessionHistory
        {
            ConnectionProfileId = connectionProfileId,
            ConnectedAt = DateTime.UtcNow,
            Status = "Connecting",
            DurationTicks = 0
        };

        _context.SessionHistories.Add(entry);
        await _context.SaveChangesAsync(ct);
        return entry;
    }

    public async Task CompleteSessionAsync(int sessionHistoryId, string status, string? errorMessage, CancellationToken ct = default)
    {
        var entry = await _context.SessionHistories.FirstOrDefaultAsync(s => s.Id == sessionHistoryId, ct);
        if (entry == null)
        {
            return;
        }

        entry.DisconnectedAt = DateTime.UtcNow;
        entry.Status = status;
        entry.ErrorMessage = errorMessage;

        if (entry.DisconnectedAt.HasValue)
        {
            entry.DurationTicks = (entry.DisconnectedAt.Value - entry.ConnectedAt).Ticks;
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Session history {SessionHistoryId} completed with status {Status}", sessionHistoryId, status);
    }

    public async Task<List<SessionHistory>> GetRecentAsync(int connectionProfileId, int take = 50, CancellationToken ct = default)
    {
        return await _context.SessionHistories
            .AsNoTracking()
            .Where(s => s.ConnectionProfileId == connectionProfileId)
            .OrderByDescending(s => s.ConnectedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<List<SessionHistory>> GetRecentGlobalAsync(int take = 200, CancellationToken ct = default)
    {
        return await _context.SessionHistories
            .AsNoTracking()
            .Include(s => s.ConnectionProfile)
            .OrderByDescending(s => s.ConnectedAt)
            .Take(take)
            .ToListAsync(ct);
    }
}
