using Computrition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Computrition.Models
{
    public class MealDetails
    {
        public Meal Meal;
        public ComputritionMeal CompMeal;
        public MealDetails(Meal meal, ComputritionMeal compMeal)
        {
            Meal = meal;
            CompMeal = compMeal;
        }
    }
}
