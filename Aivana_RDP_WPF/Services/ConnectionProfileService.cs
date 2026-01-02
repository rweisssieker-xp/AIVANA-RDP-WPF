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
        try
        {
            _logger.LogInformation("Creating connection profile {ProfileName}", profile.Name);
            
            // Validation
            if (string.IsNullOrWhiteSpace(profile.Name))
                throw new ArgumentException("Profile name is required");
            
            if (string.IsNullOrWhiteSpace(profile.ServerAddress))
                throw new ArgumentException("Server address is required");
            
            // Check for duplicate names
            var existingProfile = await _context.ConnectionProfiles
                .FirstOrDefaultAsync(p => p.Name == profile.Name, ct);
            
            if (existingProfile != null)
                throw new InvalidOperationException($"A profile with name '{profile.Name}' already exists");
            
            profile.CreatedAt = DateTime.UtcNow;
            profile.ConnectionCount = 0;
            
            _context.ConnectionProfiles.Add(profile);
            await _context.SaveChangesAsync(ct);
            
            _logger.LogInformation("Created connection profile {ProfileId} with name {ProfileName}", profile.Id, profile.Name);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating connection profile {ProfileName}", profile?.Name);
            throw;
        }
    }

    public async Task UpdateProfileAsync(ConnectionProfile profile, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating connection profile {ProfileId}", profile.Id);
            
            // Validation
            if (string.IsNullOrWhiteSpace(profile.Name))
                throw new ArgumentException("Profile name is required");
            
            if (string.IsNullOrWhiteSpace(profile.ServerAddress))
                throw new ArgumentException("Server address is required");
            
            // Check for duplicate names (excluding current profile)
            var existingProfile = await _context.ConnectionProfiles
                .FirstOrDefaultAsync(p => p.Name == profile.Name && p.Id != profile.Id, ct);
            
            if (existingProfile != null)
                throw new InvalidOperationException($"A profile with name '{profile.Name}' already exists");
            
            profile.UpdatedAt = DateTime.UtcNow;
            _context.ConnectionProfiles.Update(profile);
            await _context.SaveChangesAsync(ct);
            
            _logger.LogInformation("Updated connection profile {ProfileId}", profile.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating connection profile {ProfileId}", profile?.Id);
            throw;
        }
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

    public async Task ToggleFavoriteAsync(int profileId, CancellationToken ct = default)
    {
        _logger.LogInformation("Toggling favorite status for profile {ProfileId}", profileId);
        var profile = await GetProfileByIdAsync(profileId, ct);
        if (profile != null)
        {
            profile.IsFavorite = !profile.IsFavorite;
            await UpdateProfileAsync(profile, ct);
        }
    }

    public async Task<List<string>> GetAllGroupsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving all group names");
        return await _context.ConnectionProfiles
            .Where(p => !string.IsNullOrEmpty(p.GroupName))
            .Select(p => p.GroupName!)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync(ct);
    }

    public async Task<List<string>> GetAllTagsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving all tags");
        var allTags = new HashSet<string>();
        var profiles = await _context.ConnectionProfiles
            .Where(p => !string.IsNullOrEmpty(p.Tags) && p.Tags != "[]")
            .Select(p => p.Tags)
            .ToListAsync(ct);

        foreach (var tagsJson in profiles)
        {
            var tags = Helpers.TagHelper.ParseTags(tagsJson);
            foreach (var tag in tags)
            {
                allTags.Add(tag);
            }
        }

        return allTags.OrderBy(t => t).ToList();
    }
}

