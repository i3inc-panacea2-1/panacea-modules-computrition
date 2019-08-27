using Computrition;
using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Models;
using Panacea.Modules.Computrition.Views;
using Panacea.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(ViewTrayPage))]
    public class ViewTrayPageViewModel : ViewModelBase
    {
        public MealViewModel Meal { get; set; }
        public ViewTrayPageViewModel(MealViewModel meal)
        {
            Meal = meal;
        }
    }
}