using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;

namespace Aivana_RDP_WPF.ViewModels.ConnectionManagement;

/// <summary>
/// ViewModel for connection list management.
/// </summary>
public partial class ConnectionListViewModel : ObservableObject
{
    private readonly IConnectionProfileService _connectionProfileService;
    private readonly IImportExportService? _importExportService;
    private readonly ILogger<ConnectionListViewModel> _logger;

    [ObservableProperty]
    private List<ConnectionProfile> _connections = new();

    [ObservableProperty]
    private ConnectionProfile? _selectedConnection;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDialogOpen;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _selectedGroup;

    [ObservableProperty]
    private string? _selectedTag;

    [ObservableProperty]
    private bool _showFavoritesOnly;

    [ObservableProperty]
    private List<string> _availableGroups = new();

    [ObservableProperty]
    private List<string> _availableTags = new();

    public ConnectionListViewModel(
        IConnectionProfileService connectionProfileService,
        ILogger<ConnectionListViewModel> logger,
        IImportExportService? importExportService = null)
    {
        _connectionProfileService = connectionProfileService;
        _importExportService = importExportService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadConnectionsAsync()
    {
        IsLoading = true;
        try
        {
            _logger.LogInformation("Loading connection profiles");
            var profiles = await _connectionProfileService.GetAllProfilesAsync();
            Connections = profiles.ToList();
            
            // Load groups and tags
            AvailableGroups = await _connectionProfileService.GetAllGroupsAsync();
            AvailableTags = await _connectionProfileService.GetAllTagsAsync();
            
            OnPropertyChanged(nameof(FilteredConnections));
            _logger.LogInformation("Loaded {Count} connection profiles", Connections.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading connection profiles");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(ConnectionProfile? profile)
    {
        if (profile == null) return;

        try
        {
            await _connectionProfileService.ToggleFavoriteAsync(profile.Id);
            await LoadConnectionsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite for profile {ProfileId}", profile.Id);
        }
    }

    [RelayCommand]
    private void FilterByGroup(string? groupName)
    {
        SelectedGroup = groupName;
        SelectedTag = null;
        OnPropertyChanged(nameof(FilteredConnections));
    }

    [RelayCommand]
    private void FilterByTag(string? tag)
    {
        SelectedTag = tag;
        SelectedGroup = null;
        OnPropertyChanged(nameof(FilteredConnections));
    }

    [RelayCommand]
    private void ToggleFavoritesFilter()
    {
        ShowFavoritesOnly = !ShowFavoritesOnly;
        OnPropertyChanged(nameof(FilteredConnections));
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedGroup = null;
        SelectedTag = null;
        ShowFavoritesOnly = false;
        SearchText = string.Empty;
        OnPropertyChanged(nameof(FilteredConnections));
    }

    [RelayCommand]
    private async Task DeleteConnectionAsync(ConnectionProfile? profile)
    {
        if (profile == null) return;

        try
        {
            _logger.LogInformation("Deleting connection profile {ProfileId}", profile.Id);
            await _connectionProfileService.DeleteProfileAsync(profile.Id);
            await LoadConnectionsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting connection profile {ProfileId}", profile.Id);
        }
    }

    [RelayCommand]
    private void CreateConnection()
    {
        _logger.LogInformation("Create connection requested");
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void EditConnection(ConnectionProfile? profile)
    {
        if (profile == null) return;
        _logger.LogInformation("Edit connection {ProfileId} requested", profile.Id);
        SelectedConnection = profile;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private async Task ImportConnectionsAsync()
    {
        if (_importExportService == null)
        {
            _logger.LogWarning("ImportExportService not available");
            return;
        }

        try
        {
            var dialog = new WpfOpenFileDialog
            {
                Filter = "RDP Files (*.rdp)|*.rdp|JSON Files (*.json)|*.json|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Import Connection Profiles"
            };

            if (dialog.ShowDialog() == true)
            {
                IEnumerable<ConnectionProfile> profiles;
                var extension = System.IO.Path.GetExtension(dialog.FileName).ToLowerInvariant();
                
                if (string.Equals(extension, ".rdp", StringComparison.OrdinalIgnoreCase))
                {
                    profiles = await _importExportService.ImportFromRdpFileAsync(dialog.FileName);
                }
                else if (string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
                {
                    profiles = await _importExportService.ImportFromJsonFileAsync(dialog.FileName);
                }
                else if (string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
                {
                    profiles = await _importExportService.ImportFromCsvFileAsync(dialog.FileName);
                }
                else
                {
                    _logger.LogWarning("Unsupported file format: {Extension}", extension);
                    return;
                }

                foreach (var profile in profiles)
                {
                    await _connectionProfileService.CreateProfileAsync(profile);
                }

                await LoadConnectionsAsync();
                _logger.LogInformation("Imported {Count} connection profiles", profiles.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing connections");
        }
    }

    [RelayCommand]
    private async Task ExportConnectionsAsync()
    {
        if (_importExportService == null || !Connections.Any())
        {
            _logger.LogWarning("ImportExportService not available or no connections to export");
            return;
        }

        try
        {
            var dialog = new WpfSaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|CSV Files (*.csv)|*.csv",
                Title = "Export Connection Profiles",
                FileName = "connections.json"
            };

            if (dialog.ShowDialog() == true)
            {
                var extension = System.IO.Path.GetExtension(dialog.FileName).ToLowerInvariant();
                
                if (string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
                {
                    await _importExportService.ExportToJsonFileAsync(Connections, dialog.FileName);
                }
                else if (string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
                {
                    await _importExportService.ExportToCsvFileAsync(Connections, dialog.FileName);
                }

                _logger.LogInformation("Exported {Count} connection profiles to {FilePath}", Connections.Count, dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting connections");
        }
    }

    public IEnumerable<ConnectionProfile> FilteredConnections
    {
        get
        {
            var filtered = Connections.AsEnumerable();

            // Filter by favorites
            if (ShowFavoritesOnly)
            {
                filtered = filtered.Where(c => c.IsFavorite);
            }

            // Filter by group
            if (!string.IsNullOrEmpty(SelectedGroup))
            {
                filtered = filtered.Where(c => c.GroupName == SelectedGroup);
            }

            // Filter by tag
            if (!string.IsNullOrEmpty(SelectedTag))
            {
                filtered = filtered.Where(c => Helpers.TagHelper.HasTag(c.Tags, SelectedTag));
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLowerInvariant();
                filtered = filtered.Where(c =>
                    c.Name.ToLowerInvariant().Contains(searchLower) ||
                    c.ServerAddress.ToLowerInvariant().Contains(searchLower) ||
                    (c.Username?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (c.GroupName?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    Helpers.TagHelper.ParseTags(c.Tags).Any(t => t.ToLowerInvariant().Contains(searchLower)));
            }

            return filtered.OrderBy(c => c.IsFavorite ? 0 : 1).ThenBy(c => c.Name);
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredConnections));
    }
}
