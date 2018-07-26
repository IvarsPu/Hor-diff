namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Linq;
    using System.Xml.Linq;

    class WebResourceReader
    {
        public static void test()
        {
            XmlFile xmlFile = new XmlFile("", "", "", "query.xml");
            xmlFile.XDocument = XDocument.Load("C:\\Temp\\test\\query.xml", LoadOptions.None);

            int hash1 = GetNoNamspaceHash(xmlFile);

             xmlFile = new XmlFile("", "", "", "query1.xml");
            xmlFile.XDocument = XDocument.Load("C:\\Temp\\test\\query1.xml", LoadOptions.None);

            int hash2 = GetNoNamspaceHash(xmlFile);

            
            Console.WriteLine("test Result: " + (hash1 == hash2));
        }


        public static List<RestService> LoadRestServices(string url, ref string rootLocalPath, ref string metadataPath)
        {
            List<RestService> services = new List<RestService>();
            WebResourceLoader webResourceLoader = new WebResourceLoader(url, rootLocalPath);
            WebResponse myResponse = webResourceLoader.GetResponseFromSite(url + "global/agentVersion").Result;
            Stream myStream = myResponse.GetResponseStream();
            XmlReader xmlReader = XmlReader.Create(myStream);

            string value = string.Empty;
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Text)
                {
                    value = xmlReader.Value;
                }
            }

            int index = value.LastIndexOf(".");
            string release = value.Substring(index + 1);
            value = value.Remove(index);
            index = value.LastIndexOf(".");
            string version = value.Substring(index + 1);

            rootLocalPath += version + "." + release + "\\";
            string localPath = rootLocalPath;

            XmlIO xmlIO = new XmlIO();
            XmlIO.CreateFolder(localPath);

            metadataPath = localPath + "metadata.xml";
            XmlWriter xmlWriter = XmlWriter.Create(metadataPath);
            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement("rest_api_metadata");
            xmlWriter.WriteAttributeString("release", release);
            xmlWriter.WriteAttributeString("version", version);

            myResponse = webResourceLoader.GetResponseFromSite(url).Result;
            myStream = myResponse.GetResponseStream();
            xmlReader = XmlReader.Create(myStream);

            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (xmlReader.Name == "group")
                        {
                            string desc = string.Empty;
                            while (xmlReader.NodeType != XmlNodeType.EndElement)
                            {
                                xmlReader.Read();
                                if (xmlReader.NodeType == XmlNodeType.Text)
                                {
                                    desc = xmlReader.Value;

                                    FixFilename(ref desc);
                                }
                            }

                            localPath += desc + "\\";

                            XmlIO.CreateFolder(localPath);
                            //Console.WriteLine(localPath);

                            xmlWriter.WriteStartElement("service_group");
                            xmlWriter.WriteAttributeString("name", desc);
                        }
                        else if (xmlReader.Name == "link")
                        {
                            RestService service = new RestService();
                            while (xmlReader.NodeType != XmlNodeType.Text)
                            {
                                xmlReader.Read();
                            }

                            service.Href = xmlReader.Value;

                            xmlReader.Read();

                            while (xmlReader.NodeType != XmlNodeType.Text)
                            {
                                xmlReader.Read();
                            }

                            service.Description = xmlReader.Value;


                            string name = service.Href;
                            int i = name.LastIndexOf("/");
                            service.Name = name.Substring(i + 1);


                            service.Filepath = localPath + service.Name + "\\";

                            services.Add(service);

                            xmlWriter.WriteStartElement("service");
                            xmlWriter.WriteAttributeString("description", service.Description);
                            xmlWriter.WriteAttributeString("name", service.Name);
                            xmlWriter.WriteEndElement();
                        }

                        // Console.WriteLine(xmlReader.Name + " EN");
                        // Console.WriteLine(xmlReader.Value + " EV");
                        break;
                    case XmlNodeType.Text:
                        // Console.WriteLine(xmlReader.Value + " TV");
                        break;
                    case XmlNodeType.EndElement:
                        if (xmlReader.Name == "group")
                        {
                            var lastFolder = Path.GetDirectoryName(localPath);
                            var pathWithoutLastFolder = Path.GetDirectoryName(lastFolder);
                            localPath = pathWithoutLastFolder + "\\";

                            xmlWriter.WriteEndElement();
                        }

                        // Console.WriteLine("END " + xmlReader.Name);
                        break;
                }
            }

            xmlReader.Close();
            myResponse.Close();

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();

            return services;
        }

        public static void AddFilesToXml(string xmlPath, List<XmlFile> xmlFiles)
        {
            XmlDocument xml = new XmlDocument();

            lock (xmlFiles)
            {
                xml.Load(xmlPath);

                foreach (XmlFile xmlFile in xmlFiles)
                {
                    string xPath = null;

                    xPath = string.Format("//service[@name='{0}']", xmlFile.ServiceName);
                    XmlNode node = xml.SelectSingleNode(xPath);
                    string schema = GetSchema(xmlFile.Filename);

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

                    // Remove previous load data
                    xPath = string.Format("{0}[@name='{1}']", schema, xmlFile.Filename);
                    XmlNode checkNode = node.SelectSingleNode(xPath);
                    if (checkNode != null)
                    {
                        node.RemoveChild(checkNode);
                    }

                    XmlElement newElem = xml.CreateElement(schema);
                    newElem.SetAttribute("name", xmlFile.Filename);

                    int hashCode = -1;
                    if (xmlFile.HttpResponse != null)
                    {
                        hashCode = xmlFile.HttpResponse.GetHashCode();
                    }

                    newElem.SetAttribute("hashCode", hashCode.ToString());
                    int noNamspaceHash = GetNoNamspaceHash(xmlFile);
                    newElem.SetAttribute("noNamspaceHashCode", noNamspaceHash.ToString());

                    if ((xmlFile.Error != null) && (xmlFile.Error != string.Empty))
                    {
                        newElem.SetAttribute("error_message", xmlFile.Error);
                        newElem.SetAttribute("status", "error");
                        newElem.SetAttribute("http_code", xmlFile.HttpResultCode.ToString());
                    }

                    node.AppendChild(newElem);
                }

                xml.Save(xmlPath);

                // Clear xml files added to metadata.xml
                xmlFiles.Clear();

            }
        }

        private static int GetNoNamspaceHash(XmlFile xmlFile)
        {
            int hash = -1;

            if (xmlFile.Error == string.Empty && xmlFile.XDocument != null)
            {
                try
                {
                    if (xmlFile.Filename.Contains(".wadl"))
                    {
                        CleanUpTag(xmlFile.XDocument, "application");
                    }
                    else if (xmlFile.Filename.Contains(".xsd"))
                    {
                        CleanUpTag(xmlFile.XDocument, "schema");
                        RemoveTags(xmlFile.XDocument, "import");
                    }
                    else if (xmlFile.Filename.Contains(".xml"))
                    {
                        CleanUpTag(xmlFile.XDocument, "collection");
                    }

                    string result = xmlFile.XDocument.ToString();
                    result = result.Replace(" ", "");
                    result = result.Replace("\r\n", "");
                    hash = result.GetHashCode();

                } catch (Exception ex)
                {
                    Logger.LogError(string.Format("Failed to calc NoNamspaceHash for service {0} file {1}", xmlFile.Filename, xmlFile.ServiceName));
                    Logger.LogError(ex.Message);
                    Logger.LogError(ex.StackTrace);
                }
            }

            return hash;
        }

        private static void CleanUpTag(XDocument doc, string name)
        {
            List<XElement> clearNodes = new List<XElement>();

            // Replace element with clear copy
            clearNodes.AddRange(
                from el in doc.Descendants()
                where el.Name.LocalName == name
                select el);

            foreach (XElement elem in clearNodes)
            {
                var childs = elem.Descendants();
                XElement newElem = new XElement(elem.Name.LocalName);
                newElem.Add(childs);
                elem.ReplaceWith(newElem);
            }

            // Clean up remaining namespaces created during node replacemant
            doc.Descendants()
              .Attributes()
              .Where(x => x.IsNamespaceDeclaration)
              .Remove();

            foreach (var elem in doc.Descendants())
            {
                elem.Name = elem.Name.LocalName;
            }
        }

        private static void RemoveTags(XDocument doc, string name)
        {
            doc.Descendants()
              .Elements()
              .Where(x => x.Name.LocalName == name)
              .Remove();
        }

        private static string GetSchema(string filename)
        {
            int i = filename.LastIndexOf(".");
            string extension = filename.Substring(i);
            string schema = "other_schema";

            if (extension == ".xsd")
            {
                schema = "data_schema";
            }
            else if (extension == ".wadl")
            {
                schema = "service_schema";
            }
            else if (filename == "query.xml")
            {
                schema = "query_schema";
            }

            return schema;
        }

        private static void FixFilename(ref string filename)
        {
            Regex regexSet = new Regex(@"([:*?\<>|])");
            filename = filename.Replace("\\", "&");
            filename = filename.Replace("/", "&");
            filename = regexSet.Replace(filename, "$");
            filename = filename.Replace(" ", "_");
        }
    }
}
