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
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }

    private void ConnectionItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is ConnectionProfile profile)
        {
            // Handle connection selection
            _viewModel.ConnectionListViewModel.SelectedConnection = profile;
        }
    }
}