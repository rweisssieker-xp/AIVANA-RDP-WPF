using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Aivana_RDP_WPF.Converters;

/// <summary>
/// Converts a count to visibility based on comparison value.
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            var compareValue = parameter != null ? int.Parse(parameter.ToString()!) : 0;
            return count == compareValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

