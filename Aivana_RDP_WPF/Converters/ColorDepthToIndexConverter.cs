using System.Globalization;
using System.Windows.Data;

namespace Aivana_RDP_WPF.Converters;

/// <summary>
/// Converts color depth value to ComboBox index.
/// </summary>
public class ColorDepthToIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int depth)
        {
            return depth switch
            {
                16 => 0,
                24 => 1,
                32 => 2,
                _ => 2
            };
        }
        return 2;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int index)
        {
            return index switch
            {
                0 => 16,
                1 => 24,
                2 => 32,
                _ => 32
            };
        }
        return 32;
    }
}

