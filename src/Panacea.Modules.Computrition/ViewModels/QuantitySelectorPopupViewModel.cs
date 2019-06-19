using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Views;
using Panacea.Mvvm;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(QuantitySelectorPopup))]
    public class QuantitySelectorPopupViewModel : PopupViewModelBase<object>
    {
        private PanaceaServices _core;
        public List<int> Quantities { get; set; }
        public QuantitySelectorPopupViewModel(PanaceaServices core, List<int> quantities)
        {
            _core = core;
            this.Quantities = quantities;
            AddCommand = new RelayCommand((args) => {
                Add?.Invoke(this, (int)args);
                SetResult(null);
            });
        }
        public event EventHandler<int> Add;
        public ICommand AddCommand { get; set; }
    }
}