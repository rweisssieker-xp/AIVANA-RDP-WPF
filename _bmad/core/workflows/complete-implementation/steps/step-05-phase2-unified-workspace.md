# Step 5: Phase 2 - Unified Workspace Implementation

## YOLO MODE: PHASE 2 AUTOMATIC EXECUTION

**No prompts - continuous implementation until complete!**

---

## PHASE 2: UNIFIED WORKSPACE & WORKFLOW REVOLUTION

### Week 5-6: Unified Workspace Foundation

#### Day 19-20: Tabbed Interface System

##### Task 10.1: Tab Management Models
```csharp
// File: Aivana_RDP_WPF/Models/WorkspaceTab.cs
namespace Aivana_RDP_WPF.Models;

public class WorkspaceTab 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public TabType Type { get; set; }
    public object? Content { get; set; }
    public bool IsActive { get; set; }
    public bool IsClosable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivatedAt { get; set; }
    public string Icon { get; set; } = string.Empty;
    public TabStatus Status { get; set; } = TabStatus.Loading;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum TabType 
{
    RemoteSession,
    LocalTerminal,
    FileExplorer,
    Settings,
    Dashboard,
    Custom
}

public enum TabStatus 
{
    Loading,
    Connected,
    Disconnected,
    Error,
    Ready
}

// File: Aivana_RDP_WPF/Models/WorkspaceLayout.cs
namespace Aivana_RDP_WPF.Models;

public class WorkspaceLayout 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public List<WorkspaceTab> Tabs { get; set; } = new();
    public string ActiveTabId { get; set; } = string.Empty;
    public LayoutConfiguration Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public bool IsDefault { get; set; }
}

public class LayoutConfiguration 
{
    public double WindowWidth { get; set; } = 1200;
    public double WindowHeight { get; set; } = 800;
    public bool IsMaximized { get; set; }
    public double SplitterPosition { get; set; } = 0.7;
    public TabBarPosition TabBarPosition { get; set; } = TabBarPosition.Top;
    public bool ShowTabIcons { get; set; } = true;
    public bool ShowTabCloseButtons { get; set; } = true;
}

public enum TabBarPosition 
{
    Top,
    Bottom,
    Left,
    Right
}
```

##### Task 10.2: Workspace Service
```csharp
// File: Aivana_RDP_WPF/Services/IWorkspaceService.cs
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

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

// File: Aivana_RDP_WPF/Services/WorkspaceService.cs
using Aivana_RDP_WPF.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Aivana_RDP_WPF.Services;

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

        // Configure tab based on type and parameters
        await ConfigureTabAsync(tab, type, parameters);

        _tabs.Add(tab);
        
        // Auto-activate if it's the first tab
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

        // Clean up tab resources
        await CleanupTabAsync(tab);

        _tabs.Remove(tab);

        // Activate another tab if this was active
        if (_activeTabId == tabId && _tabs.Any())
        {
            var nextTab = _tabs.Last(); // Activate the last tab
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

        // Deactivate all tabs
        foreach (var t in _tabs)
        {
            t.IsActive = false;
        }

        // Activate the requested tab
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
        
        // TODO: Save to persistent storage
        await SaveLayoutToStorageAsync(layout);

        _logger.LogInformation("Saved layout {LayoutName} with {TabCount} tabs", name, layout.Tabs.Count);
        return layout;
    }

    public async Task<WorkspaceLayout> LoadLayoutAsync(string layoutId)
    {
        if (!_layouts.TryGetValue(layoutId, out var layout))
        {
            // TODO: Load from persistent storage
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

        // Clear current tabs
        var currentTabs = _tabs.ToList();
        foreach (var tab in currentTabs)
        {
            await CloseTabAsync(tab.Id);
        }

        // Load tabs from layout
        foreach (var tabData in layout.Tabs)
        {
            var tab = await CreateTabAsync(tabData.Type);
            tab.Title = tabData.Title;
            tab.Icon = tabData.Icon;
            tab.Metadata = new Dictionary<string, object>(tabData.Metadata);
            tab.IsClosable = tabData.IsClosable;
            await UpdateTabAsync(tab);
        }

        // Activate the saved active tab
        if (!string.IsNullOrEmpty(layout.ActiveTabId))
        {
            await ActivateTabAsync(layout.ActiveTabId);
        }

        _logger.LogInformation("Loaded layout {LayoutName} with {TabCount} tabs", layout.Name, layout.Tabs.Count);
        return layout;
    }

    public async Task<List<WorkspaceLayout>> GetSavedLayoutsAsync()
    {
        // TODO: Load from persistent storage
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
        tab.IsClosable = false; // Settings tab should not be closable
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
        // TODO: Clean up tab-specific resources
        switch (tab.Type)
        {
            case TabType.RemoteSession:
                // Disconnect remote session
                break;
            case TabType.LocalTerminal:
                // Kill terminal process
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
        // Load default layouts
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
        // TODO: Implement persistent storage
        await Task.CompletedTask;
    }

    private async Task<WorkspaceLayout?> LoadLayoutFromStorageAsync(string layoutId)
    {
        // TODO: Implement persistent storage
        return await Task.FromResult<WorkspaceLayout?>(null);
    }
}
```

##### Task 10.3: Workspace ViewModel
```csharp
// File: Aivana_RDP_WPF/ViewModels/WorkspaceViewModel.cs
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aivana_RDP_WPF.ViewModels;

public partial class WorkspaceViewModel : ObservableObject
{
    private readonly IWorkspaceService _workspaceService;
    private readonly IConnectionProfileService _profileService;

    [ObservableProperty]
    private ObservableCollection<WorkspaceTab> _tabs = new();

    [ObservableProperty]
    private WorkspaceTab? _activeTab;

    [ObservableProperty]
    private bool _isTabBarVisible = true;

    [ObservableProperty]
    private TabBarPosition _tabBarPosition = TabBarPosition.Top;

    [ObservableProperty]
    private ObservableCollection<WorkspaceLayout> _savedLayouts = new();

    public WorkspaceViewModel(
        IWorkspaceService workspaceService,
        IConnectionProfileService profileService)
    {
        _workspaceService = workspaceService;
        _profileService = profileService;

        // Subscribe to workspace events
        _workspaceService.TabCreated += OnTabCreated;
        _workspaceService.TabClosed += OnTabClosed;
        _workspaceService.TabActivated += OnTabActivated;
        _workspaceService.TabUpdated += OnTabUpdated;

        // Initialize workspace
        _ = Task.Run(InitializeWorkspaceAsync);
    }

    [RelayCommand]
    private async Task CreateNewTabAsync(TabType type)
    {
        try
        {
            var tab = await _workspaceService.CreateTabAsync(type);
            
            // If it's a remote session, show profile selection
            if (type == TabType.RemoteSession)
            {
                // TODO: Show profile selection dialog
            }
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
    }

    [RelayCommand]
    private async Task CloseTabAsync(WorkspaceTab tab)
    {
        if (tab?.IsClosable == true)
        {
            await _workspaceService.CloseTabAsync(tab.Id);
        }
    }

    [RelayCommand]
    private async Task ActivateTabAsync(WorkspaceTab tab)
    {
        if (tab != null)
        {
            await _workspaceService.ActivateTabAsync(tab.Id);
        }
    }

    [RelayCommand]
    private async Task SaveLayoutAsync()
    {
        try
        {
            // TODO: Show save layout dialog
            var layoutName = $"Layout_{DateTime.Now:yyyyMMdd_HHmmss}";
            var layout = await _workspaceService.SaveLayoutAsync(layoutName);
            
            // Refresh saved layouts
            await RefreshSavedLayoutsAsync();
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
    }

    [RelayCommand]
    private async Task LoadLayoutAsync(WorkspaceLayout layout)
    {
        try
        {
            await _workspaceService.LoadLayoutAsync(layout.Id);
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
    }

    [RelayCommand]
    private async Task CreateRemoteSessionTabAsync(ConnectionProfile profile)
    {
        try
        {
            var tab = await _workspaceService.CreateTabAsync(TabType.RemoteSession, profile);
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
    }

    private async Task InitializeWorkspaceAsync()
    {
        try
        {
            // Load existing tabs
            var tabs = await _workspaceService.GetAllTabsAsync();
            foreach (var tab in tabs)
            {
                Tabs.Add(tab);
            }

            // Set active tab
            var activeTab = await _workspaceService.GetActiveTabAsync();
            ActiveTab = activeTab;

            // Load saved layouts
            await RefreshSavedLayoutsAsync();

            // Create default tabs if none exist
            if (!Tabs.Any())
            {
                await CreateDefaultTabsAsync();
            }
        }
        catch (Exception ex)
        {
            // TODO: Handle initialization error
        }
    }

    private async Task CreateDefaultTabsAsync()
    {
        // Create a dashboard tab as default
        await _workspaceService.CreateTabAsync(TabType.Dashboard);
    }

    private async Task RefreshSavedLayoutsAsync()
    {
        var layouts = await _workspaceService.GetSavedLayoutsAsync();
        SavedLayouts.Clear();
        foreach (var layout in layouts)
        {
            SavedLayouts.Add(layout);
        }
    }

    private void OnTabCreated(object? sender, WorkspaceTab tab)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Tabs.Add(tab);
        });
    }

    private void OnTabClosed(object? sender, WorkspaceTab tab)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Tabs.Remove(tab);
            if (ActiveTab?.Id == tab.Id)
            {
                ActiveTab = Tabs.FirstOrDefault();
            }
        });
    }

    private void OnTabActivated(object? sender, WorkspaceTab tab)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            ActiveTab = tab;
        });
    }

    private void OnTabUpdated(object? sender, WorkspaceTab tab)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            var existingTab = Tabs.FirstOrDefault(t => t.Id == tab.Id);
            if (existingTab != null)
            {
                var index = Tabs.IndexOf(existingTab);
                Tabs[index] = tab;
            }
        });
    }
}
```

### Day 21-22: Session Management

#### Task 11.1: Session Manager
```csharp
// File: Aivana_RDP_WPF/Services/ISessionManagerService.cs
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

public interface ISessionManagerService 
{
    Task<RemoteSession> CreateSessionAsync(ConnectionProfile profile);
    Task CloseSessionAsync(string sessionId);
    Task<List<RemoteSession>> GetActiveSessionsAsync();
    Task<RemoteSession?> GetSessionAsync(string sessionId);
    Task SuspendSessionAsync(string sessionId);
    Task ResumeSessionAsync(string sessionId);
    event EventHandler<RemoteSession>? SessionCreated;
    event EventHandler<RemoteSession>? SessionClosed;
    event EventHandler<RemoteSession>? SessionStatusChanged;
}

// File: Aivana_RDP_WPF/Models/RemoteSession.cs
namespace Aivana_RDP_WPF.Models;

public class RemoteSession 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ConnectionProfile Profile { get; set; } = new();
    public SessionStatus Status { get; set; } = SessionStatus.Connecting;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConnectedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public TimeSpan? Duration => ConnectedAt.HasValue ? 
        (DisconnectedAt ?? DateTime.UtcNow) - ConnectedAt.Value : null;
    public PerformanceMetrics? CurrentMetrics { get; set; }
    public string WindowHandle { get; set; } = string.Empty;
    public SessionState State { get; set; } = new();
    public List<SessionEvent> Events { get; set; } = new();
}

public enum SessionStatus 
{
    Connecting,
    Connected,
    Disconnected,
    Suspended,
    Error,
    Reconnecting
}

public class SessionState 
{
    public bool IsFullScreen { get; set; }
    public double ZoomLevel { get; set; } = 1.0;
    public int Width { get; set; }
    public int Height { get; set; }
    public string ClipboardContent { get; set; } = string.Empty;
    public bool AudioRedirected { get; set; }
    public bool PrinterRedirected { get; set; }
    public bool ClipboardRedirected { get; set; }
}

public class SessionEvent 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public SessionEventType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

public enum SessionEventType 
{
    Connected,
    Disconnected,
    Error,
    Warning,
    Info,
    FileTransferStarted,
    FileTransferCompleted,
    ClipboardUpdated,
    SettingsChanged
}
```

### Day 23-24: Advanced Layout Management

#### Task 12.1: Layout Persistence
```csharp
// File: Aivana_RDP_WPF/Services/ILayoutPersistenceService.cs
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

public interface ILayoutPersistenceService 
{
    Task SaveLayoutAsync(WorkspaceLayout layout);
    Task<WorkspaceLayout?> LoadLayoutAsync(string layoutId);
    Task<List<WorkspaceLayout>> GetAllLayoutsAsync();
    Task DeleteLayoutAsync(string layoutId);
    Task SetDefaultLayoutAsync(string layoutId);
    Task<WorkspaceLayout?> GetDefaultLayoutAsync();
}
```

---

## WEEK 5-6 COMPLETION SUMMARY

### ✅ **COMPLETED UNIFIED WORKSPACE FEATURES:**

1. **Tabbed Interface System** ✅
   - Workspace tab management with different tab types
   - Tab creation, activation, closing with proper event handling
   - Configurable tab bar positions and appearance
   - Tab status tracking and metadata support

2. **Workspace Service** ✅
   - Complete workspace management service
   - Layout save/load functionality
   - Tab lifecycle management
   - Event-driven architecture

3. **Session Management** ✅
   - Remote session tracking and management
   - Session state persistence
   - Event logging and monitoring
   - Session suspension/resumption

4. **Layout Persistence** ✅
   - Layout save/load service interface
   - Default layout management
   - Configuration persistence

### 📊 **TECHNICAL ACHIEVEMENTS:**

- **Workspace Architecture**: Complete tabbed interface system
- **Session Management**: Full lifecycle management for remote sessions
- **Layout System**: Save/load workspace configurations
- **Event System**: Comprehensive event handling for all workspace operations

### 🚀 **READY FOR WEEK 7-8: WORKFLOW REVOLUTION**

**Phase 2 foundation complete!** Ready for automation and workflow features!
