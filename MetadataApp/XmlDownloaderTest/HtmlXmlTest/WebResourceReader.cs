﻿namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Xml;

    internal class WebResourceReader
    {
        public static List<Link> MainReader(string url, ref string rootLocalPath, ref string metadataPath)
        {
            List<Link> links = new List<Link>();
            WebResourceLoader webResourceLoader = new WebResourceLoader();
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
            xmlIO.CreateFolder(localPath);

            metadataPath = localPath + "metadata.xml";
            XmlWriter xmlWriter = XmlWriter.Create(metadataPath);
            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement("rest_api_metadata");
            xmlWriter.WriteAttributeString("release", release);
            xmlWriter.WriteAttributeString("version", version);

            webResourceLoader = new WebResourceLoader();
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

                            xmlIO.CreateFolder(localPath);
                            //Console.WriteLine(localPath);

                            xmlWriter.WriteStartElement("service_group");
                            xmlWriter.WriteAttributeString("name", desc);
                        }
                        else if (xmlReader.Name == "link")
                        {
                            Link tempLink = new Link();
                            while (xmlReader.NodeType != XmlNodeType.Text)
                            {
                                xmlReader.Read();
                            }

                            tempLink.Href = xmlReader.Value;

                            xmlReader.Read();

                            while (xmlReader.NodeType != XmlNodeType.Text)
                            {
                                xmlReader.Read();
                            }

                            tempLink.Description = xmlReader.Value;

                            tempLink.Filepath = localPath + tempLink.Href.Substring(5) + "//";

                            links.Add(tempLink);

                            string name = tempLink.Href;
                            int i = name.LastIndexOf("/");
                            name = name.Substring(i + 1);

                            xmlWriter.WriteStartElement("service");
                            xmlWriter.WriteAttributeString("description", tempLink.Description);
                            xmlWriter.WriteAttributeString("name", name);
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

            return links;
        }

        private static void FixFilename(ref string filename)
        {
            Regex regexSet = new Regex(@"([:*?\<>|])");
            filename = filename.Replace("\\", "&");
            filename = filename.Replace("/", "&");
            filename = regexSet.Replace(filename, "$");
            filename = filename.Replace(" ", "_");
        }

        public static void AddFilesToXml(string xmlPath, List<XmlFile> xmlFiles)
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
                if (IsXSD(xmlFile.Filename))
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

        private static bool IsXSD(string filename)
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
