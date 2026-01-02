using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for workspace management
/// </summary>
public interface IWorkspaceService 
{
    Task<WorkspaceTab> CreateTabAsync(TabType type, object? parameters = null);
    Task CloseTabAsync(string tabId);
    Task ActivateTabAsync(string tabId);
    Task<List<WorkspaceTab>> GetAllTabsAsync();
    Task<WorkspaceTab?> GetActiveTabAsync();
    Task UpdateTabAsync(WorkspaceTab tab);
    Task<WorkspaceLayout> SaveLayoutAsync(string name);
    Task<WorkspaceLayout> LoadLayoutAsync(string layoutId);
    Task<List<WorkspaceLayout>> GetSavedLayoutsAsync();
    event EventHandler<WorkspaceTab>? TabCreated;
    event EventHandler<WorkspaceTab>? TabClosed;
    event EventHandler<WorkspaceTab>? TabActivated;
    event EventHandler<WorkspaceTab>? TabUpdated;
}
