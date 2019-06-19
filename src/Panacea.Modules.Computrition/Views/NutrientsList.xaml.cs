using Computrition;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for NutrientsList.xaml
    /// </summary>
    public partial class NutrientsList : UserControl
    {
        public List<Nutrient> Nutrients
        {
            get { return (List<Nutrient>)GetValue(NutrientsProperty); }
            set { SetValue(NutrientsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Nutrients.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NutrientsProperty =
            DependencyProperty.Register("Nutrients", typeof(List<Nutrient>), typeof(NutrientsList), new PropertyMetadata(null));

        public NutrientsList()
        {
            InitializeComponent();
        }
    }
}
