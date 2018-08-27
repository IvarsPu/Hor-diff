using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Newtonsoft.Json;
using RestLogService.Models;

namespace RestLogService.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpGet]
        public ActionResult GetServices()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Server.MapPath(@"~/rest_sample/Versions.xml"));

            List<string> str = new List<string>();

            foreach (XmlNode node in doc.ChildNodes[0].ChildNodes)
            {
                foreach (XmlNode nodes in node.ChildNodes)
                {
                    str.Add(node.Attributes["name"].Value + "/" + nodes.Attributes["name"].Value);
                }
            }

            return View(str);
        }

        public string List(string text)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Server.MapPath(@"~/rest_sample/" + text + "/metadata.xml"));
            
            return JsonConvert.SerializeXmlNode(doc);
        }
    }
}
