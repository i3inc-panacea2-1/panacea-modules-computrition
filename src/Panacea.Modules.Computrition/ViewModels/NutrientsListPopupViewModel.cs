using System.Collections.Generic;
using Computrition;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Views;
using Panacea.Mvvm;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(NutrientsListPopup))]
    public class NutrientsListPopupViewModel : PopupViewModelBase<object>
    {
        public List<Nutrient> Nutrients { get; set; }
        public NutrientsListPopupViewModel(List<Nutrient> nutrients)
        {
            this.Nutrients = nutrients;
        }
    }
}