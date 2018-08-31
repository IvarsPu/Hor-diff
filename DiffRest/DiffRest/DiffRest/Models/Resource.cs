using System;
using System.Runtime.Serialization;

namespace DiffRest.Models
{
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