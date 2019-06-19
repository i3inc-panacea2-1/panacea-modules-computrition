using Computrition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Computrition.Models
{
    public class RecipeModel : Recipe, INotifyPropertyChanged
    {
        public static RecipeModel FromRecipe(Recipe recipe)
        {
            return new RecipeModel()
            {
                NumOfServings = recipe.NumOfServings,
                Description = recipe.Description,
                Id = recipe.Id,
                IsSelected = recipe.IsSelected,
                MenuRank = recipe.MenuRank,
                Name = recipe.Name,
                PortionDenominator = recipe.PortionDenominator,
                PortionDescription = recipe.PortionDescription,
                PortionNumerator = recipe.PortionNumerator,
                RecipeNutrientsList = recipe.RecipeNutrientsList,
                ShowNutrients = recipe.ShowNutrients
            };
        }
        public override int NumOfServings
        {
            get => base.NumOfServings;
            set
            {
                base.NumOfServings = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NumOfServings"));
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
