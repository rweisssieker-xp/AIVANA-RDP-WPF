using System.Windows.Controls;
using WpfUserControl = System.Windows.Controls.UserControl;
using Aivana_RDP_WPF.ViewModels.ConnectionManagement;

namespace Aivana_RDP_WPF.Views.ConnectionManagement;

/// <summary>
/// Interaction logic for ConnectionListView.xaml
/// </summary>
public partial class ConnectionListView : WpfUserControl
{
    public ConnectionListView()
    {
        InitializeComponent();
    }

    public ConnectionListView(ConnectionListViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}

