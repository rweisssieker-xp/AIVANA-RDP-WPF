using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Aivana_RDP_WPF.Converters;

/// <summary>
/// Converts boolean to font weight for tab selection.
/// </summary>
public class TabFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected ? FontWeights.SemiBold : FontWeights.Normal;
        }
        return FontWeights.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}

