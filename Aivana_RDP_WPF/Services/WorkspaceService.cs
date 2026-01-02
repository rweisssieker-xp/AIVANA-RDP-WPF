using Aivana_RDP_WPF.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Implementation of workspace service
/// </summary>
public class WorkspaceService : IWorkspaceService 
{
    private readonly ILogger<WorkspaceService> _logger;
    private readonly List<WorkspaceTab> _tabs = new();
    private readonly Dictionary<string, WorkspaceLayout> _layouts = new();
    private string _activeTabId = string.Empty;

    public event EventHandler<WorkspaceTab>? TabCreated;
    public event EventHandler<WorkspaceTab>? TabClosed;
    public event EventHandler<WorkspaceTab>? TabActivated;
    public event EventHandler<WorkspaceTab>? TabUpdated;

    public WorkspaceService(ILogger<WorkspaceService> logger)
    {
        _logger = logger;
        LoadDefaultLayouts();
    }

    public async Task<WorkspaceTab> CreateTabAsync(TabType type, object? parameters = null)
    {
        var tab = new WorkspaceTab
        {
            Type = type,
            Title = GetDefaultTabTitle(type),
            Icon = GetDefaultTabIcon(type),
            Status = TabStatus.Loading
        };

        await ConfigureTabAsync(tab, type, parameters);

        _tabs.Add(tab);
        
        if (_tabs.Count == 1)
        {
            await ActivateTabAsync(tab.Id);
        }

        TabCreated?.Invoke(this, tab);
        _logger.LogInformation("Created {TabType} tab with ID {TabId}", type, tab.Id);

        return tab;
    }

    public async Task CloseTabAsync(string tabId)
    {
        var tab = _tabs.FirstOrDefault(t => t.Id == tabId);
        if (tab == null)
        {
            _logger.LogWarning("Tab {TabId} not found for closing", tabId);
            return;
        }

        if (!tab.IsClosable)
        {
            _logger.LogWarning("Tab {TabId} is not closable", tabId);
            return;
        }

        await CleanupTabAsync(tab);

        _tabs.Remove(tab);

        if (_activeTabId == tabId && _tabs.Any())
        {
            var nextTab = _tabs.Last();
            await ActivateTabAsync(nextTab.Id);
        }

        TabClosed?.Invoke(this, tab);
        _logger.LogInformation("Closed tab {TabId}", tabId);
    }

    public async Task ActivateTabAsync(string tabId)
    {
        var tab = _tabs.FirstOrDefault(t => t.Id == tabId);
        if (tab == null)
        {
            _logger.LogWarning("Tab {TabId} not found for activation", tabId);
            return;
        }

        foreach (var t in _tabs)
        {
            t.IsActive = false;
        }

        tab.IsActive = true;
        tab.LastActivatedAt = DateTime.UtcNow;
        _activeTabId = tabId;

        TabActivated?.Invoke(this, tab);
        _logger.LogInformation("Activated tab {TabId}", tabId);
    }

    public async Task<List<WorkspaceTab>> GetAllTabsAsync()
    {
        return await Task.FromResult(_tabs.ToList());
    }

    public async Task<WorkspaceTab?> GetActiveTabAsync()
    {
        return await Task.FromResult(_tabs.FirstOrDefault(t => t.IsActive));
    }

    public async Task UpdateTabAsync(WorkspaceTab tab)
    {
        var existingTab = _tabs.FirstOrDefault(t => t.Id == tab.Id);
        if (existingTab != null)
        {
            existingTab.Title = tab.Title;
            existingTab.Status = tab.Status;
            existingTab.Icon = tab.Icon;
            existingTab.Metadata = tab.Metadata;
            existingTab.LastActivatedAt = DateTime.UtcNow;

            TabUpdated?.Invoke(this, existingTab);
            _logger.LogDebug("Updated tab {TabId}", tab.Id);
        }
    }

    public async Task<WorkspaceLayout> SaveLayoutAsync(string name)
    {
        var layout = new WorkspaceLayout
        {
            Name = name,
            Tabs = _tabs.Select(t => new WorkspaceTab
            {
                Id = t.Id,
                Title = t.Title,
                Type = t.Type,
                Icon = t.Icon,
                Metadata = new Dictionary<string, object>(t.Metadata),
                IsClosable = t.IsClosable
            }).ToList(),
            ActiveTabId = _activeTabId,
            ModifiedAt = DateTime.UtcNow
        };

        _layouts[layout.Id] = layout;
        
        await SaveLayoutToStorageAsync(layout);

        _logger.LogInformation("Saved layout {LayoutName} with {TabCount} tabs", name, layout.Tabs.Count);
        return layout;
    }

    public async Task<WorkspaceLayout> LoadLayoutAsync(string layoutId)
    {
        if (!_layouts.TryGetValue(layoutId, out var layout))
        {
            layout = await LoadLayoutFromStorageAsync(layoutId);
            if (layout != null)
            {
                _layouts[layoutId] = layout;
            }
        }

        if (layout == null)
        {
            throw new ArgumentException($"Layout {layoutId} not found");
        }

        var currentTabs = _tabs.ToList();
        foreach (var tab in currentTabs)
        {
            await CloseTabAsync(tab.Id);
        }

        foreach (var tabData in layout.Tabs)
        {
            var tab = await CreateTabAsync(tabData.Type);
            tab.Title = tabData.Title;
            tab.Icon = tabData.Icon;
            tab.Metadata = new Dictionary<string, object>(tabData.Metadata);
            tab.IsClosable = tabData.IsClosable;
            await UpdateTabAsync(tab);
        }

        if (!string.IsNullOrEmpty(layout.ActiveTabId))
        {
            await ActivateTabAsync(layout.ActiveTabId);
        }

        _logger.LogInformation("Loaded layout {LayoutName} with {TabCount} tabs", layout.Name, layout.Tabs.Count);
        return layout;
    }

    public async Task<List<WorkspaceLayout>> GetSavedLayoutsAsync()
    {
        return await Task.FromResult(_layouts.Values.ToList());
    }

    private async Task ConfigureTabAsync(WorkspaceTab tab, TabType type, object? parameters)
    {
        switch (type)
        {
            case TabType.RemoteSession:
                await ConfigureRemoteSessionTabAsync(tab, parameters);
                break;
            case TabType.LocalTerminal:
                await ConfigureTerminalTabAsync(tab, parameters);
                break;
            case TabType.FileExplorer:
                await ConfigureFileExplorerTabAsync(tab, parameters);
                break;
            case TabType.Settings:
                await ConfigureSettingsTabAsync(tab, parameters);
                break;
            case TabType.Dashboard:
                await ConfigureDashboardTabAsync(tab, parameters);
                break;
            default:
                tab.Status = TabStatus.Ready;
                break;
        }
    }

    private async Task ConfigureRemoteSessionTabAsync(WorkspaceTab tab, object? parameters)
    {
        if (parameters is ConnectionProfile profile)
        {
            tab.Title = profile.Name;
            tab.Metadata["ProfileId"] = profile.Id;
            tab.Metadata["ProtocolType"] = profile.ProtocolType;
            tab.Status = TabStatus.Ready;
        }
        else
        {
            tab.Status = TabStatus.Error;
        }
        
        await Task.CompletedTask;
    }

    private async Task ConfigureTerminalTabAsync(WorkspaceTab tab, object? parameters)
    {
        tab.Title = "Terminal";
        tab.Metadata["WorkingDirectory"] = parameters?.ToString() ?? Environment.CurrentDirectory;
        tab.Status = TabStatus.Ready;
        await Task.CompletedTask;
    }

    private async Task ConfigureFileExplorerTabAsync(WorkspaceTab tab, object? parameters)
    {
        tab.Title = "File Explorer";
        tab.Metadata["Path"] = parameters?.ToString() ?? Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
        tab.Status = TabStatus.Ready;
        await Task.CompletedTask;
    }

    private async Task ConfigureSettingsTabAsync(WorkspaceTab tab, object? parameters)
    {
        tab.Title = "Settings";
        tab.IsClosable = false;
        tab.Status = TabStatus.Ready;
        await Task.CompletedTask;
    }

    private async Task ConfigureDashboardTabAsync(WorkspaceTab tab, object? parameters)
    {
        tab.Title = "Dashboard";
        tab.Status = TabStatus.Ready;
        await Task.CompletedTask;
    }

    private async Task CleanupTabAsync(WorkspaceTab tab)
    {
        switch (tab.Type)
        {
            case TabType.RemoteSession:
                break;
            case TabType.LocalTerminal:
                break;
            default:
                break;
        }
        
        await Task.CompletedTask;
    }

    private string GetDefaultTabTitle(TabType type)
    {
        return type switch
        {
            TabType.RemoteSession => "Remote Session",
            TabType.LocalTerminal => "Terminal",
            TabType.FileExplorer => "File Explorer",
            TabType.Settings => "Settings",
            TabType.Dashboard => "Dashboard",
            _ => "New Tab"
        };
    }

    private string GetDefaultTabIcon(TabType type)
    {
        return type switch
        {
            TabType.RemoteSession => "🖥️",
            TabType.LocalTerminal => "💻",
            TabType.FileExplorer => "📁",
            TabType.Settings => "⚙️",
            TabType.Dashboard => "📊",
            _ => "📄"
        };
    }

    private void LoadDefaultLayouts()
    {
        var defaultLayout = new WorkspaceLayout
        {
            Name = "Default",
            IsDefault = true,
            Configuration = new LayoutConfiguration()
        };

        _layouts[defaultLayout.Id] = defaultLayout;
    }

    private async Task SaveLayoutToStorageAsync(WorkspaceLayout layout)
    {
        await Task.CompletedTask;
    }

    private async Task<WorkspaceLayout?> LoadLayoutFromStorageAsync(string layoutId)
    {
        return await Task.FromResult<WorkspaceLayout?>(null);
    }
}
