using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace RNGExperiments;

public class NumberConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType.IsAssignableTo(typeof(string))) {
            return value?.ToString();
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue && 
            targetType.IsAssignableTo(typeof(int)) &&
            int.TryParse(stringValue, out int result)) {
            return result;
        }

        return null;
    }
}