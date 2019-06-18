using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Computrition.Models
{
    public class Mrn
    {
        [DataMember(Name = "mrn")]
        public string MRN { get; set; }
    }
}
