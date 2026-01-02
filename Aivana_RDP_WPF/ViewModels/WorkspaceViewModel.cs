using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Aivana_RDP_WPF.ViewModels;

/// <summary>
/// ViewModel for workspace management
/// </summary>
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

        _workspaceService.TabCreated += OnTabCreated;
        _workspaceService.TabClosed += OnTabClosed;
        _workspaceService.TabActivated += OnTabActivated;
        _workspaceService.TabUpdated += OnTabUpdated;

        _ = Task.Run(InitializeWorkspaceAsync);
    }

    [RelayCommand]
    private async Task CreateNewTabAsync(TabType type)
    {
        try
        {
            var tab = await _workspaceService.CreateTabAsync(type);
            
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
            var layoutName = $"Layout_{DateTime.Now:yyyyMMdd_HHmmss}";
            var layout = await _workspaceService.SaveLayoutAsync(layoutName);
            
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
            var tabs = await _workspaceService.GetAllTabsAsync();
            foreach (var tab in tabs)
            {
                Tabs.Add(tab);
            }

            var activeTab = await _workspaceService.GetActiveTabAsync();
            ActiveTab = activeTab;

            await RefreshSavedLayoutsAsync();

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
