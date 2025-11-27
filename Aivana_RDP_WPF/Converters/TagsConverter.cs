using System.Globalization;
using System.Windows.Data;
using Aivana_RDP_WPF.Helpers;

namespace Aivana_RDP_WPF.Converters;

/// <summary>
/// Converts tags JSON string to List of strings for binding.
/// </summary>
public class TagsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string tagsJson)
        {
            return TagHelper.ParseTags(tagsJson);
        }
        return new List<string>();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is List<string> tags)
        {
            return TagHelper.SerializeTags(tags);
        }
        return "[]";
    }
}

