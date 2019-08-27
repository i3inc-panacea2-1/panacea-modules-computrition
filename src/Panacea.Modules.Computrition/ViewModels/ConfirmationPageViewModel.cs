using Computrition;
using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.Telephone;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Models;
using Panacea.Modules.Computrition.Views;
using Panacea.Multilinguality;
using Panacea.Mvvm;
using System.Windows.Input;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(ConfirmationPage))]
    public class ConfirmationPageViewModel : ViewModelBase
    {
        private PanaceaServices _core;
        public ICommand ConfirmationMade { get; set; }
        public ICommand CallStaff { get; set; }
        public ICommand ConfirmationCancelled { get; set; }
        private readonly Translator _translator = new Translator("Computrition");

        bool _status;
        public bool Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (Status)
                    _text = value;
                else
                {
                    _text = _translator.Translate("Please call a staff member to help you.");
                }
            }
        }

        public ConfirmationPageViewModel(
            PanaceaServices core,
            MenuViewModel menu,
            bool status)
        {
            _core = core;

            this.Status = status;
            this.Text = menu.PatronInfo.FirstName + " " + menu.PatronInfo.LastName;

            ConfirmationMade = new RelayCommand((args) =>
            {
                WelcomePageViewModel welcomePage = new WelcomePageViewModel(_core, menu);
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.Navigate(welcomePage);
                }
            });

            ConfirmationCancelled = new RelayCommand((args) =>
            {
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.GoHome();
                }
            });

            CallStaff = new RelayCommand((args) =>
            {
                if (_core.TryGetTelephone(out ITelephonePlugin _tel))
                {
                    menu.CallFoodServices();
                }
            });
        }
    }
}