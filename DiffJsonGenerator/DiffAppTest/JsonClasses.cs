using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffAppTest
{
    public interface IElement { }

    public class Schema : IElement
    {
        public string title { get; set; }

        public string extraClasses { get; set; }

        public int hashCode { get; set; }

        public int noNamspaceHashCode { get; set; }

        public string type { get; set; } //xmlNode.tagName

        public String diffHtmlFile { get; set; }

        public int httpCode { get; set; }

        public String errorMessage { get; set; }
    }

    public class Folder : IElement
    {
        public string title { get; set; }

        public string extraClasses { get; set; }

        public string type { get; set; } //xmlNode.tagName

        public IList<IElement> children { get; set; }
    }
}
