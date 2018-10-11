using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Models
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

    [Serializable]
    [DataContract(Name = "Resource")]
    public class Resource
    {
        public Resource(string name, string hashCode, string noNamspaceHashCode, string status)
        {
            Name = name;
            HashCode = hashCode;
            NoNamspaceHashCode = noNamspaceHashCode;
            Status = status;
        }

        [DataMember(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [DataMember(Name = "HashCode")]
        public string HashCode { get; set; } = string.Empty;

        [DataMember(Name = "NoNamspaceHashCode")]
        public string NoNamspaceHashCode { get; set; } = string.Empty;

        [DataMember(Name = "Status")]
        public string Status { get; set; } = string.Empty;
    }
}