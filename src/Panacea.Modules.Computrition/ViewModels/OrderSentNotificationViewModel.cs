using Panacea.Controls;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Views;
using Panacea.Multilinguality;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(OrderSentNotification))]
    public class OrderSentNotificationViewModel : PopupViewModelBase<object>
    {
        public String Message { get; set; }
        public ICommand CloseCommand { get; set; }
        public OrderSentNotificationViewModel(string message)
        {
            Message = new Translator("Computrition").Translate(message);
            CloseCommand = new RelayCommand(args => {
                Close();
            });
        }
    }
}
