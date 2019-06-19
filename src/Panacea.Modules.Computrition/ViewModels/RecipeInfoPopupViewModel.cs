using Computrition;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Computrition.Views;
using Panacea.Mvvm;

namespace Panacea.Modules.Computrition.ViewModels
{
    [View(typeof(RecipeInfoPopup))]
    public class RecipeInfoPopupViewModel : PopupViewModelBase<object>
    {
        public Recipe Recipe { get; set; }

        public RecipeInfoPopupViewModel(Recipe recipe)
        {
            this.Recipe = recipe;
        }
    }
}