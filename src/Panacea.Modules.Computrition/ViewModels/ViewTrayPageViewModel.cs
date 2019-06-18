using Computrition;
using Panacea.Core;
using Panacea.Modules.Computrition.Models;
using Panacea.Modules.Computrition.Views;
using Panacea.Mvvm;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(ViewTrayPage))]
    internal class ViewTrayPageViewModel : ViewModelBase
    {
        private PanaceaServices _core;
        private IComputritionService computrition;
        private Meal meal;
        private string mrn;
        private ComputritionSettings settings;

        public ViewTrayPageViewModel(PanaceaServices core, IComputritionService computrition, Meal meal, string mrn, ComputritionSettings settings)
        {
            _core = core;
            this.computrition = computrition;
            this.meal = meal;
            this.mrn = mrn;
            this.settings = settings;
        }
    }
}