using System;
using System.IO;
using System.Xml.Linq;
using Metadataload.Models;

namespace Metadataload.Controllers
{
    internal class XmlIO
    {
        internal static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        internal void SaveXml(XmlFile xmlFile)
        {
            CreateFolder(xmlFile.LocalPath);

            try
            {
                xmlFile.XDocument.Save(xmlFile.LocalPath + xmlFile.Filename);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception: {0} while saving {1}\\{2}", ex.Message, xmlFile.LocalPath, xmlFile.Filename));
            }
        }

        internal void ParseXml(XmlFile xmlFile, LoadOptions loadOptions = LoadOptions.PreserveWhitespace)
        {
            try
            {
                xmlFile.XDocument = XDocument.Parse(xmlFile.HttpResponse, loadOptions);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception: {0} while parsing {1}", ex.Message, xmlFile.HttpResponse));
            }
        }
    }
}