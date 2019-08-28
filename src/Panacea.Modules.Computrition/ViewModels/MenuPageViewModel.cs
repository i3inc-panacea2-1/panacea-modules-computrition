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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(MenuPage))]
    internal class MenuPageViewModel : ViewModelBase
    {
        private PanaceaServices _core;
        private readonly MenuViewModel _menu;
        public MenuViewModel Menu
        {
            get => _menu;
        }

        public Order Order { get; set; }

        public ICommand CompleteOrderCommand { get; set; }

        public ICommand EditMenuCommand { get; set; }

        public ICommand CallFoodServicesCommand { get; set; }

        public ICommand NoMealCommand { get; set; }

        public ICommand ShowNutrientsCommand { get; set; }

        public Visibility CallFoodServicesVisibility { get; set; }

        Visibility _nomealRequired = Visibility.Collapsed;
        public Visibility NoMealRequiredVisibility
        {
            get => _nomealRequired;
            set
            {
                _nomealRequired = value;
                OnPropertyChanged();
            }
        }

        public override void Activate()
        {
            try
            {
                NoMealRequiredVisibility = _menu.SelectedMeal.AllowsNoMeal ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                _core.Logger.Error(this, ex.Message);
                if (_core.TryGetUiManager(out IUiManager _uiManager))
                {
                    _uiManager.Toast(new Translator("Computrition").Translate("An error occured. Please, try again later"));
                    _uiManager.GoBack();
                }
                else
                {
                    _core.Logger.Error(this, "An error occured during computrition menu edit!");
                    _core.Logger.Error(this, "ui manager not loaded");
                }
            }
            base.Activate();
        }

        public MenuPageViewModel(PanaceaServices core, MenuViewModel menu)
        {
            _core = core;
            _menu = menu;
            CallFoodServicesVisibility = _menu.CanCallfoodServices ? Visibility.Visible : Visibility.Collapsed;
            EditMenuCommand = new RelayCommand((args) =>
            {
                _menu.SelectedMeal.SelectedCategory = args as CategoryViewModel;
                EditMenuViewModel editMenu = new EditMenuViewModel(_core, _menu);
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.Navigate(editMenu, false);
                }
                else
                {
                    _core.Logger.Error(this, "ui manager not loaded");
                }
            });
            CompleteOrderCommand = new RelayCommand(async (args) =>
            {

                if (await menu.SubmitMenuAsync())
                {
                    if (_core.TryGetUiManager(out IUiManager ui))
                    {
                        ui.GoBack();
                    }
                }
            },
            (args) =>
            {
                return true;
            });

            CallFoodServicesCommand = new RelayCommand((args) =>
            {
                _menu.CallFoodServices();
            });

            NoMealCommand = new RelayCommand(async (args) =>
            {
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    if (await menu.NoMealAsync())
                    {
                        _ui.GoBack(2);
                    }
                }
                else
                {
                    _core.Logger.Error(this, "ui manager not loaded");
                }
            });

            ShowNutrientsCommand = new RelayCommand(async (args) =>
            {
                if (_core.TryGetUiManager(out IUiManager ui))
                {
                    await ui.DoWhileBusy(async () =>
                    {
                        try
                        {
                            var nutrients = await menu.GetNutrientSummaryAsync(menu.SelectedMeal.SelectedRecipes.ToList());
                            await ui.ShowPopup(new NutrientsListPopupViewModel(nutrients));
                        }
                        catch (Exception ex)
                        {
                            ui.Toast(new Translator("Computrition").Translate("An error occured. Please, try again later"));
                            _core.Logger.Error(this, ex.Message);
                        }
                    });
                }
                else
                {
                    _core.Logger.Error(this, "ui manager not loaded");
                }
            },
            args=> menu.SelectedMeal.SelectedRecipes.Any());
        }
    }

}