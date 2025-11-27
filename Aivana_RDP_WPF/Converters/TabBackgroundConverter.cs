using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WpfMediaColor = System.Windows.Media.Color;
using WpfSolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace Aivana_RDP_WPF.Converters;

/// <summary>
/// Converts boolean to background color for tab selection.
/// </summary>
public class TabBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected ? new WpfSolidColorBrush(WpfMediaColor.FromRgb(255, 255, 255)) : new WpfSolidColorBrush(WpfMediaColor.FromRgb(240, 240, 240));
        }
        return new WpfSolidColorBrush(WpfMediaColor.FromRgb(240, 240, 240));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}

