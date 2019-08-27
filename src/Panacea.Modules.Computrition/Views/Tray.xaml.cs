using Computrition;
using Panacea.Controls;
using Panacea.Modules.Computrition.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Panacea.Modules.Computrition.Views
{
    /// <summary>
    /// Interaction logic for Tray.xaml
    /// </summary>
    public partial class Tray : UserControl
    {
        public IReadOnlyList<CategoryViewModel> Categories
        {
            get { return (IReadOnlyList<CategoryViewModel>)GetValue(CategoriesProperty); }
            set { SetValue(CategoriesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Categories.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoriesProperty =
            DependencyProperty.Register("Categories", typeof(IReadOnlyList<CategoryViewModel>), typeof(Tray), new PropertyMetadata(null, OnCategoriesChanged));



        public CategoryViewModel SelectedCategory
        {
            get { return (CategoryViewModel)GetValue(SelectedCategoryProperty); }
            set { SetValue(SelectedCategoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedCategory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCategoryProperty =
            DependencyProperty.Register("SelectedCategory", typeof(CategoryViewModel), typeof(Tray), new PropertyMetadata(null));



        private static void OnCategoriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }


        public ICommand AddOneCommand { get; protected set; }

        // Using a DependencyProperty as the backing store for command.  This enables animation, styling, binding, etc...

        public ICommand RemoveOneCommand { get; protected set; }


        public ICommand RemoveCommand { get; protected set; }

        public Tray()
        {
            AddOneCommand = new RelayCommand(args =>
            {
                var recipe = args as Recipe;
                SelectedCategory.Add(recipe);
                (AddOneCommand as RelayCommand)?.OnCanExecuteChanged();
            },
            (args) => SelectedCategory.CanAcceptMore);

            RemoveOneCommand = new RelayCommand(args =>
            {
                var recipe = args as Recipe;
                SelectedCategory.Remove(recipe);
                (RemoveOneCommand as RelayCommand)?.OnCanExecuteChanged();
            });
            RemoveCommand = new RelayCommand(args =>
            {
                var recipe = args as Recipe;
                SelectedCategory.Remove(recipe, recipe.NumOfServings);
                (RemoveCommand as RelayCommand)?.OnCanExecuteChanged();
            });
            InitializeComponent();
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            SelectedCategory = (sender as Expander).Tag as CategoryViewModel;
        }
    }


    class CategoryExpandedConveter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values[0] == values[1];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
