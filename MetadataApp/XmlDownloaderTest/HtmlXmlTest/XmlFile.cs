
namespace HtmlXmlTest
{
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Linq;
    internal class XmlFile
    {
        public XmlFile(string name, string localPath, string filename, bool attachment = false, string errorMSG = null)
        {
            this.Name = name;
            this.LocalPath = localPath;
            this.Filename = filename;
            this.Attachment = attachment;
            this.ErrorMSG = errorMSG;
        }

        public string Name { get; set; }

        public string LocalPath { get; set; }

        public string Filename { get; set; }

        public bool Attachment { get; set; }

        public string ErrorMSG { get; set; }

        public int HttpResultCode { get; set; }
    }
}
