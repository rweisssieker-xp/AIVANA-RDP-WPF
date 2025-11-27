using System.Windows;
using System.Windows.Controls;
using WpfUserControl = System.Windows.Controls.UserControl;
using Aivana_RDP_WPF.ViewModels.ConnectionManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Aivana_RDP_WPF.Views.ConnectionManagement;

/// <summary>
/// Interaction logic for ConnectionListView.xaml
/// </summary>
public partial class ConnectionListView : WpfUserControl
{
    private readonly ConnectionListViewModel? _viewModel;

    public ConnectionListView()
    {
        InitializeComponent();
        Loaded += ConnectionListView_Loaded;
    }

    public ConnectionListView(ConnectionListViewModel viewModel) : this()
    {
        DataContext = viewModel;
        _viewModel = viewModel;
    }

    private void ConnectionListView_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ConnectionListViewModel vm)
        {
            vm.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConnectionListViewModel.IsDialogOpen) && 
            DataContext is ConnectionListViewModel vm)
        {
            if (vm.IsDialogOpen)
            {
                ShowConnectionDialog(vm.SelectedConnection);
            }
        }
    }

    private void ShowConnectionDialog(Models.ConnectionProfile? profile = null)
    {
        if (DataContext is not ConnectionListViewModel vm) return;

        var serviceProvider = Application.Current.Resources["ServiceProvider"] as IServiceProvider;
        if (serviceProvider == null) return;

        var configViewModel = serviceProvider.GetRequiredService<ConnectionConfigViewModel>();
        
        if (profile != null)
        {
            configViewModel.LoadProfile(profile);
        }
        else
        {
            // Reset for new connection
            configViewModel.Name = string.Empty;
            configViewModel.ServerAddress = string.Empty;
            configViewModel.Port = 3389;
            configViewModel.Username = string.Empty;
            configViewModel.Domain = string.Empty;
            configViewModel.IsEditMode = false;
            configViewModel.EditingProfileId = null;
        }

        var dialog = new ConnectionConfigDialog(configViewModel, profile);
        dialog.Owner = Window.GetWindow(this);
        
        if (dialog.ShowDialog() == true)
        {
            // Refresh connection list after save
            vm.LoadConnectionsCommand.ExecuteAsync(null);
        }

        vm.IsDialogOpen = false;
    }
}

