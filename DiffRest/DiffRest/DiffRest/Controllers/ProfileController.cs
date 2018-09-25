using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml;
using DiffRest.Models;

namespace DiffRest.Controllers
{
    [RoutePrefix("Profile")]
    public class ProfileController : Controller
    {
        public static string path;
        public static string profileId = "profileId";

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("GetProfiles")]
        public List<Profile> GetProfiles()
        {
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                doc.Load(path);
                List<Profile> profiles = new List<Profile>();
                foreach (XmlNode node in doc.SelectNodes("//Profile"))
                {
                    try
                    {
                        Profile profile = new Profile();
                        profile.Id = Int32.Parse(node.Attributes["ID"].Value);
                        profile.Name = node.Attributes["Name"].Value;
                        profile.Url = node.Attributes["Url"].Value;
                        profile.Username = node.Attributes["Username"].Value;
                        profile.Password = node.Attributes["Password"].Value;
                        profiles.Add(profile);
                    }
                    catch { }
                }
                return profiles;
            }
            return null;
        }

        [HttpGet]
        [Route("CreateProfile")]
        public int CreateProfile(string name, string url, string username, string password)
        {
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                doc.Load(path);
                if (doc.SelectSingleNode("//Profiles/Profile[@Url='" + url + "']") != null)
                {
                    return 0;
                }
            }
            else
            {
                doc.AppendChild(doc.CreateElement("Profiles"));
            }

            #region Profile
            XmlNode profileNodes = doc.SelectSingleNode("//Profiles");
            XmlNode profileNode = doc.CreateElement("Profile");

            int id = 1;
            if (profileNodes.LastChild != null)
            {
                id = Int32.Parse(profileNodes.LastChild.Attributes["ID"].Value) + 1;
            }
            XmlAttribute ID = doc.CreateAttribute("ID");
            ID.Value = id.ToString();
            profileNode.Attributes.SetNamedItem(ID);

            XmlAttribute Name = doc.CreateAttribute("Name");
            Name.Value = name;
            profileNode.Attributes.SetNamedItem(Name);

            XmlAttribute Url = doc.CreateAttribute("Url");
            Url.Value = url;
            profileNode.Attributes.SetNamedItem(Url);

            XmlAttribute Username = doc.CreateAttribute("Username");
            Username.Value = username;
            profileNode.Attributes.SetNamedItem(Username);

            XmlAttribute Password = doc.CreateAttribute("Password");
            Password.Value = password;
            profileNode.Attributes.SetNamedItem(Password);

            profileNodes.AppendChild(profileNode);
            #endregion

            doc.Save(path);
            return id;
        }

        [HttpGet]
        [Route("DeleteProfile")]
        public bool DeleteProfile(int id)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode node = doc.SelectSingleNode("//Profiles/Profile[@ID='" + id + "']");
            if (node != null)
            {
                node.ParentNode.RemoveChild(node);
                doc.Save(path);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        [HttpGet]
        [Route("UpdateProfile")]
        public bool UpdateProfile(int id, string name, string url, string username, string password)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode node = doc.SelectSingleNode("//Profiles/Profile[@ID='" + id + "']");
            if (node != null)
            {
                if ((node.Attributes["Url"].Value.Equals(url) && doc.SelectNodes("//Profiles/Profile[@Url='" + url + "']").Count < 2) ||
                    (!node.Attributes["Url"].Value.Equals(url) && doc.SelectNodes("//Profiles/Profile[@Url='" + url + "']").Count < 1))
                {
                    node.Attributes["Name"].Value = name;
                    node.Attributes["Url"].Value = url;
                    node.Attributes["Username"].Value = username;
                    node.Attributes["Password"].Value = password;
                    doc.Save(path);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
