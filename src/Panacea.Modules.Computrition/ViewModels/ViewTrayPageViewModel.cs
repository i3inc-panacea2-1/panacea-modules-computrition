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
        private PanaceaServices _core;
        private IComputritionService computrition;
        private Meal meal;
        private string mrn;
        private ComputritionSettings settings;
        public string Title { get; set; }

        ObservableCollection<Recipe> _selected;
        public ObservableCollection<Recipe> SelectedRecipes
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged();
            }
        }
        public override async void Activate()
        {   
            if (_core.TryGetUiManager(out IUiManager _ui))
            {
                await _ui.DoWhileBusy(async () =>
                {
                    var menu = await computrition.GetPatronMenuAsync(mrn, meal.Id, meal.Date, settings.PatronMenuParams.Nutrients, settings.PatronMenuParams.RoundingMethod);

                    SelectedRecipes = new ObservableCollection<Recipe>(menu.Categories.SelectMany(c => c.Recipes).Where(r => r.NumOfServings > 0).ToList());

                });
            }
            base.Activate();
        }
        public ViewTrayPageViewModel(PanaceaServices core, IComputritionService computrition, Meal meal, string mrn, ComputritionSettings settings)
        {
            _core = core;
            this.computrition = computrition;
            this.meal = meal;
            this.mrn = mrn;
            this.settings = settings;
            Title = meal.Name;
        }
    }
}