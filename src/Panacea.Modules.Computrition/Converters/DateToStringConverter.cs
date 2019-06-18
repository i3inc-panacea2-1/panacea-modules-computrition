using System;
using System.Globalization;
using System.Windows.Data;

namespace Panacea.Modules.Computrition.Converters
{
    public class DateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime)value;
            if (date.Date == DateTime.Now.Date)
            {
                return "Today";
            }
            if (date.Date == DateTime.Now.AddDays(1).Date)
            {
                return "Tomorrow";
            }
            return date.ToString("MM/dd/yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
