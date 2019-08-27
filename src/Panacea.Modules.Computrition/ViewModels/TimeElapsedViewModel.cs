using Panacea.Controls;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Views;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(TimeElapsedNotification))]
    class TimeElapsedViewModel : PopupViewModelBase<bool>
    {
        internal DispatcherTimer Timer;
        private int _seconds = 10;

        string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }
        public TimeElapsedViewModel()
        {
            Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            Timer.Tick += TimeLimit_Ellapsed;
            Text = _seconds.ToString();
            YesCommand = new RelayCommand(args => SetResult(true));
            NoCommand = new RelayCommand(args => SetResult(false));
        }

        private void TimeLimit_Ellapsed(object sender, EventArgs e)
        {
            _seconds--;
            if (_seconds > 0)
            {
                Text = _seconds.ToString();
            }
            else
            {
                SetResult(false);
            }
        }

        public override void Activate()
        {
            base.Activate();
            Timer.Start();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            Timer.Stop();
        }

        public ICommand YesCommand { get; }

        public ICommand NoCommand { get; }
    }
}
