using System;
using System.Web.Mvc;
using System.Xml;
using DiffRest.Models;

namespace DiffRest.Controllers
{
    [RoutePrefix("MetadataService")]
    public class MetadataServiceController : Controller
    {
        public static string path;

        public ActionResult Index()
        {
            return View();
        }
        
        [Route("CreateMetadataService")]
        public int CreateMetadataService(string name, string url, string username, string password)
        {
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                doc.Load(path);
                if (doc.SelectSingleNode("//MetadataServices/MetadataService[@Url='" + url + "']") != null)
                {
                    return 0;
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
            Name.Value = name;
            metadataServiceNode.Attributes.SetNamedItem(Name);

            XmlAttribute Url = doc.CreateAttribute("Url");
            Url.Value = url;
            metadataServiceNode.Attributes.SetNamedItem(Url);

            XmlAttribute Username = doc.CreateAttribute("Username");
            Username.Value = username;
            metadataServiceNode.Attributes.SetNamedItem(Username);

            XmlAttribute Password = doc.CreateAttribute("Password");
            Password.Value = password;
            metadataServiceNode.Attributes.SetNamedItem(Password);

            metadataServiceNodes.AppendChild(metadataServiceNode);
            #endregion

            doc.Save(path);
            return id;
        }
        
        [Route("DeleteMetadataService")]
        public void DeleteMetadataService(int id)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode node = doc.SelectSingleNode("//MetadataServices/MetadataService[@ID='" + id + "']");
            if (node != null)
            {
                node.ParentNode.RemoveChild(node);
                doc.Save(path);
            }
        }
        
        [Route("UpdateMetadataService")]
        public void UpdateMetadataService(int id, string name, string url, string username, string password)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode node = doc.SelectSingleNode("//MetadataServices/MetadataService[@ID='" + id + "']");
            if (node != null)
            {
                if ((node.Attributes["Url"].Value.Equals(url) && doc.SelectNodes("//MetadataServices/MetadataService[@Url='" + url + "']").Count < 2) ||
                    (!node.Attributes["Url"].Value.Equals(url) && doc.SelectNodes("//MetadataServices/MetadataService[@Url='" + url + "']").Count < 1))
                {
                    node.Attributes["Name"].Value = name;
                    node.Attributes["Url"].Value = url;
                    node.Attributes["Username"].Value = username;
                    node.Attributes["Password"].Value = password;
                    doc.Save(path);
                }
            }
        }
        
        internal static MetadataService GetMetadataService(int id)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
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
    }
}
