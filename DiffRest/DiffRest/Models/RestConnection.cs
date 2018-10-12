using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Models
{
    [DataContract(Name = "MetadataService")]
    public class RestConnection
    {
        [DataMember(Name = "Id")]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DataMember(Name = "Name")]
        [DisplayName("Vārds")]
        public string Name { get; set; }

        [DataMember(Name = "Url")]
        [DisplayName("Url")]
        public string Url { get; set; }

        [DataMember(Name = "Username")]
        [DisplayName("Lietotāja vārds")]
        public string Username { get; set; }

        [DataMember(Name = "Password")]
        [DisplayName("Parole")]
        public string Password { get; set; }
    }
}