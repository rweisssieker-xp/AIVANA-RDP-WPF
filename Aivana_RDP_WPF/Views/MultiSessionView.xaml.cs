using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfUserControl = System.Windows.Controls.UserControl;
using WpfButton = System.Windows.Controls.Button;
using Aivana_RDP_WPF.ViewModels.ConnectionManagement;

namespace Aivana_RDP_WPF.Views;

/// <summary>
/// Interaction logic for MultiSessionView.xaml
/// </summary>
public partial class MultiSessionView : WpfUserControl
{
    public MultiSessionView()
    {
        InitializeComponent();
    }

    private void Tab_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is ConnectionSessionViewModel session)
        {
            if (DataContext is ViewModels.MultiSessionViewModel vm)
            {
                vm.SwitchSessionCommand.Execute(session);
            }
        }
    }

    private void CloseTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is WpfButton button && button.Tag is ConnectionSessionViewModel session)
        {
            if (DataContext is ViewModels.MultiSessionViewModel vm)
            {
                vm.CloseSessionCommand.Execute(session);
            }
        }
    }
}

