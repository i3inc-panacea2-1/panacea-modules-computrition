using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Panacea.Modules.Computrition.Models
{
    public class PatronInfoParamsModel
    {
        [DataMember(Name = "validMeals")]
        public List<string> ValidMeals { get; set; }

        [DataMember(Name = "numberOfMeals")]
        public int NumberOfMeals { get; set; }

        [DataMember(Name = "skipCurrentMeal")]
        public bool SkipCurrentMeal { get; set; }

        [DataMember(Name = "numberOfDays")]
        public int NumberOfDays { get; set; }
    }
}