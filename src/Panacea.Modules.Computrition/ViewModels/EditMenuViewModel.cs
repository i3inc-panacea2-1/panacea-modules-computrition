using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Computrition;
using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Models;
using Panacea.Modules.Computrition.Views;
using Panacea.Multilinguality;
using Panacea.Mvvm;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(EditMenu))]
    public class EditMenuViewModel : ViewModelBase
    {
        private PanaceaServices _core;

        public MenuViewModel Menu { get; }

        public ObservableCollection<Recipe> Order { get; set; }

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
        public ICommand PreviousCommand { get; set; }


        public EditMenuViewModel(
            PanaceaServices core,
            MenuViewModel menu)
        {
            _core = core;
            Menu = menu;

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
                var category = menu.SelectedMeal.SelectedCategory;
                var sel = category.SelectedRecipes;
                if (category.MaxSelections <= 1 || category.MaxSelections - category.SelectionCount == 1)
                {
                    category.Add(recipe);

                    //Completed = MaxSelections != 0 && MaxSelections == SelectedRecipes.Where(r => category.Recipes.Contains(r)).Select(r => r.NumOfServings).Sum();
                }
                else
                {
                    List<int> Quantities = new List<int>();

                    for (var i = 1; i <= category.MaxSelections - category.SelectionCount; i++)
                    {
                        Quantities.Add(i);
                    }
                    var quantitySelector = new QuantitySelectorPopupViewModel(_core, Quantities);
                    _core.GetUiManager().ShowPopup(quantitySelector);
                    quantitySelector.Add += (oo, ee) =>
                    {
                        category.Add(recipe, ee);
                        //Completed = MaxSelections == SelectedRecipes.Where(r => category.Recipes.Contains(r)).Select(r => r.NumOfServings).Sum();
                    };
                }
                (AddToTrayCommand as RelayCommand).RaiseCanExecuteChanged();
            },
            (args) =>
            {
                var category = menu.SelectedMeal.SelectedCategory;
                var sel = category.SelectedRecipes;
                return category.MaxSelections == 0 || category.MaxSelections > sel.Select(r => r.NumOfServings).Sum();
            });
            NextCommand = new RelayCommand(async args =>
            {
                var c = menu.SelectedMeal.SelectedCategory;
                if (c.CanAcceptMore && c.HasLimit)
                {
                    if (_core.TryGetUiManager(out IUiManager ui))
                    {
                        var popup = new QuantityWarningViewModel();
                        if (await ui.ShowPopup(popup))
                        {
                            menu.SelectedMeal.SelectedCategory = menu.SelectedMeal.NextCategory;
                        }
                    }
                }
                else
                {
                    menu.SelectedMeal.SelectedCategory = menu.SelectedMeal.NextCategory;
                }

            });
            PreviousCommand = new RelayCommand(async args =>
            {
                var c = menu.SelectedMeal.SelectedCategory;
                if (c.CanAcceptMore && c.HasLimit)
                {
                    if (_core.TryGetUiManager(out IUiManager ui))
                    {
                        var popup = new QuantityWarningViewModel();
                        if (await ui.ShowPopup(popup))
                        {
                            menu.SelectedMeal.SelectedCategory = menu.SelectedMeal.PreviousCategory;
                        }
                    }
                }
                else
                {
                    menu.SelectedMeal.SelectedCategory = menu.SelectedMeal.PreviousCategory;
                }
            });
            AddOneCommand = new RelayCommand((args) =>
            {
                menu.SelectedMeal.SelectedCategory = menu.SelectedMeal.PreviousCategory;
            },
            (args) =>
            {
                var c = menu.SelectedMeal.SelectedCategory;
                return c.MaxSelections == 0 || c.SelectionCount < c.MaxSelections;
            });

            RemoveOneCommand = new RelayCommand((args) =>
            {
                var c = menu.SelectedMeal.SelectedCategory;
                var recipe = args as Recipe;
                c.Remove(recipe);
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
                menu.SelectedMeal.SelectedCategory.Remove(recipe);
            },
            (args) =>
            {
                return true;
            });
            CompleteOrderCommand = new RelayCommand(async (args) =>
            {
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    if (await Menu.SubmitMenuAsync())
                        _ui.GoBack(2);
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