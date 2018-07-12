
namespace HtmlXmlTest
{
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Linq;
    internal class XmlFile
    {
        public XmlFile(string name = null, string filename = null, bool attachment = false, string errorMSG = null)
        {
            this.Name = name;
            this.Filename = filename;
            this.Attachment = attachment;
            this.ErrorMSG = errorMSG;
        }

        public string Name { get; set; }

        public string Filename { get; set; }

        public bool Attachment { get; set; }

        public string ErrorMSG { get; set; }


    }
}
