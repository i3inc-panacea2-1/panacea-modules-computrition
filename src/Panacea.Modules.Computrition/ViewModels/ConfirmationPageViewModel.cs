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
        private IComputritionService computrition;
        private ComputritionSettings _settings;
        private string _mrn;
        private PatronInfo patron;
        public ICommand ConfirmationMade { get; set; }
        public ICommand CallStaff { get; set;}
        public ICommand ConfirmationCancelled { get; set; }
        private readonly Translator _translator = new Translator("Computrition");

        PatronInfo _patronInfo;
        public PatronInfo PatronInfo
        {
            get => _patronInfo;
            set
            {
                _patronInfo = value;
                OnPropertyChanged();
            }
        }

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

        public ConfirmationPageViewModel(PanaceaServices core, IComputritionService computrition, ComputritionSettings settings, string mrn, bool status, PatronInfo patron)
        {
            _core = core;
            _settings = settings;
            this.computrition = computrition;
            this.Status = status;
            this.PatronInfo= patron;
            this.Text = patron.FirstName + " " + patron.LastName;

            ConfirmationMade = new RelayCommand((args) =>
            {
                WelcomePageViewModel welcomePage = new WelcomePageViewModel(_core, settings, computrition, mrn, Text, PatronInfo);
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.Navigate(welcomePage);
                }
            });

            ConfirmationCancelled = new RelayCommand((args) => {
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.GoHome();
                }
            });
            CallStaff = new RelayCommand((args) => {
                if (_core.TryGetTelephone(out ITelephonePlugin _tel))
                {
                    _tel.Call(_settings.FoodServicesPhone);
                }
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.GoHome();
                }
            });
        }
    }
}