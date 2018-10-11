using System;
using System.Runtime.Serialization;

namespace DiffRest.Models
{
    [Serializable]
    [DataContract(Name = "MetadataService")]
    public class MetadataService
    {
        [DataMember(Name = "Id")]
        public int Id { get; set; }

        [DataMember(Name = "Name")]
        public string Name { get; set; }

        [DataMember(Name = "Url")]
        public string Url { get; set; }

        [DataMember(Name = "Username")]
        public string Username { get; set; }

        [DataMember(Name = "Password")]
        public string Password { get; set; }
    }
}