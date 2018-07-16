namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    internal class XmlIO
    {
        internal static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        internal void SaveXml(XmlData xDocument, string path, string fileName)
        {
            CreateFolder(path);

            try
            {
                xDocument.XDocument.Save(path + fileName);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception: {0} while saving {1}\\{2}", ex.Message, path, fileName));
            }
        }

        internal XmlData ParseXml(DownloadData stringDocument, LoadOptions loadOptions = LoadOptions.PreserveWhitespace)
        {
            XmlData xmlData = new XmlData();
                try
                {
                    xmlData.XDocument = XDocument.Parse(stringDocument.ResponseTask.Result, loadOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Exception: {0} while parsing {1}", ex.Message, stringDocument));
                }

            return xmlData;
        }
    }
}
