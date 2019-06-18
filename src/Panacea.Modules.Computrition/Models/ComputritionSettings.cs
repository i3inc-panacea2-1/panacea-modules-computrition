using Panacea.Multilinguality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Computrition.Models
{
    [DataContract]
    public class ComputritionSettings : Translatable
    {
        [DataMember(Name = "serverAddress")]
        public string ServerAddress { get; set; }

        [DataMember(Name = "patronMenuParams")]
        public PatronMenuParamsModel PatronMenuParams { get; set; }

        [DataMember(Name = "patronInfoParams")]
        public PatronInfoParamsModel PatronInfoParams { get; set; }

        [IsTranslatable(256)]
        [DataMember(Name = "orderButtonText")]
        public string OrderButtonText { get; set; }

        [IsTranslatable(256)]
        [DataMember(Name = "callFoodServicesButtonText")]
        public string CallFoodServicesButtonText { get; set; }

        [IsTranslatable(256)]
        [DataMember(Name = "closeButtonText")]
        public string CloseButtonText { get; set; }

        [IsTranslatable(256)]
        [DataMember(Name = "noMealButtonText")]
        public string NoMealButtonText { get; set; }

        [DataMember(Name = "foodServicesPhone")]
        public string FoodServicesPhone { get; set; }

        [DataMember(Name = "checkInterval")]
        public int CheckInterval { get; set; }

        [DataMember(Name = "reminders")]
        public List<int> Reminders { get; set; }

        [DataMember(Name = "meals")]
        public List<ComputritionMeal> Meals { get; set; }
    }
}
