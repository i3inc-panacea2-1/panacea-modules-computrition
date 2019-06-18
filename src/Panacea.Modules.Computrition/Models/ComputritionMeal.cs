using Panacea.Models;
using Panacea.Multilinguality;
using System.Runtime.Serialization;

namespace Panacea.Modules.Computrition.Models
{
    [DataContract]
    public class ComputritionMeal : Translatable
    {
        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "messageType")]
        public string MessageType { get; set; }

        [IsTranslatable]
        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "image")]
        public string Image { get; set; }

        [DataMember(Name = "image_thumbnail")]
        public Thumbnail ImageThumbnail { get; set; }

        [DataMember(Name = "noMealButton")]
        public bool NoMealButton { get; set; }

        [DataMember(Name = "callFoodServicesButton")]
        public bool CallFoodServicesButton { get; set; }
    }
}