using System.Windows;
using System.Windows.Controls;
using WpfMessageBox = System.Windows.MessageBox;
using Aivana_RDP_WPF.ViewModels.ConnectionManagement;

namespace Aivana_RDP_WPF.Views.ConnectionManagement;

/// <summary>
/// Interaction logic for ConnectionConfigDialog.xaml
/// </summary>
public partial class ConnectionConfigDialog : Window
{
    private readonly ConnectionConfigViewModel _viewModel;

    public ConnectionConfigDialog(ConnectionConfigViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
    }

    public ConnectionConfigDialog(ConnectionConfigViewModel viewModel, Models.ConnectionProfile? profile) : this(viewModel)
    {
        if (profile != null)
        {
            _viewModel.LoadProfile(profile);
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.Password = passwordBox.Password;
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Validate())
        {
            try
            {
                await _viewModel.SaveCommand.ExecuteAsync(null);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Error saving connection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            WpfMessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

