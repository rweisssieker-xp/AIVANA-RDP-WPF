using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for layout persistence
/// </summary>
public interface ILayoutPersistenceService 
{
    Task SaveLayoutAsync(WorkspaceLayout layout);
    Task<WorkspaceLayout?> LoadLayoutAsync(string layoutId);
    Task<List<WorkspaceLayout>> GetAllLayoutsAsync();
    Task DeleteLayoutAsync(string layoutId);
    Task SetDefaultLayoutAsync(string layoutId);
    Task<WorkspaceLayout?> GetDefaultLayoutAsync();
}
