using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Models;

namespace BusinessLogic
{
    public class Connection
    {
        public RestConnection GetServerConn(int id)
        {
            XmlDocument doc = new XmlDocument();
            if (!File.Exists(AppInfo.path))
            {
                return null;
            }
            doc.Load(AppInfo.path);
            XmlNode node = doc.SelectSingleNode("//MetadataServices/MetadataService[@ID='" + id + "']");
            if (node != null)
            {

                RestConnection metadataService = new RestConnection();
                metadataService.Id = Int32.Parse(node.Attributes["ID"].Value);
                metadataService.Name = node.Attributes["Name"].Value;
                metadataService.Url = node.Attributes["Url"].Value;
                metadataService.Username = node.Attributes["Username"].Value;
                metadataService.Password = node.Attributes["Password"].Value;
               

                try
                {
                    metadataService.LoadQuery = Convert.ToBoolean(node.Attributes["LoadQuery"].Value);
                    metadataService.LoadTemplate = Convert.ToBoolean(node.Attributes["LoadTemplate"].Value);
                    metadataService.ParallelThreads = Convert.ToInt32(node.Attributes["ParallelThreads"].Value);
                }
                catch (Exception) { }

                return metadataService;
            }
            else
            {
                return null;
            }
        }

        public bool CreateServerConn(RestConnection service)
        {
            XmlDocument doc = new XmlDocument();
            if (File.Exists(AppInfo.path))
            {
                doc.Load(AppInfo.path);
                if (doc.SelectSingleNode("//MetadataServices/MetadataService[@Url='" + service.Url + "']") != null)
                {
                    return false;
                }
            }
            else
            {
                doc.AppendChild(doc.CreateElement("MetadataServices"));
            }

            #region MetadataService
            XmlNode metadataServiceNodes = doc.SelectSingleNode("//MetadataServices");
            XmlNode metadataServiceNode = doc.CreateElement("MetadataService");

            int id = 1;
            if (metadataServiceNodes.LastChild != null)
            {
                id = Int32.Parse(metadataServiceNodes.LastChild.Attributes["ID"].Value) + 1;
            }
            XmlAttribute ID = doc.CreateAttribute("ID");
            ID.Value = id.ToString();
            metadataServiceNode.Attributes.SetNamedItem(ID);

            XmlAttribute Name = doc.CreateAttribute("Name");
            Name.Value = service.Name;
            metadataServiceNode.Attributes.SetNamedItem(Name);

            XmlAttribute Url = doc.CreateAttribute("Url");
            Url.Value = service.Url;
            metadataServiceNode.Attributes.SetNamedItem(Url);

            XmlAttribute Username = doc.CreateAttribute("Username");
            Username.Value = service.Username;
            metadataServiceNode.Attributes.SetNamedItem(Username);

            XmlAttribute Password = doc.CreateAttribute("Password");
            Password.Value = service.Password;
            metadataServiceNode.Attributes.SetNamedItem(Password);

            XmlAttribute LoadTemplate = doc.CreateAttribute("LoadTemplate");
            LoadTemplate.Value = Convert.ToString(service.LoadTemplate);
            metadataServiceNode.Attributes.SetNamedItem(LoadTemplate);

            XmlAttribute LoadQuery = doc.CreateAttribute("LoadQuery");
            LoadQuery.Value = Convert.ToString(service.LoadQuery);
            metadataServiceNode.Attributes.SetNamedItem(LoadQuery);

            XmlAttribute ParallelThreads = doc.CreateAttribute("ParallelThreads");
            ParallelThreads.Value = Convert.ToString(service.ParallelThreads);
            metadataServiceNode.Attributes.SetNamedItem(ParallelThreads);

            metadataServiceNodes.AppendChild(metadataServiceNode);
            #endregion

            doc.Save(AppInfo.path);
            return true;
        }

        public bool DeleteServerConn(int id)
        {
            XmlDocument doc = new XmlDocument();
            if (!File.Exists(AppInfo.path))
            {
                return false;
            }
            doc.Load(AppInfo.path);
            XmlNode node = doc.SelectSingleNode("//MetadataServices/MetadataService[@ID='" + id + "']");
            if (node != null)
            {
                node.ParentNode.RemoveChild(node);
                doc.Save(AppInfo.path);
                return true;
            }
            return false;
        }

        public bool EditServerConn(RestConnection service)
        {
            XmlDocument doc = new XmlDocument();
            if (!File.Exists(AppInfo.path))
            {
                return false;
            }
            doc.Load(AppInfo.path);
            XmlNode node = doc.SelectSingleNode("//MetadataServices/MetadataService[@ID='" + service.Id + "']");
            if (node != null)
            {
                if ((node.Attributes["Url"].Value.Equals(service.Url) && doc.SelectNodes("//MetadataServices/MetadataService[@Url='" + service.Url + "']").Count < 2) ||
                    (!node.Attributes["Url"].Value.Equals(service.Url) && doc.SelectNodes("//MetadataServices/MetadataService[@Url='" + service.Url + "']").Count < 1))
                {
                    node.Attributes["Name"].Value = service.Name;
                    node.Attributes["Url"].Value = service.Url;
                    node.Attributes["Username"].Value = service.Username;
                    node.Attributes["Password"].Value = service.Password;

                    if(node.Attributes.GetNamedItem("LoadQuery") == null)
                    {
                        XmlAttribute LoadTemplate = doc.CreateAttribute("LoadTemplate");
                        node.Attributes.SetNamedItem(LoadTemplate);

                        XmlAttribute LoadQuery = doc.CreateAttribute("LoadQuery");
                        node.Attributes.SetNamedItem(LoadQuery);

                        XmlAttribute ParallelThreads = doc.CreateAttribute("ParallelThreads");
                        node.Attributes.SetNamedItem(ParallelThreads);                        
                    }

                    if (node.Attributes.GetNamedItem("ParallelThreads") == null)
                    {
                        XmlAttribute ParallelThreads = doc.CreateAttribute("ParallelThreads");
                        node.Attributes.SetNamedItem(ParallelThreads);
                    }
                    node.Attributes["LoadQuery"].Value = Convert.ToString(service.LoadQuery);
                    node.Attributes["LoadTemplate"].Value = Convert.ToString(service.LoadTemplate);
                    node.Attributes["ParallelThreads"].Value = Convert.ToString(service.ParallelThreads);

                    doc.Save(AppInfo.path);
                    return true;
                }
            }
            return false;
        }

        public List<RestConnection> GetConnections()
        {
            XmlDocument doc = new XmlDocument();
            if (File.Exists(AppInfo.path))
            {
                doc.Load(AppInfo.path);
                List<RestConnection> metadataServices = new List<RestConnection>();
                foreach (XmlNode node in doc.SelectNodes("//MetadataService"))
                {
                    try
                    {
                        RestConnection metadataService = new RestConnection();
                        metadataService.Id = Int32.Parse(node.Attributes["ID"].Value);
                        metadataService.Name = node.Attributes["Name"].Value;
                        metadataService.Url = node.Attributes["Url"].Value;
                        metadataService.Username = node.Attributes["Username"].Value;
                        metadataService.Password = node.Attributes["Password"].Value;

                        try
                        {   // not available?
                            metadataService.LoadQuery = Convert.ToBoolean(node.Attributes["LoadQuery"].Value);
                            metadataService.LoadTemplate = Convert.ToBoolean(node.Attributes["LoadTemplate"].Value);
                            metadataService.ParallelThreads = Convert.ToInt32(node.Attributes["ParallelThreads"].Value);                            
                        }
                        catch (Exception) { }
                        metadataServices.Add(metadataService);
                    }
                    catch { }
                }
                return metadataServices;
            }
            return null;
        }
    }
}
