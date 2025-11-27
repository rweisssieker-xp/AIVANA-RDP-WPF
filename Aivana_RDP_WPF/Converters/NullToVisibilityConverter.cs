using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Aivana_RDP_WPF.Converters;

/// <summary>
/// Converts null to Visibility.Collapsed, non-null to Visibility.Visible.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

