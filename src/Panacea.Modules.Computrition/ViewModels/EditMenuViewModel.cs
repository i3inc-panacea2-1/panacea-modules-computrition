using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Computrition;
using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Views;
using Panacea.Multilinguality;
using Panacea.Mvvm;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(EditMenu))]
    public class EditMenuViewModel : ViewModelBase
    {
        private PanaceaServices _core;
        private Category category;
        private PatronMenu _menu;
        private IComputritionService computrition;
        private Meal meal;
        private string mrn;


        public List<Recipe> Recipes { get; set; }
        public List<List<Recipe>> GroupedRecipes { get; set; }
        public ObservableCollection<Recipe> SelectedRecipes { get; set; }

        public ObservableCollection<Recipe> Order { get; set; }
        public string Title { get; set; }
        public bool HasNext { get; set; } = true;
        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
        public ICommand CompleteOrderCommand { get; set; }

        public ICommand AddOneCommand { get; set; }

        public ICommand RemoveOneCommand { get; set; }

        public ICommand RemoveCommand { get; set; }

        public ICommand InfoCommand { get; set; }

        public ICommand AddToTrayCommand { get; set; }

        public ICommand NextCommand { get; set; }

        public string NextCategory { get; set; }

        public int MaxSelections { get; set; }

        int _selectionCount;
        public int SelectionCount
        {
            get => _selectionCount;
            set
            {
                _selectionCount = value;
                OnPropertyChanged();
            }
        }
        bool _completed;
        public bool Completed
        {
            get => _completed;
            set
            {
                _completed = value;
                OnPropertyChanged();
            }
        }
        public EditMenuViewModel(PanaceaServices core, ObservableCollection<Recipe> order, Category category, PatronMenu menu, IComputritionService computrition, Meal meal, string mrn)
        {
            _core = core;
            this.category = category;
            _menu = menu;
            this.computrition = computrition;
            this.meal = meal;
            this.mrn = mrn;
            Recipes = category.Recipes;
            GroupedRecipes = SplitList(Recipes, 2).ToList();
            SelectedRecipes = new ObservableCollection<Recipe>(order.Where(r => category.Recipes.Contains(r)));
            SelectionCount = SelectedRecipes.Sum(r => r.NumOfServings);
            MaxSelections = category.MaxSelections;
            Title = category.Name;
            var index = menu.Categories.IndexOf(category);
            if (index != menu.Categories.Count() - 1)
            {
                HasNext = true;
                NextCategory = menu.Categories[++index].Name;
            }
            else HasNext = false;
            InfoCommand = new RelayCommand((args) =>
            {
                var recipe = args as Recipe;
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.ShowPopup(new RecipeInfoPopupViewModel(recipe));
                }
                else
                {
                    _core.Logger.Error(this, "ui manager not loaded");
                }
            });
            AddToTrayCommand = new RelayCommand((args) =>
            {
                var recipe = args as Recipe;
                recipe.IsSelected = true;
                if (category.MaxSelections <= 1 || category.MaxSelections - SelectionCount == 1)
                {
                    var existing = SelectedRecipes.FirstOrDefault(r => r == recipe);
                    if (existing != null) existing.NumOfServings++;
                    else
                    {
                        SelectedRecipes.Add(recipe);
                        order.Add(recipe);
                        recipe.NumOfServings = 1;
                    }
                    SelectionCount++;
                    Completed = MaxSelections != 0 && MaxSelections == SelectedRecipes.Where(r => category.Recipes.Contains(r)).Select(r => r.NumOfServings).Sum();
                }
                else
                {
                    List<int> Quantities = new List<int>();
                    
                    for (var i = 1; i <= category.MaxSelections - SelectionCount; i++)
                    {
                        Quantities.Add(i);
                    }
                    var quantitySelector = new QuantitySelectorPopupViewModel(_core, Quantities);
                    _core.GetUiManager().ShowPopup(quantitySelector);
                    quantitySelector.Add += (oo, ee) =>
                    {
                        SelectionCount += ee;

                        var existing = SelectedRecipes.FirstOrDefault(r => r == recipe);
                        if (existing != null) existing.NumOfServings += ee;
                        else
                        {
                            SelectedRecipes.Add(recipe);
                            order.Add(recipe);
                            recipe.NumOfServings = ee;
                        }

                        Completed = MaxSelections == SelectedRecipes.Where(r => category.Recipes.Contains(r)).Select(r => r.NumOfServings).Sum();
                    };
                }
                (AddToTrayCommand as RelayCommand).RaiseCanExecuteChanged();
            },
            (args) => MaxSelections == 0 || MaxSelections > SelectedRecipes.Select(r => r.NumOfServings).Sum());
            NextCommand = new RelayCommand((args) =>
            {

                var newCategory = menu.Categories[index];
                var recipes = newCategory.Recipes;
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.Navigate(new EditMenuViewModel(_core, order, newCategory, menu, computrition, meal, mrn), false);
                }
            });
            AddOneCommand = new RelayCommand((args) =>
            {
                var recipe = args as Recipe;
                recipe.NumOfServings++;
                SelectionCount++;
                (AddOneCommand as RelayCommand)?.OnCanExecuteChanged();
            },
            (args) =>
            {
                return MaxSelections == 0 || SelectionCount < MaxSelections;
            });

            RemoveOneCommand = new RelayCommand((args) =>
            {
                var recipe = args as Recipe;
                recipe.NumOfServings--;
                SelectionCount--;
                if (recipe.NumOfServings == 0)
                {
                    recipe.IsSelected = false;
                    //order.Remove(recipe);
                    SelectedRecipes.Remove(recipe);
                }
                (RemoveOneCommand as RelayCommand)?.OnCanExecuteChanged();
            },
            (args) =>
            {
                var recipe = args as Recipe;
                return recipe.NumOfServings > 1;
            });

            RemoveCommand = new RelayCommand((args) =>
            {
                var recipe = args as Recipe;
                SelectionCount -= recipe.NumOfServings;
                recipe.NumOfServings = 0;
                recipe.IsSelected = false;
                //order.Remove(recipe);
                SelectedRecipes.Remove(recipe);
            },
            (args) =>
            {
                return true;
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
                                Recipes = order.ToList(),
                                UniqueHash = menu.UniqueHash
                            };
                            await computrition.SelectPatronMenuAsync(order2);
                            _ui.GoBack(2);
                            _ui.ShowPopup(new OrderSentNotificationViewModel("Your order has been successfully sent!"));
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
        }
    }
}