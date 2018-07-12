namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IElement { }

    public class Schema : IElement
    {
        public string title { get; set; }

        public string extraClasses { get; set; }

        public int hashCode { get; set; }
    }

    public class Folder : IElement
    {
        public string title { get; set; }

        public string extraClasses { get; set; }

        public IList<IElement> children { get; set; }
    }
}
