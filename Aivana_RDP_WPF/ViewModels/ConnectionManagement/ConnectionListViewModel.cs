using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;
using Aivana_RDP_WPF.ViewModels.ConnectionManagement;

namespace Aivana_RDP_WPF.ViewModels.ConnectionManagement;

/// <summary>
/// ViewModel for connection list management.
/// </summary>
public partial class ConnectionListViewModel : ObservableObject
{
    private readonly IConnectionProfileService _connectionProfileService;
    private readonly ILogger<ConnectionListViewModel> _logger;

    [ObservableProperty]
    private List<ConnectionProfile> _connections = new();

    [ObservableProperty]
    private ConnectionProfile? _selectedConnection;

    [ObservableProperty]
    private bool _isLoading;

    public ConnectionListViewModel(
        IConnectionProfileService connectionProfileService,
        ILogger<ConnectionListViewModel> logger)
    {
        _connectionProfileService = connectionProfileService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadConnectionsAsync()
    {
        IsLoading = true;
        try
        {
            _logger.LogInformation("Loading connection profiles");
            Connections = await _connectionProfileService.GetAllProfilesAsync();
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

    [ObservableProperty]
    private bool _isDialogOpen;

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
}

