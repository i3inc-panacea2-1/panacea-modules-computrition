using Computrition;
using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Models;
using Panacea.Modules.Computrition.Views;
using Panacea.Multilinguality;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Panacea.Modularity.Telephone;
namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(Reminder))]
    public class ReminderViewModel : PopupViewModelBase<object>
    {
        private ComputritionPlugin _plugin;
        private PanaceaServices _core;
        private IComputritionService _computrition;
        private string _mrn;
        private PatronMenu _menu;
        private Meal _meal;
        public ComputritionMeal Meal { get; set; }
        public ComputritionSettings Settings { get; set; }
        public Visibility TextVisibility { get; set; }
        public Visibility ImageVisibility { get; set; }
        Visibility _nomealvisibility = Visibility.Collapsed;
        Visibility _callBtnVisibility = Visibility.Collapsed;
        public Visibility NoMealVisibility
        {
            get
            {
                return _nomealvisibility;
            }
            set
            {
                _nomealvisibility = value;
            }
        }
        public Visibility CallFoodServicesButtonVisibility
        {
            get
            {
                return _callBtnVisibility;
            }
            set
            {
                _callBtnVisibility = value;
            }
        }
        public ICommand NoMealCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public ICommand OrderCommand { get; set; }
        public ICommand CallFoodServicesCommand { get; set; }
        public bool CallButton { get; set; }
        public bool NoMealButton { get; set; }
        public ReminderViewModel(ComputritionPlugin plugin, PanaceaServices core, IComputritionService computrition, ComputritionSettings settings, string mrn, PatronMenu menu, ComputritionMeal compMeal, Meal meal)
        {
            this._plugin = plugin;
            this._core = core;
            this._computrition = computrition;
            this.Settings = settings;
            this._mrn = mrn;
            this._menu = menu;
            this.Meal = compMeal;
            this._meal = meal;
            if (menu != null && compMeal.NoMealButton && _menu.Categories.Exists(r => r.Name.ToLower() == "meal not required"))
            {
                NoMealVisibility = Visibility.Visible;
            }
            else
            {
                NoMealVisibility = Visibility.Collapsed;
            }
            if (compMeal.CallFoodServicesButton)
            {
                if (_core.TryGetTelephone(out ITelephonePlugin _tel))
                {
                    CallFoodServicesButtonVisibility = Visibility.Visible;
                }
            }
            else
            {
                CallFoodServicesButtonVisibility = Visibility.Collapsed;
            }
            if (compMeal.MessageType == "notification")
            {
                TextVisibility = Visibility.Visible;
                ImageVisibility = Visibility.Collapsed;
            }
            else
            {
                ImageVisibility = Visibility.Visible;
                TextVisibility = Visibility.Collapsed;
            }
            NoMealCommand = new RelayCommand(async (args) => {
                try
                {
                    var category = _menu.Categories.First(c => c.Name.ToLower() == "meal not required");
                    var order = new Order()
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
                    await computrition.SelectPatronMenuAsync(order);
                    var popup = new OrderSentNotificationViewModel("Your order has been successfully sent!");
                    //popup.MessageBox.Text = new Translator("Computrition").Translate("Your order has been successfully sent!");
                    if (_core.TryGetUiManager(out IUiManager _ui))
                    {
                        _ui.ShowPopup<object>(popup);                        
                    }
                    else
                    {
                        _core.Logger.Error(this, "ui manager not loaded");
                    }
                    Close();
                }
                catch (Exception ex)
                {
                    if (_core.TryGetUiManager(out IUiManager _ui))
                    {
                        _ui.Toast(new Translator("Computrition").Translate("An error occured. Please, try again later"));
                    }
                    else
                    {
                        _core.Logger.Error(this, "ui manager not loaded");
                    }
                }
            });
            CloseCommand = new RelayCommand(args =>{
                Close();
            });
            OrderCommand = new RelayCommand(args => {
                _plugin.Call();
                Close();
            });
            CallFoodServicesCommand = new RelayCommand(args => {
                if (_core.TryGetTelephone(out ITelephonePlugin _tel))
                {
                    _tel.Call(Settings.FoodServicesPhone);
                }
                Close();
            });
        }

        public override void Close()
        {
            if (_core.TryGetUiManager(out IUiManager _ui))
            {
                if (Meal.MessageType == "notification")
                {
                    _ui.Refrain(this);
                }
                else
                {
                    _ui.HidePopup(this);
                }
            }
            else
            {
                _core.Logger.Error(this, "ui manager not loaded");
            }
            base.Close();
        }
    }
}
