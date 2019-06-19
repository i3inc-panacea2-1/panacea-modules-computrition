﻿using Computrition;
using Computrition.Http;
using Computrition.Mock;
using Panacea.Core;
using Panacea.Modularity;
using Panacea.Modules.Computrition.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using PatronInfo = Computrition.PatronInfo;
using Meal = Computrition.Meal;
using Menu = Computrition.PatronMenu;
using Panacea.Modularity.UiManager;
using Panacea.Mvvm;
using Panacea.Modules.Computrition.ViewModels;
using System.Windows;

namespace Panacea.Modules.Computrition
{
    public class ComputritionPlugin : ICallablePlugin
    {
        //To be set by cmd arguments
        protected string computritionmrn;
        DispatcherTimer refreshTimer;
        DispatcherTimer reminderTimer;
        ComputritionSettings _settings;
        PatronInfo _patron;
        string _mrn;
        IComputritionService computrition = null;
        int refreshInterval = 1;//default if no server answer
        private readonly PanaceaServices _core;

        public ComputritionPlugin(PanaceaServices core)
        {
            _core = core;
            computritionmrn = "H001501022";
        }
        public Task BeginInit()
        {
            return Task.CompletedTask;
        }
        public Task EndInit()
        {
            RefreshSettings().ContinueWith(tsk=> {
                SetupRefreshTimer(refreshInterval);
                SetupReminderTimer();
            });
            return Task.CompletedTask;
        }
        public Task Shutdown()
        {
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            return;
        }

        //TODO: NOT NEEDED I GUESS? public IMainScreenButton MainButton { get; set; }

        public void Call()
        {
            if (_core.TryGetUiManager(out IUiManager _ui)){
                _ui.Navigate(new LoadingSettingsViewModel(this, _core), false);
            }
            else
            {
                _core.Logger.Error(this, "ui manager not loaded");
            }
            //TODO: MAIN ENTRY TURN TO MVVM_window.ThemeManager.Navigate(new LoadingSettingsControl(), false);
        }
        public void SetupRefreshTimer(int interval)
        {
            refreshTimer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher);
            refreshTimer.Interval = TimeSpan.FromMinutes(interval);
            refreshTimer.Tick += RefreshSettings;
            refreshTimer.Start();
        }
        public void SetupReminderTimer()
        {
            reminderTimer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher);
            reminderTimer.Interval = TimeSpan.FromSeconds(1);//.FromMinutes(5);
            reminderTimer.Tick += CheckForReminder;
            reminderTimer.Start();
        }
        private async void CheckForReminder(object sender, EventArgs e)
        {
            if (_settings == null || _patron == null)
            {
                return;
            }
            var mealDetails = GetNextMealReminder(_settings, _patron);
            var mealToRemind = mealDetails?.CompMeal;
            var meal = mealDetails?.Meal;
#if DEBUG
            (sender as DispatcherTimer).Stop();
#endif
            if (mealDetails == null)
            {
#if DEBUG
                mealToRemind = _settings.Meals.Find(m => { return m.Code == "LUN"; });
                meal = _patron.Meals.Find(m => { return m.Code == "LUN"; });
#else
                return;
#endif
            }
            if (mealToRemind == null || meal == null)
            {
                return;
            }
            ReminderViewModel reminder;
            try
            {
                var menu = await GetMenu(meal);
                reminder = new ReminderViewModel(this, _core, computrition, _settings, _mrn, menu, mealToRemind, meal);
            }
            catch (Exception ex)
            {
                reminder = new ReminderViewModel(this, _core, computrition, _settings, _mrn, null, mealToRemind, meal);
                _core.Logger.Error(this, "Could not get menu information!");
            }
            if (_core.TryGetUiManager(out IUiManager _ui))
            {
                if (mealToRemind.MessageType == "notification")
                {
                    _ui.Notify(reminder);
                } else
                {
                    _ui.ShowPopup<object>(reminder);
                }
            }
            else
            {
                _core.Logger.Error(this, "ui manager not loaded");
            }
        }
        private async void RefreshSettings(object sender, EventArgs e)
        {
            if ((sender as DispatcherTimer).Interval.TotalMinutes != refreshInterval)
            {
                (sender as DispatcherTimer).Interval = TimeSpan.FromMinutes(refreshInterval);
            }
            await RefreshSettings();
        }

        public async Task RefreshSettings()
        {
            try
            {
                _settings = await GetSettings();
                refreshInterval = _settings.CheckInterval != 0 ? _settings.CheckInterval : 10;
                _mrn = await GetMRN();
                if (Debugger.IsAttached)
                {
                    computrition = new ComputritionService(new MockConnector(), "");
                }
                else
                {
                    computrition = new ComputritionService(new HttpConnector(15000), _settings.ServerAddress);
                }
                _patron = await GetPatronInfo();
            }
            catch (Exception ex)
            {
                _core.Logger.Error(this, "Unable to get settings: " + ex.Message);
                refreshInterval = 1;
            }
        }
        public async Task<ComputritionSettings> GetSettings()
        {
            var serverResponse =
                await _core.HttpClient.GetObjectAsync<ComputritionSettings>("computrition/get_settings/");
            if (!serverResponse.Success)
            {
                throw (new Exception("Computrition not configured (success: false)"));
            }
            return serverResponse.Result;
        }
        public async Task<string> GetMRN()
        {
            var response = await _core.HttpClient.GetObjectAsync<Mrn>("computrition/get_mrn/", allowCache: false);

            if (response.Success)
            {
                return response.Result.MRN;
            }
            else if (computritionmrn != null)
            {
                return computritionmrn; // "H001501022"
            }
            else
            {
                if (!response.Success)
                {
                    throw (new Exception(response.Error));
                }
                else
                {
                    throw (new Exception("Could not get mrn"));
                }
            }
        }
        public async Task<PatronInfo> GetPatronInfo()
        {
            return await computrition.GetPatronInfoAsync(
                                        _mrn,
                                        _settings.Meals.Select(m => { return m.Code; }).ToList(),
                                        _settings.PatronInfoParams.NumberOfMeals,
                                        _settings.PatronInfoParams.NumberOfDays,
                                        _settings.PatronInfoParams.SkipCurrentMeal);
        }
        public async Task<Menu> GetMenu(Meal meal)
        {
            return await computrition.GetPatronMenuAsync(
                                        _mrn, 
                                        meal.Id, 
                                        meal.Date, 
                                        _settings.PatronMenuParams.Nutrients, 
                                        _settings.PatronMenuParams.RoundingMethod);
        }
        MealDetails GetNextMealReminder(ComputritionSettings settings, PatronInfo patron)
        {
            var now = DateTime.Now;
            var nextCheck = now.AddMinutes(5);
            if (settings?.Reminders.Count > 0)
            {
                foreach (ComputritionMeal compMeal in settings.Meals)
                {
                    var meal = patron.Meals?.Find((cMeal) => { return cMeal.Code == compMeal.Code; });
                    if (meal == null)
                    {
                        break;
                    }
                    var endingTime = meal.EndTime;
                    if (settings.Reminders
                        .Select((reminder) => { return endingTime.AddMinutes(-reminder); })
                        .Where((notTime) => { return (notTime.CompareTo(nextCheck) <= 0 && notTime.CompareTo(now) > 0); })
                        .ToList().Count > 0)
                    {
                        return new MealDetails(meal, compMeal);
                    }
                }
            }
            return null;
        }
    }
    public static class DistinctExt
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}