using Computrition;
using Panacea.Core;
using Panacea.Modules.Computrition.Models;
using Panacea.Modules.Computrition.Views;
using Panacea.Mvvm;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(MenuPage))]
    internal class MenuPageViewModel : ViewModelBase
    {
        private PanaceaServices _core;
        private ComputritionSettings settings;
        private IComputritionService computrition;
        private string mrn;
        private Meal meal;

        public MenuPageViewModel(PanaceaServices core, ComputritionSettings settings, IComputritionService computrition, string mrn, Meal meal)
        {
            _core = core;
            this.settings = settings;
            this.computrition = computrition;
            this.mrn = mrn;
            this.meal = meal;
        }
    }
}