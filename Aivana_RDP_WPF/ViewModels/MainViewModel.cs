using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.ViewModels.ConnectionManagement;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.ViewModels;

/// <summary>
/// Main ViewModel for the application.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ILogger<MainViewModel> _logger;
    private readonly ConnectionListViewModel _connectionListViewModel;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private object? _currentContent;

    [ObservableProperty]
    private ConnectionSessionViewModel? _activeSession;

    [ObservableProperty]
    private bool _useMultiSessionMode;

    public ConnectionListViewModel ConnectionListViewModel => _connectionListViewModel;

    public MainViewModel(
        ConnectionListViewModel connectionListViewModel,
        IServiceProvider serviceProvider,
        ILogger<MainViewModel> logger)
    {
        _connectionListViewModel = connectionListViewModel;
        _serviceProvider = serviceProvider;
        _logger = logger;
        CurrentContent = _connectionListViewModel;
        
        // Subscribe to connection selection changes
        _connectionListViewModel.PropertyChanged += ConnectionListViewModel_PropertyChanged;
    }

    private void ConnectionListViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConnectionListViewModel.SelectedConnection))
        {
            OnConnectionSelected(_connectionListViewModel.SelectedConnection);
        }
    }

    private void OnConnectionSelected(ConnectionProfile? profile)
    {
        if (profile == null)
        {
            // Show connection list when no connection selected
            CurrentContent = _connectionListViewModel;
            ActiveSession = null;
            return;
        }

        // Create or switch to connection session view
        try
        {
            var sessionViewModel = _serviceProvider.GetRequiredService<ConnectionSessionViewModel>();
            sessionViewModel.LoadProfile(profile);
            ActiveSession = sessionViewModel;
            CurrentContent = sessionViewModel;
            _logger.LogInformation("Switched to connection session for profile {ProfileId}", profile.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating connection session view for profile {ProfileId}", profile.Id);
        }
    }

    [RelayCommand]
    private void ShowConnectionList()
    {
        CurrentContent = _connectionListViewModel;
        ActiveSession = null;
        _connectionListViewModel.SelectedConnection = null;
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing MainViewModel");
        await _connectionListViewModel.LoadConnectionsCommand.ExecuteAsync(null);
    }
}

