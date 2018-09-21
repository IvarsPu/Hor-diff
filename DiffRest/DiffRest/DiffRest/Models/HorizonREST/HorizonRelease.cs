using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace DiffRest.Models
{
    [Serializable]
    [DataContract(Name = "HorizonRelease")]
    public class HorizonRelease
    {
        public HorizonRelease(string release)
        {
            Release = release;
        }

        [DataMember(Name = "Release")]
        public string Release { get; set; } = string.Empty;
    }
}