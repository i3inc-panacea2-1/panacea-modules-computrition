using Computrition;
using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.Telephone;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Models;
using Panacea.Modules.Computrition.Views;
using Panacea.Multilinguality;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(WelcomePage))]
    internal class WelcomePageViewModel : ViewModelBase
    {
        private PanaceaServices _core;

        MenuViewModel _menu;
        public MenuViewModel Menu
        {
            get => _menu; set
            {
                _menu = value;
                OnPropertyChanged();
            }
        }

        public string Name { get; set; }
        public ICommand ViewTrayCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand CallFoodServicesCommand { get; set; }
        public Visibility CallFoodServicesVisibility { get; set; }

        public WelcomePageViewModel(
            PanaceaServices core,
            MenuViewModel menu)
        {
            _core = core;
            Menu = menu;
            Name = Menu.PatronInfo.FirstName + " " + Menu.PatronInfo.LastName;

            CallFoodServicesVisibility = Menu.CanCallfoodServices ? Visibility.Visible : Visibility.Collapsed;

            EditCommand = new RelayCommand(async (args) =>
            {
                if (!core.TryGetUiManager(out IUiManager ui)) return;
                var meal = args as Meal;
                await ui.DoWhileBusy(async () =>
                {
                    try
                    {
                        await Menu.SetSelectedMealAndMonitorAsync(meal);
                        var menuPage = new MenuPageViewModel(_core, Menu);
                        if (_core.TryGetUiManager(out IUiManager _ui))
                        {
                            _ui.Navigate(menuPage, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        _core.Logger.Error(this, ex.Message);
                        ui.Toast(new Translator("Computrition").Translate("An error occured. Please, try again later"));
                        ui.GoBack();
                        return;
                    }
                });

            });
            ViewTrayCommand = new RelayCommand(async (args) =>
            {
                try
                {
                    var meal = args as Meal;
                    if (_core.TryGetUiManager(out IUiManager ui))
                    {
                        await ui.DoWhileBusy(async () =>
                        {
                            await menu.SetSelectedMealAsync(meal);
                            var viewTrayPage = new ViewTrayPageViewModel(menu.SelectedMeal);
                            if (_core.TryGetUiManager(out IUiManager _ui))
                            {
                                _ui.Navigate(viewTrayPage, true);
                            }
                        });

                    }
                }
                catch (Exception ex)
                {
                    _core.Logger.Error(this, ex.Message);
                    if (_core.TryGetUiManager(out IUiManager ui))
                    {
                        ui.Toast(new Translator("Computrition").Translate("An error occured"));
                    }
                }

            });
            CallFoodServicesCommand = new RelayCommand((args) =>
            {
                Menu.CallFoodServices();
            });
        }

        public override async void Activate()
        {
            try
            {
                if (_core.TryGetUiManager(out IUiManager u))
                {
                    await u.DoWhileBusy(async () =>
                    {
                        await Menu.GetMealsAsync();
                    });

                }
            }
            catch (Exception ex)
            {
                _core.Logger.Error(this, ex.Message);
                if (_core.TryGetUiManager(out IUiManager ui))
                {
                    ui.GoBack();
                }
            }

        }
    }
}