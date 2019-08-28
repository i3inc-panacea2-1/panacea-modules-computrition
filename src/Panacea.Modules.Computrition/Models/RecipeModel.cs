using Computrition;
using Panacea.Core;
using Panacea.Modularity.Telephone;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.ViewModels;
using Panacea.Multilinguality;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Panacea.Modules.Computrition.Models
{

    public class RecipeModel : Recipe, INotifyPropertyChanged
    {
        public static RecipeModel FromRecipe(Recipe recipe)
        {
            return new RecipeModel()
            {
                NumOfServings = recipe.NumOfServings,
                Description = recipe.Description,
                Id = recipe.Id,
                IsSelected = recipe.IsSelected,
                MenuRank = recipe.MenuRank,
                Name = recipe.Name,
                PortionDenominator = recipe.PortionDenominator,
                PortionDescription = recipe.PortionDescription,
                PortionNumerator = recipe.PortionNumerator,
                RecipeNutrientsList = recipe.RecipeNutrientsList,
                ShowNutrients = recipe.ShowNutrients,
            };
        }
        public override int NumOfServings
        {
            get => base.NumOfServings;
            set
            {
                base.NumOfServings = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumOfServings)));
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class MenuViewModel : INotifyPropertyChanged
    {
        private readonly PanaceaServices _core;
        private readonly IComputritionService _service;
        private readonly string _mrn;
        private readonly ComputritionSettings _settings;
        private readonly string _noMealText;
        public ComputritionSettings Settings { get => _settings; }
        public event PropertyChangedEventHandler PropertyChanged;
        public DispatcherTimer Timer { get; private set; }
        public MenuViewModel(PanaceaServices core, string mrn, IComputritionService service, ComputritionSettings settings, string noMealText)
        {
            _core = core;
            _service = service;
            _mrn = mrn;
            _settings = settings;
            _noMealText = noMealText;
            Timer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher);
            Timer.Tick += _timer_Tick;
            Timer.Interval = TimeSpan.FromMinutes(_settings.Timeout);
        }

        private async void _timer_Tick(object sender, EventArgs e)
        {
            Timer.Stop();
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                ui.BeforeNavigate -= Host_BeforeNavigate;
                if (SelectedMeal.Categories != null && SelectedMeal.Categories.Any(c => c.SelectionCount > 0))
                {

                    var pop = new TimeElapsedViewModel();
                    if (await ui.ShowPopup(pop))
                    {
                        await SubmitMenuAsync();
                        ui.GoHome();
                    }
                    else
                    {
                        ui.GoHome();
                    }
                }
                else
                {
                    ui.Notify(new Translator("Computrition").Translate("It has been too long and your menu might have changed! You were automatically navigated to Panacea™ Home."), null);
                    ui.GoHome();
                }
            }
        }


        private async void Host_BeforeNavigate(object sender, BeforeNavigateEventArgs e)
        {
            if (e.NextPage?.GetType() != typeof(EditMenuViewModel) && e.NextPage?.GetType() != typeof(MenuPageViewModel))
            {
                Timer.Stop();
                if (_core.TryGetUiManager(out IUiManager ui))
                {
                    ui.BeforeNavigate -= Host_BeforeNavigate;
                    try
                    {
                        await ui.DoWhileBusy(async () =>
                        {
                            await _service.UnlockPatronMenuAsync(SelectedMeal.Id, SelectedMeal.Date, _mrn);
                        });
                    }
                    catch (Exception ex)
                    {
                        _core.Logger.Error(this, ex.Message);
                    }
                }
            }
        }


        public async Task GetMealsAsync()
        {
            PatronInfo = await _service.GetPatronInfoAsync(
                                                    _mrn,
                                                    _settings.PatronInfoParams.ValidMeals,
                                                    _settings.PatronInfoParams.NumberOfMeals,
                                                    _settings.PatronInfoParams.NumberOfDays,
                                                    _settings.PatronInfoParams.SkipCurrentMeal);
            Meals = PatronInfo.Meals.OrderBy(m => m.StartTime).ToList().AsReadOnly();
        }

        public async Task SetSelectedMealAsync(Meal meal)
        {
            PatronMenu = await _service.GetPatronMenuAsync(
                _mrn,
                meal.Id,
                meal.Date,
                _settings.PatronMenuParams.Nutrients,
                _settings.PatronMenuParams.RoundingMethod);

            SelectedMeal = new MealViewModel(PatronMenu, meal, _noMealText);

        }

        public async Task SetSelectedMealAndMonitorAsync(Meal meal)
        {
            await SetSelectedMealAsync(meal);

            if (_core.TryGetUiManager(out IUiManager ui))
            {
                ui.BeforeNavigate += Host_BeforeNavigate;
            }
            Timer.Start();

        }

        IReadOnlyList<Meal> _meals;
        public IReadOnlyList<Meal> Meals
        {
            get => _meals;
            private set
            {
                _meals = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Meals)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroupedMeals)));
            }
        }

        public IReadOnlyList<IGrouping<DateTime, Meal>> GroupedMeals
        {
            get => Meals.GroupBy(m => m.StartTime.Date)
                        .ToList()
                        .AsReadOnly();
        }
        public PatronInfo PatronInfo { get; private set; }
        public PatronMenu PatronMenu { get; private set; }

        public MealViewModel SelectedMeal { get; set; }

        public bool CanCallfoodServices { get => !string.IsNullOrEmpty(_settings.FoodServicesPhone); }

        public void CallFoodServices()
        {
            if (_core.TryGetTelephone(out ITelephonePlugin tel))
            {
                tel.Call(_settings.FoodServicesPhone);
            }
        }

        public Task<bool> NoMealAsync()
        {
            return SubmitMenuAsync(new List<Recipe>() { SelectedMeal.NoMealCategory.Recipes.FirstOrDefault() });
        }

        async Task<bool> SubmitMenuAsync(List<Recipe> order)
        {
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                if (await ui.DoWhileBusy(async () =>
                {
                    try
                    {
                        var order2 = new Order()
                        {
                            MealDate = SelectedMeal.Date,
                            MealEndTime = SelectedMeal.EndTime,
                            MealId = SelectedMeal.Id,
                            MealName = SelectedMeal.Name,
                            MealStartTime = SelectedMeal.StartTime,
                            MenuId = SelectedMeal.MenuId,
                            Mrn = _mrn,
                            Recipes = order,
                            UniqueHash = PatronMenu.UniqueHash
                        };
                        await _service.SelectPatronMenuAsync(order2);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        ui.Toast(new Translator("Computrition").Translate("An error occured. Please, try again later"));
                        _core.Logger.Error(this, ex.Message);
                        return false;
                    }
                }))
                {
                    var popup = new OrderSentNotificationViewModel(new Translator("Computrition").Translate("Your order has been successfully sent!"));
                    await ui.ShowPopup(popup);
                    return true;
                }
                else
                {
                    var popup = new OrderSentNotificationViewModel(new Translator("Computrition").Translate("An error occured. Please, try again later."));
                    await ui.ShowPopup(popup);
                    return false;
                }
            }
            return false;
        }


        public Task<bool> SubmitMenuAsync()
        {
            return SubmitMenuAsync(SelectedMeal.SelectedRecipes.ToList());
        }

        public Task<List<Nutrient>> GetNutrientSummaryAsync(List<Recipe> recipes)
        {
            return _service.GetNutrientSummaryAsync(recipes, _settings.PatronMenuParams.RoundingMethod, string.Join(",", _settings.PatronMenuParams.Nutrients));
        }

    }

    public class MealViewModel : Meal, INotifyPropertyChanged
    {
        public MealViewModel(PatronMenu patron, Meal meal, string noMealText)
        {
            base.Code = meal.Code;
            base.Date = meal.Date;
            base.EndTime = meal.EndTime;
            base.HasOrdered = meal.HasOrdered;
            base.Id = meal.Id;
            base.MenuId = meal.MenuId;
            base.MenuName = meal.MenuName;
            base.Name = meal.Name;
            base.StartTime = meal.StartTime;

            Categories = patron.Categories
                .Where(c => c.Name != noMealText)
                .Select(c =>
                {
                    var vm = CategoryViewModel.FromCategory(c, this);
                    vm.Recipes = c.Recipes.Select(r => RecipeModel.FromRecipe(r)).Cast<Recipe>().ToList();
                    vm.SelectedRecipes = new ObservableCollection<Recipe>(vm.Recipes.Where(r => r.NumOfServings > 0));
                    return vm;
                })
                .ToList()
                .AsReadOnly();
            SelectedRecipes = new ObservableCollection<Recipe>(Categories.SelectMany(c => c.Recipes.Where(r => r.NumOfServings > 0)));
            NoMealCategory = Categories.FirstOrDefault(c => c.Name == noMealText);
            AllowsNoMeal = NoMealCategory != null;

            if (Categories.Any())
            {
                SelectedCategory = Categories[0];
            }
        }

        public Category NoMealCategory { get; }

        public bool AllowsNoMeal { get; }

        public IReadOnlyList<CategoryViewModel> Categories { get; }

        CategoryViewModel _selectedCategory;

        public ObservableCollection<Recipe> SelectedRecipes { get; private set; }

        public CategoryViewModel SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropChanged();
                OnPropChanged(nameof(PreviousCategory));
                OnPropChanged(nameof(NextCategory));
                OnPropChanged(nameof(HasNextCategory));
                OnPropChanged(nameof(HasPreviousCategory));
            }
        }

        public bool HasNextCategory { get => NextCategory != null; }

        public bool HasPreviousCategory { get => PreviousCategory != null; }

        public CategoryViewModel NextCategory
        {
            get
            {
                var lst = Categories.ToList();
                var i = lst.IndexOf(SelectedCategory);
                if (i < lst.Count - 1)
                {
                    return lst[i + 1];
                }
                return null;
            }
        }

        public CategoryViewModel PreviousCategory
        {
            get
            {
                var lst = Categories.ToList();
                var i = lst.IndexOf(SelectedCategory);
                if (i > 0)
                {
                    return lst[i - 1];
                }
                return null;
            }
        }

        public void Invalidate()
        {
            OnPropChanged(nameof(HasOrdered));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class CategoryViewModel : Category, INotifyPropertyChanged
    {
        public MealViewModel Meal { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public static CategoryViewModel FromCategory(Category c, MealViewModel meal)
        {
            return new CategoryViewModel()
            {
                Id = c.Id,
                MaxSelections = c.MaxSelections,
                Name = c.Name,
                Recipes = c.Recipes,
                Meal = meal
                //SelectedRecipes = new ObservableCollection<Recipe>(c.Recipes.Where(r => r.NumOfServings > 0))
            };
        }

        public ObservableCollection<Recipe> SelectedRecipes { get; internal set; }

        public bool HasLimit { get => MaxSelections > 0; }

        public bool CanAcceptMore { get => MaxSelections == 0 || RemainingQuantity > 0; }

        public int SelectionCount { get => SelectedRecipes.Sum(r => r.NumOfServings); }

        public int RemainingQuantity { get => MaxSelections - SelectedRecipes.Sum(r => r.NumOfServings); }

        //public ObservableCollection<Recipe> SelectedRecipes { get; private set; }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Add(Recipe recipe, int count = 1)
        {
            if (MaxSelections == 0 || count <= RemainingQuantity)
            {
                recipe.NumOfServings += count;
                recipe.IsSelected = true;
                if (!Meal.SelectedRecipes.Contains(recipe))
                {
                    Meal.SelectedRecipes.Add(recipe);
                    SelectedRecipes.Add(recipe);
                }
                OnPropertyChanged(nameof(RemainingQuantity));
                OnPropertyChanged(nameof(CanAcceptMore));
                OnPropertyChanged(nameof(SelectionCount));
            }
        }

        public void Remove(Recipe recipe, int count = 1)
        {
            recipe.NumOfServings -= count;
            if (recipe.NumOfServings <= 0)
            {
                recipe.IsSelected = false;
                recipe.NumOfServings = 0;
                Meal.SelectedRecipes.Remove(recipe);
                SelectedRecipes.Remove(recipe);
            }

            OnPropertyChanged(nameof(RemainingQuantity));
            OnPropertyChanged(nameof(CanAcceptMore));
            OnPropertyChanged(nameof(SelectionCount));
        }

        bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public List<List<Recipe>> GroupedRecipes
        {
            get
            {
                return SplitList(Recipes, 2).ToList();
            }
        }

        private static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }

    public static class RecipeExtensions
    {
        public static Recipe Clone(this Recipe r)
        {
            return new Recipe()
            {
                Description = r.Description,
                Id = r.Id,
                IsSelected = r.IsSelected,
                MenuRank = r.MenuRank,
                Name = r.Name,
                NumOfServings = r.NumOfServings,
                PortionDenominator = r.PortionDenominator,
                PortionDescription = r.PortionDescription,
                PortionNumerator = r.PortionNumerator,
                RecipeNutrientsList = r.RecipeNutrientsList,
                ShowNutrients = r.ShowNutrients
            };
        }
    }
}
