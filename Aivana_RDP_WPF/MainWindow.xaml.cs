using System.Windows;
using System.Windows.Input;
using Aivana_RDP_WPF.ViewModels;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    public MainWindow(MainViewModel viewModel) : this()
    {
        DataContext = viewModel;
        _viewModel = viewModel;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            _viewModel = vm;
            await vm.InitializeCommand.ExecuteAsync(null);
        }
    }

    private void ConnectionItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel != null && sender is FrameworkElement element && element.DataContext is ConnectionProfile profile)
        {
            // Handle connection selection
            _viewModel.ConnectionListViewModel.SelectedConnection = profile;
        }
    }

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null && sender is Button button && button.Tag is ConnectionProfile profile)
        {
            // Open connection session - this will trigger view switch via MainViewModel
            _viewModel.ConnectionListViewModel.SelectedConnection = profile;
        }
    }
}