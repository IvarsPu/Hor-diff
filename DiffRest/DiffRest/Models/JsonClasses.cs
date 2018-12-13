using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Models
{
    [Serializable]
    [KnownType(typeof(Schema))]
    [DataContract(Name = "Schema")]
    public class Schema
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "extraClasses")]
        public string ExtraClasses { get; set; } = "doc_changed";

        [DataMember(Name = "hashCode")]
        public string HashCode { get; set; }

        [DataMember(Name = "noNamspaceHashCode")]
        public string NoNamspaceHashCode { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "diffHtmlFile")]
        public String DiffHtmlFile { get; set; }

        [DataMember(Name = "httpCode")]
        public int HttpCode { get; set; }

        [DataMember(Name = "errorMessage")]
        public String ErrorMessage { get; set; }
    }

    [Serializable]
    [KnownType(typeof(Folder))]
    [DataContract(Name = "Folder")]
    public class Folder
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "extraClasses")]
        public string ExtraClasses { get; set; } = "service_changed";

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "children")]
        public List<object> Children { get; set; }
    }
}