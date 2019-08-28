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
        private PanaceaServices _core;
        private readonly MenuViewModel _menu;

        public ComputritionMeal Meal { get; set; }

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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }
        public ICommand NoMealCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public ICommand OrderCommand { get; set; }
        public ICommand CallFoodServicesCommand { get; set; }
        public bool CallButton { get; set; }
        public bool NoMealButton { get; set; }
        public MenuViewModel Menu { get => _menu; }
        public ReminderViewModel(PanaceaServices core, MenuViewModel menu, ComputritionMeal compMeal, ComputritionPlugin plugin)
        {

            _core = core;
            _menu = menu;
            if (menu != null && menu.SelectedMeal.AllowsNoMeal)
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
            NoMealCommand = new RelayCommand(async (args) =>
            {
                try
                {
                    await menu.NoMealAsync();
                    Close();
                }
                catch (Exception)
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
            CloseCommand = new RelayCommand(args =>
            {
                Close();
            });
            OrderCommand = new RelayCommand(args =>
            {
                plugin.Call();
                Close();
            });
            CallFoodServicesCommand = new RelayCommand(args =>
            {
                menu.CallFoodServices();
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
