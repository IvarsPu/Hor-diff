using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DiffRest.Models
{
    public interface IElement { }

    [Serializable]
    [KnownType(typeof(Schema))]
    [DataContract(Name = "Schema")]
    public class Schema : IElement
    {
        [DataMember(Name = "title")]
        public string title { get; set; }

        [DataMember(Name = "extraClasses")]
        public string extraClasses { get; set; } = "doc_changed";

        [DataMember(Name = "hashCode")]
        public int hashCode { get; set; }

        [DataMember(Name = "noNamspaceHashCode")]
        public int noNamspaceHashCode { get; set; }

        [DataMember(Name = "type")]
        public string type { get; set; }

        [DataMember(Name = "diffHtmlFile")]
        public String diffHtmlFile { get; set; }

        [DataMember(Name = "httpCode")]
        public int httpCode { get; set; }

        [DataMember(Name = "errorMessage")]
        public String errorMessage { get; set; }
    }

    [Serializable]
    [KnownType(typeof(Folder))]
    [DataContract(Name = "Folder")]
    public class Folder : IElement
    {
        [DataMember(Name = "title")]
        public string title { get; set; }

        [DataMember(Name = "extraClasses")]
        public string extraClasses { get; set; } = "service_changed";

        [DataMember(Name = "type")]
        public string type { get; set; }

        [DataMember(Name = "children")]
        public IList<IElement> children { get; set; }
    }
}