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
        public int GetUser(string url, string password)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.ConnectionStrings["LocalFolder"].ConnectionString));
                XmlNode node = doc.SelectSingleNode("//Users/User[@Url='" + url + "' and @Password = '" + password + "']");
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
        public int CreateUser(string url, string password)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.ConnectionStrings["LocalFolder"].ConnectionString));
                if (doc.SelectSingleNode("//Users/User[@Url='" + url + "']") != null)
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

                XmlAttribute Url = doc.CreateAttribute("Url");
                Url.Value = url;
                userNode.Attributes.SetNamedItem(Url);

                XmlAttribute Password = doc.CreateAttribute("Password");
                Password.Value = password;
                userNode.Attributes.SetNamedItem(Password);

                userNodes.AppendChild(userNode);
                #endregion

                doc.Save(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.ConnectionStrings["LocalFolder"].ConnectionString));
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
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.ConnectionStrings["LocalFolder"].ConnectionString));
                XmlNode node = doc.SelectSingleNode("//Users/User[@ID='" + Session["userId"] + "']");
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                    doc.Save(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.ConnectionStrings["LocalFolder"].ConnectionString));
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
        public bool UpdateUser(string url, string password)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.ConnectionStrings["LocalFolder"].ConnectionString));
                XmlNode node = doc.SelectSingleNode("//Users/User[@ID='" + Session["userId"].ToString() + "']");
                if (node != null)
                {
                    if (node.Attributes["Url"].Value.Equals(url) && doc.SelectNodes("//Users/User[@Url='" + url + "']").Count <2)
                    {
                        node.Attributes["Url"].Value = url;
                        node.Attributes["Password"].Value = password;
                        doc.Save(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.ConnectionStrings["LocalFolder"].ConnectionString));
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
