using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace UserPlugins.Computrition.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value
                ? new BrushConverter().ConvertFrom("#0096FF")
                : new SolidColorBrush(Colors.Transparent);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}