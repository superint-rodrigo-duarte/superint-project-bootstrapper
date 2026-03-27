using System.Globalization;
using Avalonia.Data.Converters;

namespace superint.ProjectBootstrapper.UI.Converters;

/// <summary>
/// Converters genéricos para comparações de igualdade.
/// </summary>
public static class EqualityConverters
{
    /// <summary>
    /// Converter genérico que compara o valor com o ConverterParameter.
    /// Uso: Converter={x:Static local:EqualityConverters.IsEqual}, ConverterParameter=valor
    /// </summary>
    public static readonly IValueConverter IsEqual = new EqualsConverter();
}

/// <summary>
/// Converter que compara o valor de binding com o ConverterParameter.
/// Suporta comparação case-insensitive para strings.
/// </summary>
public class EqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null && parameter == null)
            return true;

        if (value == null || parameter == null)
            return false;

        // Comparação case-insensitive para strings
        if (value is string stringValue && parameter is string stringParameter)
        {
            return string.Equals(stringValue, stringParameter, StringComparison.OrdinalIgnoreCase);
        }

        return Equals(value, parameter);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
