using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace DiffRest.Models
{
    [Serializable]
    [DataContract(Name = "HorizonVersion")]
    public class HorizonVersion
    {
        public HorizonVersion(string version)
        {
            Version = version;
        }

        [DataMember(Name = "Version")]
        public string Version { get; set; } = String.Empty;

        [DataMember(Name = "ReleaseList")]
        public List<HorizonRelease> ReleaseList { get; set; } = new List<HorizonRelease>();
    }
}