﻿using Computrition;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Panacea.Modules.Computrition.Converters
{
    public class MealClosedToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var meal = value as Meal;
            if (meal.EndTime <= DateTime.Now.AddMinutes(5) && meal.HasOrdered)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
