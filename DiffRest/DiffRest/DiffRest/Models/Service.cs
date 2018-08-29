using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Web;

namespace DiffRest.Models
{
    [Serializable]
    [DataContract(Name = "Service")]
    public class Service
    {
        public Service(string name, string description)
        {
            Name = name;
            Description = description;
        }

        [DataMember(Name = "Name")]
        public string Name { get; set; } = String.Empty;

        [DataMember(Name = "Description")]
        public string Description { get; set; } = String.Empty;

        [DataMember(Name = "ResourceList")]
        public List<Resource> ResourceList { get; set; } = new List<Resource>();
    }
}