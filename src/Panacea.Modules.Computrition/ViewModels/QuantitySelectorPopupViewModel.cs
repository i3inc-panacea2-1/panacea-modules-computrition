using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Models;
using Panacea.Modules.Computrition.Views;
using Panacea.Mvvm;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(QuantitySelectorPopup))]
    public class QuantitySelectorPopupViewModel : PopupViewModelBase<int>
    {
        private PanaceaServices _core;
        public List<int> Quantities { get; }

        public CategoryViewModel Category { get; }

        public QuantitySelectorPopupViewModel(PanaceaServices core, List<int> quantities, CategoryViewModel category)
        {
            _core = core;
            Category = category;
            Quantities = quantities;
            AddCommand = new RelayCommand((args) =>
            {
                SetResult((int)args);
            });
        }

        public ICommand AddCommand { get; set; }
    }
}