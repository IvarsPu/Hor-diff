
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

        public void AddFilesToXml(string xmlPath, List<XmlFile> xmlFiles)
        {
            XmlDocument xml = new XmlDocument();

            xml.Load(xmlPath);

            foreach (XmlFile xmlFile in xmlFiles)
            {
                string xPath = string.Format(
                    "//service[@name='{0}']",
                    xmlFile.Name);

                XmlNode node = xml.SelectSingleNode(xPath);

                if (xmlFile.Attachment)
                {
                    node = xml.SelectSingleNode(xPath + "/resource");
                    if (node == null)
                    {
                        XmlElement newAttachment = xml.CreateElement("resource");
                        newAttachment.SetAttribute("path", "{pk}/attachments");
                        newAttachment.SetAttribute("name", "attachments");

                        node = xml.SelectSingleNode(xPath);

                        node.AppendChild(newAttachment);

                        node = newAttachment;
                    }
                }

                string schema = string.Empty;
                if (this.IsXSD(xmlFile.Filename))
                {
                    schema = "data_schema";
                }
                else
                {
                    schema = "service_schema";
                }

                XmlElement newElem = xml.CreateElement(schema);
                newElem.SetAttribute("name", xmlFile.Filename);

                if ((xmlFile.ErrorMSG != null) && (xmlFile.ErrorMSG != string.Empty))
                {
                    newElem.SetAttribute("error_message", xmlFile.ErrorMSG);
                    newElem.SetAttribute("status", "error");
                }

                if (!xmlFile.ErrorMSG.Contains("nav atrasts!"))
                {
                    node.AppendChild(newElem);
                    xml.Save(xmlPath);
                }
            }
        }

        private bool IsXSD(string filename)
        {
            int i = filename.LastIndexOf(".");
            string extension = filename.Substring(i);

            if (extension == ".xsd")
                return true;
            else
                return false;
        }
    }
}
