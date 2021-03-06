﻿using Computrition;
using Computrition.Http;
using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Models;
using Panacea.Modules.Computrition.Views;
using Panacea.Multilinguality;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(LoadingSettings))]
    public class LoadingSettingsViewModel : ViewModelBase
    {
        PanaceaServices _core;
        ComputritionPlugin _plugin;
        ComputritionSettings _settings;
        string _mrn;
        private readonly string _nomealtext;
        Translator _translator = new Translator("Computrition");

        string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        public override async void Activate()
        {
            _core.WebSocket.PopularNotifyPage("Computrition");
            IComputritionService computrition = null;
            try
            {
                Status = _translator.Translate("Loading settings...");
                _settings = await _plugin.GetSettings();
            }
            catch (Exception ex)
            {
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.Toast("Computrition application is not properly configured.");
                    _ui.GoBack();
                }
                _core.Logger.Error(this, ex.Message);
                return;
            }
            try
            {

                computrition = new ComputritionService(new HttpConnector(15000), _settings.ServerAddress);

            }
            catch (Exception ex)
            {
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.Toast(_translator.Translate("Unable to contact Panacea service to fetch Computrition settings. Please, try again later."));
                    _ui.GoBack();
                }
                _core.Logger.Error(this, "Unable to get settings. " + ex.Message);
                return;
            }
            try
            {
                Status = _translator.Translate("Getting patient information...");
                if (_mrn == null)
                    _mrn = await _plugin.GetMRN();
            }
            catch (Exception ex)
            {
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.Toast(_translator.Translate("Unable to get patient information. Please, try again later."));
                    _ui.GoBack();
                }
                _core.Logger.Debug(this, "Can not get the MRN. " + ex.Message);
                return;
            }
            try
            {
                Status = _translator.Translate("Getting available meals...");
                var model = new MenuViewModel(_core, _mrn, computrition, _settings, _nomealtext);
                await model.GetMealsAsync();

                ConfirmationPageViewModel confirmation = new ConfirmationPageViewModel(_core, model, true);
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.Navigate(confirmation, false);
                }
                else
                {
                    _core.Logger.Error(this, "ui manager not loaded");
                }
            }
            catch (Exception ex)
            {
                if (_core.TryGetUiManager(out IUiManager _ui))
                {
                    _ui.Toast(_translator.Translate("Unable to get patient meal information. Please, try again later."));
                    _ui.GoBack();
                }
                _core.Logger.Debug(this, "Can not get the meal info. " + ex.Message);
                return;
            }
            base.Activate();
        }
        public LoadingSettingsViewModel(ComputritionPlugin plugin, PanaceaServices core)
        {
            _core = core;
            _plugin = plugin;
        }

        public LoadingSettingsViewModel(ComputritionPlugin plugin, PanaceaServices core, string mrn, string nomealtext)
            : this(plugin, core)
        {
            _mrn = mrn;
            _nomealtext = nomealtext;
        }
    }
}
