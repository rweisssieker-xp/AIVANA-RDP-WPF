using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.ViewModels.ConnectionManagement;

namespace Aivana_RDP_WPF.ViewModels;

/// <summary>
/// Main ViewModel for the application.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ILogger<MainViewModel> _logger;
    private readonly ConnectionListViewModel _connectionListViewModel;

    [ObservableProperty]
    private object? _currentContent;

    public ConnectionListViewModel ConnectionListViewModel => _connectionListViewModel;

    public MainViewModel(
        ConnectionListViewModel connectionListViewModel,
        ILogger<MainViewModel> logger)
    {
        _connectionListViewModel = connectionListViewModel;
        _logger = logger;
        CurrentContent = _connectionListViewModel;
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing MainViewModel");
        await _connectionListViewModel.LoadConnectionsCommand.ExecuteAsync(null);
    }
}

