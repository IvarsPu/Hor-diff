using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        public string Url { get; set; } = "";

        [DataMember(Name = "Username")]
        [DisplayName("Lietotāja vārds")]
        public string Username { get; set; }

        [DataMember(Name = "Password")]
        [DisplayName("Parole")]
        public string Password { get; set; }

        [DataMember(Name = "ParallelThreads")]
        [DisplayName("Paralēli ielādes procesi")]
        public int ParallelThreads { get; set; } = 2;

        [DataMember(Name = "LoadQuery")]
        [DisplayName("Ielādēt query struktūru")]
        public bool LoadQuery { get; set; } = false;

        [DataMember(Name = "LoadTemplate")]
        [DisplayName("Ielādēt template struktūru")]
        public bool LoadTemplate { get; set; } = false;
    }
}