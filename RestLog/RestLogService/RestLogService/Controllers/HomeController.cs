using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
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

            TreeNode treeNode = null;
            foreach (XmlNode node in doc)
            {
                treeNode = AddBranch(node, new TreeNode(node.Name));
            }

            return View(treeNode);
        }
        
        public string List(string text)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Server.MapPath(@"~/rest_sample/" + text + "/metadata.xml"));
            return doc.ChildNodes[1].Attributes["version"].Value + " _ " + doc.ChildNodes[1].Attributes["release"].Value;
        }

        private TreeNode AddBranch(XmlNode service_group, TreeNode branch)
        {
            foreach (XmlNode node in service_group.ChildNodes)
            {
                if (node.ChildNodes.Count > 0)
                {
                    branch.Add(AddBranch(node, new TreeNode(node.Attributes["name"].Value)));
                }
                else
                {
                    branch.Add(new TreeNode(node.Attributes["name"].Value));
                }
            }
            return branch;
        }
    }
}
