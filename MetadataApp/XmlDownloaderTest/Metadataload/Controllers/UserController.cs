using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using Metadataload.Models;

namespace Metadataload.Controllers
{
    [RoutePrefix("User")]
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public bool CreateUser(string username, string password, string name)
        {
            User user = new User(username, password)
            {
                Name = name
            };
            return AddUserXml(user);
        }

        public User GetUser(string username, string password)
        {
            return new User(username, password);
        }

        public User UpdateUser(string username, string password)
        {
            return new User(username, password);
        }

        public User DeleteUser(string username, string password)
        {
            return new User(username, password);
        }

        private bool AddUserXml(User user)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load("C:/Users/ralfs.zangis/Desktop/test.xml");
                if (doc.SelectSingleNode("//Users/User/Username[.='" + user.Username + "']") != null)
                {
                    return false;
                }
            }
            catch
            {
                doc.AppendChild(doc.CreateElement("Users"));
            }

            //Create a new element
            XmlNode userNodes = doc.SelectSingleNode("//Users");
            #region user
            XmlNode userNode = doc.CreateElement("User");
            XmlNode Username = doc.CreateElement("Username");
            Username.InnerText = user.Username;
            userNode.AppendChild(Username);

            XmlNode Password = doc.CreateElement("Password");
            Password.InnerText = user.Password;
            userNode.AppendChild(Password);

            XmlNode Name = doc.CreateElement("Name");
            Name.InnerText = user.Name;
            userNode.AppendChild(Name);

            userNodes.AppendChild(userNode);
            #endregion

            doc.Save("C:/Users/ralfs.zangis/Desktop/test.xml");

            return true;
        }
    }
}
