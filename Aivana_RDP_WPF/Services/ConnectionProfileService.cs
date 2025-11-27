using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Infrastructure.Database;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for managing connection profiles.
/// </summary>
public class ConnectionProfileService : IConnectionProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConnectionProfileService> _logger;

    public ConnectionProfileService(
        ApplicationDbContext context,
        ILogger<ConnectionProfileService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ConnectionProfile>> GetAllProfilesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving all connection profiles");
        return await _context.ConnectionProfiles
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<ConnectionProfile?> GetProfileByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving connection profile {ProfileId}", id);
        return await _context.ConnectionProfiles
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<ConnectionProfile> CreateProfileAsync(ConnectionProfile profile, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating connection profile {ProfileName}", profile.Name);
        
        profile.CreatedAt = DateTime.UtcNow;
        profile.ConnectionCount = 0;
        
        _context.ConnectionProfiles.Add(profile);
        await _context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Created connection profile {ProfileId} with name {ProfileName}", profile.Id, profile.Name);
        return profile;
    }

    public async Task UpdateProfileAsync(ConnectionProfile profile, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating connection profile {ProfileId}", profile.Id);
        
        _context.ConnectionProfiles.Update(profile);
        await _context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Updated connection profile {ProfileId}", profile.Id);
    }

    public async Task DeleteProfileAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting connection profile {ProfileId}", id);
        
        var profile = await GetProfileByIdAsync(id, ct);
        if (profile != null)
        {
            _context.ConnectionProfiles.Remove(profile);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Deleted connection profile {ProfileId}", id);
        }
    }

    public async Task<List<ConnectionProfile>> GetFavoritesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving favorite connection profiles");
        return await _context.ConnectionProfiles
            .Where(p => p.IsFavorite)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<List<ConnectionProfile>> GetByGroupAsync(string groupName, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving connection profiles for group {GroupName}", groupName);
        return await _context.ConnectionProfiles
            .Where(p => p.GroupName == groupName)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<List<ConnectionProfile>> GetByTagAsync(string tag, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving connection profiles with tag {Tag}", tag);
        return await _context.ConnectionProfiles
            .Where(p => p.Tags.Contains(tag))
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }
}

