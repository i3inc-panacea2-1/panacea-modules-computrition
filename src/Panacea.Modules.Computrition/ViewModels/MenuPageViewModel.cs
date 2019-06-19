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
        private ComputritionSettings settings;
        private IComputritionService _computrition;
        private string mrn;
        private Meal meal;
        PatronMenu _menu;
        ObservableCollection<Recipe> _selectedRecipes = new ObservableCollection<Recipe>();
        public ObservableCollection<Recipe> SelectedRecipes
        {
            get => _selectedRecipes;
            set
            {
                _selectedRecipes = value;
                OnPropertyChanged();
            }
        }


        public Order Order { get; set; }

        List<Category> _categories;
        public List<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        bool _hasChanges;
        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                _hasChanges = value;
                OnPropertyChanged();
            }

        }

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
        bool activatedOnce = false;
        List<Recipe> originalRecipes = null;
        public override async void Activate()
        {
            if (!activatedOnce)
            {
                if (_core.TryGetUiManager(out IUiManager _ui)){
                    await _ui.DoWhileBusy(async () => {
                        try
                        {
                            _menu = await _computrition.GetPatronMenuAsync(mrn, meal.Id, meal.Date, settings.PatronMenuParams.Nutrients, settings.PatronMenuParams.RoundingMethod);
                            Categories = _menu.Categories.Where(c => c.Name.ToLower() != "meal not required").ToList();
                            originalRecipes = _menu.Categories.SelectMany(c => c.Recipes).ToList();
                            _menu.Categories.ForEach(c => c.Recipes = c.Recipes.Select(r => RecipeModel.FromRecipe(r)).Cast<Recipe>().ToList());
                            activatedOnce = true;
                        }
                        catch (Exception ex)
                        {
                            _ui.Toast(new Translator("Computrition").Translate("An error occured. Please, try again later"));
                            _ui.GoBack();
                            return;
                        }
                    });
                }
                else
                {
                    _core.Logger.Error(this, "An error occured during computrition menu edit!");
                    _core.Logger.Error(this, "ui manager not loaded");
                }
            }
            try
            {
                Categories = new List<Category>(Categories);
                if (SelectedRecipes.Count == 0) //SUPPOSED TO BE THE FIRST ENTRY!
                {
                    HasChanges = false;
                    HasChanges = !SelectedRecipes.All(r => originalRecipes.FirstOrDefault(rec => rec.Id == r.Id)?.NumOfServings == r.NumOfServings);
                    SelectedRecipes = new ObservableCollection<Recipe>(_menu.Categories.SelectMany(c => c.Recipes).Where(r => r.NumOfServings > 0).ToList());
                }
                else
                {
                    HasChanges = !SelectedRecipes.All(r => originalRecipes.FirstOrDefault(rec => rec.Id == r.Id)?.NumOfServings == r.NumOfServings);
                    SelectedRecipes = new ObservableCollection<Recipe>(SelectedRecipes.Where(r => r.NumOfServings > 0));
                }
                NoMealRequiredVisibility = _menu.Categories.Any(r => r.Name.ToLower() == "meal not required") ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {

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

        public MenuPageViewModel(PanaceaServices core, ComputritionSettings settings, IComputritionService computrition, string mrn, Meal meal)
        {
            this._core = core;
            this.settings = settings;
            this._computrition = computrition;
            this.mrn = mrn;
            this.meal = meal;

            if (!string.IsNullOrEmpty(settings.FoodServicesPhone) && _core.TryGetTelephone(out ITelephonePlugin _tel))
            {
                CallFoodServicesVisibility = Visibility.Visible;
            }
            Order = new Order()
            {
                Recipes = new List<Recipe>()
            };
            EditMenuCommand = new RelayCommand((args) =>
            {
                var category = args as Category;
                var recipes = category.Recipes;
                EditMenuViewModel editMenu = new EditMenuViewModel(_core, SelectedRecipes, category, _menu, computrition, meal, mrn);
                if(_core.TryGetUiManager(out IUiManager _ui))
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
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    await _ui.DoWhileBusy(async () =>
                    {
                        try
                        {
                            var order2 = new Order()
                            {
                                MealDate = meal.Date,
                                MealEndTime = meal.EndTime,
                                MealId = meal.Id,
                                MealName = meal.Name,
                                MealStartTime = meal.StartTime,
                                MenuId = meal.MenuId,
                                Mrn = mrn,
                                Recipes = SelectedRecipes.ToList(),
                                UniqueHash = _menu.UniqueHash
                            };
                            await computrition.SelectPatronMenuAsync(order2);
                            HasChanges = false;
                            _ui.GoBack();
                            var popup = new OrderSentNotificationViewModel("Your order has been successfully sent!");
                            _ui.ShowPopup(popup);
                        }
                        catch (Exception ex)
                        {
                            _ui.Toast(new Translator("Computrition").Translate("An error occured. Please, try again later"));
                        }
                    });
                }
                else
                {
                    _core.Logger.Error(this, "ui manager not loaded");
                }
            },
            (args) =>
            {
                return true;
            });

            CallFoodServicesCommand = new RelayCommand((args) =>
            {
                if (_core.TryGetTelephone(out ITelephonePlugin _telephone))
                {
                    _telephone.Call(settings.FoodServicesPhone);
                } else
                {
                    _core.Logger.Error(this, "telephone not loaded");
                }
            });

            NoMealCommand = new RelayCommand(async (args) =>
            {
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    await _ui.DoWhileBusy(async () =>
                    {
                        var category = _menu.Categories.First(c => c.Name.ToLower() == "meal not required");
                        try
                        {
                            var order2 = new Order()
                            {
                                MealDate = meal.Date,
                                MealEndTime = meal.EndTime,
                                MealId = meal.Id,
                                MealName = meal.Name,
                                MealStartTime = meal.StartTime,
                                MenuId = meal.MenuId,
                                Mrn = mrn,
                                Recipes = new List<Recipe>() { category.Recipes.First() },
                                UniqueHash = _menu.UniqueHash
                            };
                            await computrition.SelectPatronMenuAsync(order2);
                            _ui.GoBack(2);
                            var popup = new OrderSentNotificationViewModel("Your order has been successfully sent!");                            
                            _ui.ShowPopup(popup);
                        }
                        catch (Exception ex)
                        {
                            _ui.Toast(new Translator("Computrition").Translate("An error occured. Please, try again later"));
                        }
                    });
                }
                else
                {
                    _core.Logger.Error(this, "ui manager not loaded");
                }
            });

            ShowNutrientsCommand = new RelayCommand(async (args) =>
            {
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    await _ui.DoWhileBusy(async () =>
                    {
                        try
                        {
                            var nutrients = await computrition.GetNutrientSummaryAsync(SelectedRecipes.ToList(), settings.PatronMenuParams.RoundingMethod, string.Join(",", settings.PatronMenuParams.Nutrients));
                            _ui.ShowPopup(new NutrientsListPopupViewModel(nutrients));
                        }
                        catch (Exception ex)
                        {
                            _ui.Toast(new Translator("Computrition").Translate("An error occured. Please, try again later"));

                        }
                    });
                }
                else
                {
                    _core.Logger.Error(this, "ui manager not loaded");
                }
            });
        }
    }

}