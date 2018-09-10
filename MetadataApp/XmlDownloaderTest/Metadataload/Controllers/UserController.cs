using System;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml;

namespace Metadataload.Controllers
{
    [RoutePrefix("User")]
    public class UserController : Controller
    {
        public ActionResult LogIn()
        {
            Session["userId"] = "";
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Update()
        {
            return View();
        }

        [HttpGet]
        [Route("GetUser")]
        public int GetUser(string username, string password)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["LocalFolder"].ToString()));
                XmlNode node = doc.SelectSingleNode("//Users/User[@Username='" + username + "' and @Password = '" + password + "']");
                if (node != null)
                {
                    Session["userId"] = Int32.Parse(node.Attributes["ID"].Value);
                    return Int32.Parse(node.Attributes["ID"].Value);
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        [HttpGet]
        [Route("CreateUser")]
        public int CreateUser(string username, string password, string name)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["LocalFolder"].ToString()));
                if (doc.SelectSingleNode("//Users/User[@Username='" + username + "']") != null)
                {
                    return 0;
                }
            }
            catch
            {
                doc.AppendChild(doc.CreateElement("Users"));
            }

            try
            {
                #region user
                XmlNode userNodes = doc.SelectSingleNode("//Users");
                XmlNode userNode = doc.CreateElement("User");

                int id = 1;
                if (userNodes.LastChild != null)
                {
                    id = Int32.Parse(userNodes.LastChild.Attributes["ID"].Value) + 1;
                }
                XmlAttribute ID = doc.CreateAttribute("ID");
                ID.Value = id.ToString();
                userNode.Attributes.SetNamedItem(ID);

                XmlAttribute Username = doc.CreateAttribute("Username");
                Username.Value = username;
                userNode.Attributes.SetNamedItem(Username);

                XmlAttribute Password = doc.CreateAttribute("Password");
                Password.Value = password;
                userNode.Attributes.SetNamedItem(Password);

                XmlAttribute Name = doc.CreateAttribute("Name");
                Name.Value = name;
                userNode.Attributes.SetNamedItem(Name);

                userNodes.AppendChild(userNode);
                #endregion

                doc.Save(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["LocalFolder"].ToString()));
                Session["userId"] = id;
                return id;
            }
            catch
            {
                return 0;
            }
        }

        [HttpGet]
        [Route("DeleteUser")]
        public bool DeleteUser()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["LocalFolder"].ToString()));
                XmlNode node = doc.SelectSingleNode("//Users/User[@ID='" + Session["userId"] + "']");
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                    doc.Save(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["LocalFolder"].ToString()));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        
        [HttpGet]
        [Route("UpdateUser")]
        public bool UpdateUser(string username, string password, string name)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["LocalFolder"].ToString()));
                XmlNode node = doc.SelectSingleNode("//Users/User[@ID='" + Session["userId"] + "']");
                if (node != null)
                {
                    if (node.Attributes["Username"].Value.Equals(username) && doc.SelectNodes("//Users/User[@Username='" + username + "']").Count <2)
                    {
                        node.Attributes["Username"].Value = username;
                        node.Attributes["Password"].Value = password;
                        node.Attributes["Name"].Value = name;
                        doc.Save(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["LocalFolder"].ToString()));
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
            catch
            {
                return false;
            }
        }
    }
}
