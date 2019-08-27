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

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(QuantityWarning))]
    class QuantityWarningViewModel : PopupViewModelBase<bool>
    {
        public ICommand ContinueCommand { get; set; }

        public ICommand CancelCommand { get; set; }


        public QuantityWarningViewModel()
        {
            ContinueCommand = new RelayCommand(args => SetResult(true));

            CancelCommand = new RelayCommand(args => SetResult(false));
        }
    }
}
