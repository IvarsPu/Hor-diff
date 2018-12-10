using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using Models;
using AppContext = Models.AppContext;
using System.Text;
using System.Security.Cryptography;

namespace BusinessLogic
{
    internal class XmlMetadata
    {
        bool disposed = false;
        private AppContext appContext;
        private Logger Logger;
        private string prevRelease;
        private string prevVersionMetaXmlPath;
        private XmlDocument prevVersionMetaXml;
        private MD5 md5Hash;

        public XmlMetadata(AppContext appContext)
        {
            this.appContext = appContext;
            this.Logger = (Logger) this.appContext.Logger;
            md5Hash = MD5.Create();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                md5Hash.Dispose();
            }

            disposed = true;
        }


        public List<RestService> InitServiceMetadata(WebResourceLoader webResourceLoader)
        {
            List<RestService> services = new List<RestService>();
            WebResponse myResponse = webResourceLoader.GetResponseFromSite(appContext.RootUrl + "global/agentVersion").Result;
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
            this.appContext.Release = value.Substring(index + 1);
            value = value.Remove(index);
            index = value.LastIndexOf(".");
            this.appContext.Version = value.Substring(index + 1);

            this.appContext.ReleaseLocalPath = appContext.RootLocalPath + this.appContext.Version + "\\" + this.appContext.Release + "\\";

            XmlIO xmlIO = new XmlIO();
            XmlIO.CreateFolder(appContext.ReleaseLocalPath);

            appContext.MetaFilePath = appContext.ReleaseLocalPath + "metadata.xml";

            XmlWriter xmlWriter;

            if (File.Exists(appContext.MetaFilePath))
            {
                // do not owerwrite existing metadata file, write just in memory
                StringBuilder builder = new StringBuilder();
                xmlWriter = XmlWriter.Create(builder);
            }
            else
            {
                xmlWriter = XmlWriter.Create(appContext.MetaFilePath);
            }

            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement("rest_api_metadata");
            xmlWriter.WriteAttributeString("release", this.appContext.Release);
            xmlWriter.WriteAttributeString("version", this.appContext.Version);

            myResponse = webResourceLoader.GetResponseFromSite(appContext.RootUrl).Result;
            myStream = myResponse.GetResponseStream();
            xmlReader = XmlReader.Create(myStream);
            string localPath = appContext.ReleaseLocalPath;

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
                        break;
                    case XmlNodeType.Text:
                        break;
                    case XmlNodeType.EndElement:
                        if (xmlReader.Name == "group")
                        {
                            var lastFolder = Path.GetDirectoryName(localPath);
                            var pathWithoutLastFolder = Path.GetDirectoryName(lastFolder);
                            localPath = pathWithoutLastFolder + "\\";

                            xmlWriter.WriteEndElement();
                        }
                        break;
                }
            }

            xmlReader.Close();
            myResponse.Close();

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();


            this.DetectAndInitPreviousRelease();
            return services;
        }

        public void AddFilesToXml(List<XmlFile> xmlFiles)
        {
            XmlDocument xml = new XmlDocument();

            lock (xmlFiles)
            {
                xml.Load(this.appContext.MetaFilePath);

                foreach (XmlFile xmlFile in xmlFiles)
                {
                    string xPath = null;
                    string fileXPath = null;

                    xPath = string.Format("//service[@name='{0}']", xmlFile.ServiceName);
                    fileXPath = xPath;
                    XmlNode node = xml.SelectSingleNode(xPath);
                    string schema = GetSchema(xmlFile.Filename);

                    if (xmlFile.Attachment)
                    {
                        node = xml.SelectSingleNode(xPath + "/resource");
                        fileXPath += "/resource";
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
                    fileXPath = fileXPath + "/" + xPath;
                    XmlNode checkNode = node.SelectSingleNode(xPath);
                    if (checkNode != null)
                    {
                        node.RemoveChild(checkNode);
                    }

                    XmlElement newElem = xml.CreateElement(schema);
                    newElem.SetAttribute("name", xmlFile.Filename);
                    
                    string hashCode = "";
                    if (String.IsNullOrEmpty(xmlFile.Error) && xmlFile.XDocument != null)
                    {
                        CleanUpXmlDoc(xmlFile);
                        hashCode = GetMd5Hash(xmlFile.XDocument.ToString());
                    }

                    // If hash is equal to previous release hash, delete file and refer to previous release in metadata
                    // otherwise store it under currrent release
                    string release = GetFileStoredRelease(xmlFile, prevVersionMetaXml, fileXPath, hashCode);
                    newElem.SetAttribute("stored_release", release);

                    newElem.SetAttribute("hashCode", hashCode);
                    string noNamspaceHash = GetNoNamspaceHash(xmlFile);
                    newElem.SetAttribute("noNamspaceHashCode", noNamspaceHash);

                    if ((xmlFile.Error != null) && (xmlFile.Error != string.Empty))
                    {
                        newElem.SetAttribute("error_message", xmlFile.Error);
                        newElem.SetAttribute("status", "error");
                        newElem.SetAttribute("http_code", xmlFile.HttpResultCode.ToString());
                    }

                    node.AppendChild(newElem);
                }

                xml.Save(this.appContext.MetaFilePath);

                // Clear xml files added to metadata.xml
                xmlFiles.Clear();

            }

        }

        // Detect and initialize previous version release to avoid storing equal file dublicates
        private void DetectAndInitPreviousRelease()
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(this.GetGlobalVerionFilePath());

                string xpath = string.Format("//version[@name={0}]", this.appContext.Version);
                XmlNode root = xml.SelectSingleNode(xpath);

                if (root == null)
                {
                    Logger.LogInfo("Previous release for file reuse was not found");
                    return;
                }

                XmlNodeList releaseNodes = root.SelectNodes("release/@name");

                List<int> releases = new List<int>();
                foreach (XmlNode el in releaseNodes)
                {
                    releases.Add(Convert.ToInt32(el.Value));
                }
                releases.Sort();
                int lastUsableRelease = -1;
                int currentRelease = Convert.ToInt32(this.appContext.Release);

                for (int i = releases.Count-1; i >= 0; i--)
                {
                    if (releases[i] < currentRelease)
                    {
                        lastUsableRelease = releases[i];
                        break;
                    }
                }

                if (lastUsableRelease < 0)
                {
                    Logger.LogInfo("Previous release for file reuse was not found");
                    return;
                }

                // Load prev metadata file
                this.prevRelease = lastUsableRelease.ToString();
                this.prevVersionMetaXmlPath = this.appContext.RootLocalPath + this.appContext.Version + "\\" + prevRelease.ToString() + "\\metadata.xml";


                this.prevVersionMetaXml = new XmlDocument();
                if (this.prevVersionMetaXmlPath != null)
                {
                    prevVersionMetaXml.Load(this.prevVersionMetaXmlPath);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to get previous release for file reuse ");
                Logger.LogError(ex.Message);
                Logger.LogError(ex.StackTrace);
            }
        }

        private string GetGlobalVerionFilePath()
        {
            return this.appContext.RootLocalPath + "Versions.xml";
        }

        private string GetFileStoredRelease(XmlFile xmlFile, XmlDocument prevVersionMetaXml, string fileXPath, string hashCode)
        {
            string release = this.appContext.Release;

            if (prevVersionMetaXml != null && !String.IsNullOrEmpty(hashCode))
            {
                string xPath = fileXPath + "/@hashCode";
                XmlNode node = prevVersionMetaXml.SelectSingleNode(xPath);
                if (node != null)
                {
                    string prevVerHashCode = node.Value;
                    if (hashCode.Equals(prevVerHashCode))
                    {
                        // Refer xml file to this release and remove loaded file
                        xPath = fileXPath + "/@stored_release";
                        node = prevVersionMetaXml.SelectSingleNode(xPath);
                        if (node == null)
                        {
                            // Just refer to prev release
                            release = this.prevRelease;
                        }
                        else
                        {
                            // Could be older release
                            release = node.Value;
                        }

                        File.Delete(xmlFile.LocalPath + xmlFile.Filename);
                    }
                }
            }

            return release;
        }

        private string GetNoNamspaceHash(XmlFile xmlFile)
        {
            string hash = "";

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
                    else if (xmlFile.Filename.Contains("query.xml"))
                    {
                        CleanUpTag(xmlFile.XDocument, "collection");
                    }
                    else if (xmlFile.Filename.Contains("template.xml"))
                    {
                        CleanUpTag(xmlFile.XDocument, "entity");
                    }

                    hash = GetMd5Hash(xmlFile.XDocument.ToString());

                }
                catch (Exception ex)
                {
                    Logger.LogError(string.Format("Failed to calc NoNamspaceHash for service {0} file {1}", xmlFile.Filename, xmlFile.ServiceName));
                    Logger.LogError(ex.Message);
                    Logger.LogError(ex.StackTrace);
                }
            }

            return hash;
        }

        private void CleanUpTag(XDocument doc, string name)
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

        private void RemoveTags(XDocument doc, string name)
        {
            doc.Descendants()
              .Elements()
              .Where(x => x.Name.LocalName == name)
              .Remove();
        }
                
        private string GetSchema(string filename)
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
            else if (filename == "template.xml")
            {
                schema = "template_schema";
            }

            return schema;
        }

        private void FixFilename(ref string filename)
        {
            Regex regexSet = new Regex(@"([:*?\<>|])");
            filename = filename.Replace("\\", "&");
            filename = filename.Replace("/", "&");
            filename = regexSet.Replace(filename, "$");
            filename = filename.Replace(" ", "_");
        }

        public void AddReleaseToVersionXmlFile(ServiceLoadState serviceState)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(this.GetGlobalVerionFilePath());
            XmlNode version = xml.SelectSingleNode(string.Format("//versions/version[@name={0}]", this.appContext.Version));

            if (version == null)
            {
                XmlElement versionEl = xml.CreateElement("version");
                versionEl.SetAttribute("name", this.appContext.Version);
                XmlNode versions = xml.SelectSingleNode("//versions");
                versions.AppendChild(versionEl);
                version = versionEl;
            }

            XmlNode release = version.SelectSingleNode(string.Format("release[@name={0}]", this.appContext.Release));

            if (release != null)
            {
                release.ParentNode.RemoveChild(release);
            }
            
            XmlElement releaseEl = xml.CreateElement("release");
            releaseEl.SetAttribute("name", this.appContext.Release);

            releaseEl.SetAttribute("pendingLoad", Convert.ToString(serviceState.NotLoaded));
            releaseEl.SetAttribute("loaded", Convert.ToString(serviceState.Loaded + serviceState.LoadedWithErrors));
            releaseEl.SetAttribute("loadedWithErrors", Convert.ToString(serviceState.LoadedWithErrors));
            releaseEl.SetAttribute("failed", Convert.ToString(serviceState.Failed));

            version.AppendChild(releaseEl);
            
            xml.Save(this.GetGlobalVerionFilePath());
        }

        private string GetMd5Hash(string input)
        {
            string content = input.Replace(" ", "");
            content = content.Replace("\r\n", "");
            content = content.Replace("\t", "");

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public void CleanUpXmlDoc(XmlFile xmlFile)
        {
            if (xmlFile.Filename == "query.xml")
            {
                CleanUpQuery(xmlFile.XDocument);
            }
            else if (xmlFile.Filename == "template.xml")
            {
                CleanUpTemplate(xmlFile.XDocument);
            }
            else if (xmlFile.Filename.Contains(".xsd"))
            {
                CleanUpXsd(xmlFile.XDocument);
            }
        }

        private void CleanUpTemplate(XDocument doc)
        {
            List<XElement> clearNodes = new List<XElement>();

            // Replace element with clear copy
            clearNodes.AddRange(
                from el in doc.Descendants()
                where el.Value != null
                select el);

            foreach (XElement elem in clearNodes)
            {
                elem.SetValue("");
            }
        }

        private void CleanUpQuery(XDocument doc)
        {
            List<XElement> clearNodes = new List<XElement>();

            // Replace element with clear copy
            clearNodes.AddRange(
                from el in doc.Descendants()
                where el.Name == "title"
                select el);

            foreach (XElement elem in clearNodes)
            {
                elem.SetValue("");
            }
        }

        private void CleanUpXsd(XDocument doc)
        {
            doc.Descendants()
              .Elements()
              .Where(x => x.Name.LocalName == "annotation")
              .Remove();
        }
        
    }
}