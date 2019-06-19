using Computrition;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Panacea.Modules.Computrition.Converters
{
    public class EditSelectionsButtonVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var order = values[0] as ObservableCollection<Recipe>;
            var category = values[1] as Category;
            if (category.Recipes.Any(r => order.Any(r2 => r2.Id == r.Id)))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
