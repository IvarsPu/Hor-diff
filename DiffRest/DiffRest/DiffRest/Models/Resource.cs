using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DiffRest.Models
{
    [Serializable]
    [DataContract(Name = "Resource")]
    public class Resource
    {
        public Resource(string name, string status)
        {
            Name = name;
            Status = status;
        }

        [DataMember(Name = "Name")]
        public string Name { get; set; } = String.Empty;

        [DataMember(Name = "Status")]
        public string Status { get; set; } = String.Empty;
    }
}