using System;
using System.Collections.Generic;

namespace DiffRest.Models
{
    public interface IElement { }

    public class Schema : IElement
    {
        public string title { get; set; }

        public string extraClasses { get; set; } = "doc_changed";

        public int hashCode { get; set; }

        public int noNamspaceHashCode { get; set; }

        public string type { get; set; }

        public String diffHtmlFile { get; set; }

        public int httpCode { get; set; }

        public String errorMessage { get; set; }
    }

    public class Folder : IElement
    {
        public string title { get; set; }

        public string extraClasses { get; set; } = "service_changed";

        public string type { get; set; }

        public IList<IElement> children { get; set; }
    }
}