using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Panacea.Modules.Computrition.Models
{
    public class PatronMenuParamsModel
    {
        [DataMember(Name = "nutrients")]
        public List<string> Nutrients { get; set; }

        [DataMember(Name = "roundingMethod")]
        public String RoundingMethod { get; set; }
    }
}