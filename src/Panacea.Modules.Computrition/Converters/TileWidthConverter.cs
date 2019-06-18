using System;
using System.Globalization;
using System.Windows.Data;

namespace UserPlugins.Computrition.Converters
{
    public class TileWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double) value/4 - 5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}