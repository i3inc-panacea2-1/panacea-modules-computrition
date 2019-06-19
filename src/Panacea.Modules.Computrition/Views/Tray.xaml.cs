using Computrition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<Recipe> SelectedRecipes
        {
            get { return (ObservableCollection<Recipe>)GetValue(SelectedRecipesProperty); }
            set { SetValue(SelectedRecipesProperty, value); }
        }
        // Using a DependencyProperty as the backing store for SelectedRecipes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedRecipesProperty =
            DependencyProperty.Register("SelectedRecipes", typeof(ObservableCollection<Recipe>), typeof(Tray), new PropertyMetadata(null));
        public bool Completed
        {
            get { return (bool)GetValue(CompletedProperty); }
            set { SetValue(CompletedProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Completed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CompletedProperty =
            DependencyProperty.Register("Completed", typeof(bool), typeof(Tray), new PropertyMetadata(false));
        public int MaxSelections
        {
            get { return (int)GetValue(MaxSelectionsProperty); }
            set { SetValue(MaxSelectionsProperty, value); }
        }
        // Using a DependencyProperty as the backing store for MaxSelections.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxSelectionsProperty =
            DependencyProperty.Register("MaxSelections", typeof(int), typeof(Tray), new PropertyMetadata(0, OnMaxSelectionsChanged));
        private static void OnMaxSelectionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tray = d as Tray;
            tray.SelectionsVisibility = (int)e.NewValue > 0;
        }
        public int SelectionCount
        {
            get { return (int)GetValue(SelectionCountProperty); }
            set { SetValue(SelectionCountProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Selectioncount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionCountProperty =
            DependencyProperty.Register("SelectionCount", typeof(int), typeof(Tray), new PropertyMetadata(0));
        public bool SelectionsVisibility
        {
            get { return (bool)GetValue(SelectionsVisibilityProperty); }
            set { SetValue(SelectionsVisibilityProperty, value); }
        }
        // Using a DependencyProperty as the backing store for SelectionsVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionsVisibilityProperty =
            DependencyProperty.Register("SelectionsVisibility", typeof(bool), typeof(Tray), new PropertyMetadata(false));
        public ICommand AddOneCommand
        {
            get { return (ICommand)GetValue(AddOneCommandProperty); }
            set { SetValue(AddOneCommandProperty, value); }
        }
        // Using a DependencyProperty as the backing store for command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddOneCommandProperty =
            DependencyProperty.Register("AddOneCommand", typeof(ICommand), typeof(Tray), new PropertyMetadata(null));
        public ICommand RemoveOneCommand
        {
            get { return (ICommand)GetValue(RemoveOneCommandProperty); }
            set { SetValue(RemoveOneCommandProperty, value); }
        }
        // Using a DependencyProperty as the backing store for RemoveOneCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RemoveOneCommandProperty =
            DependencyProperty.Register("RemoveOneCommand", typeof(ICommand), typeof(Tray), new PropertyMetadata(null));
        public ICommand RemoveCommand
        {
            get { return (ICommand)GetValue(RemoveCommandProperty); }
            set { SetValue(RemoveCommandProperty, value); }
        }
        // Using a DependencyProperty as the backing store for RemoveOneCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RemoveCommandProperty =
            DependencyProperty.Register("RemoveCommand", typeof(ICommand), typeof(Tray), new PropertyMetadata(null));
        public Tray()
        {
            InitializeComponent();
        }

    }
}
