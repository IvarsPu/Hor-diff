using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DiffRest.Models
{
    [Serializable]
    [DataContract(Name = "Service")]
    public class Service
    {
        public Service(string name, string description, string status)
        {
            Name = name;
            Description = description;
            Status = status;
        }

        [DataMember(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [DataMember(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [DataMember(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [DataMember(Name = "ResourceList")]
        public List<Resource> ResourceList { get; set; } = new List<Resource>();
    }
}