using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for managing connection profiles.
/// </summary>
public interface IConnectionProfileService
{
    Task<List<ConnectionProfile>> GetAllProfilesAsync(CancellationToken ct = default);
    Task<ConnectionProfile?> GetProfileByIdAsync(int id, CancellationToken ct = default);
    Task<ConnectionProfile> CreateProfileAsync(ConnectionProfile profile, CancellationToken ct = default);
    Task UpdateProfileAsync(ConnectionProfile profile, CancellationToken ct = default);
    Task DeleteProfileAsync(int id, CancellationToken ct = default);
    Task<List<ConnectionProfile>> GetFavoritesAsync(CancellationToken ct = default);
    Task<List<ConnectionProfile>> GetByGroupAsync(string groupName, CancellationToken ct = default);
    Task<List<ConnectionProfile>> GetByTagAsync(string tag, CancellationToken ct = default);
    Task ToggleFavoriteAsync(int profileId, CancellationToken ct = default);
    Task<List<string>> GetAllGroupsAsync(CancellationToken ct = default);
    Task<List<string>> GetAllTagsAsync(CancellationToken ct = default);
}

