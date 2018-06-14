
namespace HtmlXmlTest
{
    using System.Xml.Linq;
    internal class XmlFile
    {
        public XmlFile(XDocument xDocument, string filename, bool attachment = false, string errorMSG = null)
        {
            this.XDocument = xDocument;
            this.Filename = filename;
            this.Attachment = attachment;
            this.ErrorMSG = errorMSG;
        }

        public XDocument XDocument { get; set; }

        public string Filename { get; set; }

        public bool Attachment { get; set; }

        public string ErrorMSG { get; set; }
    }
}
