using Computrition;
using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.Telephone;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Models;
using Panacea.Modules.Computrition.Views;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(WelcomePage))]
    internal class WelcomePageViewModel : ViewModelBase
    {
        private PanaceaServices _core;
        private ComputritionSettings settings;
        private IComputritionService computrition;
        private string mrn;
        PatronInfo _patronInfo;
        public PatronInfo PatronInfo
        {
            get => _patronInfo;
            set
            {
                _patronInfo = value;

                OnPropertyChanged();
                OnPropertyChanged("GroupedMeals");
                //new Meal().
            }
        }
        public override async void Activate()
        {
            await _core.GetUiManager().DoWhileBusy(async () =>
            {
                PatronInfo = await computrition.GetPatronInfoAsync(
                                                mrn,
                                                settings.PatronInfoParams.ValidMeals,
                                                settings.PatronInfoParams.NumberOfMeals,
                                                settings.PatronInfoParams.NumberOfDays,
                                                settings.PatronInfoParams.SkipCurrentMeal);
            });
            base.Activate();
        }

        public string Name { get; set; }
        public IEnumerable<IGrouping<DateTime, Meal>> GroupedMeals { get => PatronInfo?.Meals.GroupBy(m => m.StartTime.Date).ToList(); }
        public ICommand ViewTrayCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand CallFoodServicesCommand { get; set; }
        public Visibility CallFoodServicesVisibility { get; set; }
        public WelcomePageViewModel(PanaceaServices core, ComputritionSettings settings, IComputritionService computrition, string mrn, string name, PatronInfo patronInfo)
        {
            _core = core;
            this.settings = settings;
            this.computrition = computrition;
            this.mrn = mrn;
            this.Name= name;
            this.PatronInfo = patronInfo;

            if (!string.IsNullOrEmpty(settings.FoodServicesPhone) && _core.TryGetTelephone(out ITelephonePlugin _t))
            {
                CallFoodServicesVisibility = Visibility.Visible;
            }
            else
            {
                CallFoodServicesVisibility = Visibility.Collapsed;
            }
            EditCommand = new RelayCommand((args) =>
            {
                MenuPageViewModel menuPage = new MenuPageViewModel(_core, settings, computrition, mrn, args as Meal);
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.Navigate(menuPage, true);
                }
            });
            ViewTrayCommand = new RelayCommand((args) =>
            {
                var meal = args as Meal;
                ViewTrayPageViewModel viewTrayPage = new ViewTrayPageViewModel(_core, computrition, meal, mrn, settings);
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.Navigate(viewTrayPage, true);
                }

            });
            CallFoodServicesCommand = new RelayCommand((args) =>
            {
                if (_core.TryGetTelephone(out ITelephonePlugin _tel))
                {
                    _tel.Call(settings.FoodServicesPhone);
                }                
            });
        }
    }
}