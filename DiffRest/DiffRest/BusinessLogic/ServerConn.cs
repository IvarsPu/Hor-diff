using System;
using System.Collections.Generic;
using System.Xml;
using Models;

namespace BusinessLogic
{
    public class ServerConn
    {
        public MetadataService GetServerConn(int id)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppInfo.path);
            XmlNode node = doc.SelectSingleNode("//MetadataServices/MetadataService[@ID='" + id + "']");
            if (node != null)
            {

                MetadataService metadataService = new MetadataService();
                metadataService.Id = Int32.Parse(node.Attributes["ID"].Value);
                metadataService.Name = node.Attributes["Name"].Value;
                metadataService.Url = node.Attributes["Url"].Value;
                metadataService.Username = node.Attributes["Username"].Value;
                metadataService.Password = node.Attributes["Password"].Value;
                return metadataService;
            }
            else
            {
                return null;
            }
        }

        public bool CreateServerConn(MetadataService service)
        {
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(AppInfo.path))
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

            metadataServiceNodes.AppendChild(metadataServiceNode);
            #endregion

            doc.Save(AppInfo.path);
            return true;
        }

        public void DeleteServerConn(int id)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppInfo.path);
            XmlNode node = doc.SelectSingleNode("//MetadataServices/MetadataService[@ID='" + id + "']");
            if (node != null)
            {
                node.ParentNode.RemoveChild(node);
                doc.Save(AppInfo.path);
            }
        }

        public bool EditServerConn(MetadataService service)
        {
            XmlDocument doc = new XmlDocument();
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
                    doc.Save(AppInfo.path);
                    return true;
                }
            }
            return false;
        }

        public List<MetadataService> GetMetadataServices()
        {
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(AppInfo.path))
            {
                doc.Load(AppInfo.path);
                List<MetadataService> metadataServices = new List<MetadataService>();
                foreach (XmlNode node in doc.SelectNodes("//MetadataService"))
                {
                    try
                    {
                        MetadataService metadataService = new MetadataService();
                        metadataService.Id = Int32.Parse(node.Attributes["ID"].Value);
                        metadataService.Name = node.Attributes["Name"].Value;
                        metadataService.Url = node.Attributes["Url"].Value;
                        metadataService.Username = node.Attributes["Username"].Value;
                        metadataService.Password = node.Attributes["Password"].Value;
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
