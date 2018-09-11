using System;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml;

namespace Metadataload.Controllers
{
    [RoutePrefix("Profile")]
    public class ProfileController : Controller
    {
        public ActionResult LogIn()
        {
            Session["profileId"] = "";
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
        [Route("GetProfile")]
        public int GetProfile(string url, string password)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.ConnectionStrings["LocalFolder"].ConnectionString));
                XmlNode node = doc.SelectSingleNode("//Profiles/Profile[@Url='" + url + "' and @Password = '" + password + "']");
                if (node != null)
                {
                    int id = Int32.Parse(node.Attributes["ID"].Value);
                    Session["profileId"] = id;
                    return id;
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
        [Route("CreateProfile")]
        public int CreateProfile(string url, string password)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.ConnectionStrings["LocalFolder"].ConnectionString));
                if (doc.SelectSingleNode("//Profiles/Profile[@Url='" + url + "']") != null)
                {
                    return 0;
                }
            }
            catch
            {
                doc.AppendChild(doc.CreateElement("Profiles"));
            }

            try
            {
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

                XmlAttribute Url = doc.CreateAttribute("Url");
                Url.Value = url;
                profileNode.Attributes.SetNamedItem(Url);

                XmlAttribute Password = doc.CreateAttribute("Password");
                Password.Value = password;
                profileNode.Attributes.SetNamedItem(Password);

                profileNodes.AppendChild(profileNode);
                #endregion

                doc.Save(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.ConnectionStrings["LocalFolder"].ConnectionString));
                Session["profileId"] = id;
                return id;
            }
            catch
            {
                return 0;
            }
        }

        [HttpGet]
        [Route("DeleteProfile")]
        public bool DeleteProfile()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.ConnectionStrings["LocalFolder"].ConnectionString));
                XmlNode node = doc.SelectSingleNode("//Profiles/Profile[@ID='" + Session["profileId"] + "']");
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
        [Route("UpdateProfile")]
        public bool UpdateProfile(string url, string password)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.ConnectionStrings["LocalFolder"].ConnectionString));
                XmlNode node = doc.SelectSingleNode("//Profiles/Profile[@ID='" + Session["profileId"].ToString() + "']");
                if (node != null)
                {
                    if ((node.Attributes["Url"].Value.Equals(url) && doc.SelectNodes("//Profiles/Profile[@Url='" + url + "']").Count <2)|| 
                        (!node.Attributes["Url"].Value.Equals(url) && doc.SelectNodes("//Profiles/Profile[@Url='" + url + "']").Count < 1))
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
