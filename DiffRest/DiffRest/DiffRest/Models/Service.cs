using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DiffRest.Models
{
    [Serializable]
    [DataContract(Name = "Service")]
    public class Service
    {
        public Service(string description)
        {
            Description = description;
        }

        [DataMember(Name = "Description")]
        public string Description { get; set; } = String.Empty;

        [DataMember(Name = "ResourceList")]
        public List<Resource> ResourceList { get; set; } = new List<Resource>();
    }
}