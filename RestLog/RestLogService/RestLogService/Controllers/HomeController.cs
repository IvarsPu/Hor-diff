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

        private TreeNode tree;
        private string changes;

        [HttpGet]
        public string List(string oldRelease, string newRelease)
        {
            string xml1 = Server.MapPath(@"~/rest_sample/" + oldRelease + "/metadata.xml");
            string xml2 = Server.MapPath(@"~/rest_sample/" + newRelease + "/metadata.xml");

            changes = "<table>" +
                "<thead>" +
                    "<tr>" +
                        "<td><h3>State</h3></td>" +
                        "<td><h3>Name</h3></td>" +
                        "<td><h3>Service</h3></td>" +
                    "<tr>" +
                "</thead>" +
                "<tbody>";
            CompareFiles(xml1, xml2);
            changes += "</tbody>" +
                "</table>";

            return changes;
        }

        #region change
        private void CompareFiles(string xml1, string xml2)
        {
            //Shows what hasnt changed, what has changed and whats added, but not whats removed
            XmlDocument doc = new XmlDocument();

            doc.Load(xml1);
            //Creates a tree
            tree = new TreeNode("Root");
            foreach (XmlNode node in doc)
            {
                tree.Add(AddBranch(node, new TreeNode(node.Name)));
            }
            
            doc.Load(xml2);
            foreach (XmlNode node in doc)
            {
                CheckBranch(node, tree.GetChild(node.Name));
            }
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
                    TreeNode smallBranch = new TreeNode(node.Attributes["name"].Value);
                    if (node.Attributes["hashCode"] != null)
                    {
                        smallBranch.Add(new TreeNode(node.Attributes["hashCode"].Value));
                    }
                    branch.Add(smallBranch);
                }
            }
            return branch;
        }
        
        private void CheckBranch(XmlNode nodes, TreeNode branch)
        {
            TreeNode minibranch;
            foreach (XmlNode node in nodes.ChildNodes)
            {
                minibranch = branch.GetChild(node.Attributes["name"].Value);
                if (minibranch != null)
                { 
                    if (node.ChildNodes.Count > 0 || node.Attributes["hashCode"] == null)
                    {
                        CheckBranch(node, minibranch);
                    }
                    else
                    {
                        if (minibranch.GetChild(node.Attributes["hashCode"].Value) != null)
                        {
                            //changes += "<tr><td>Not changed</td><td>" + minibranch.ID + "</td><td>" + minibranch.Parent.ID + "</td></tr>";
                        }
                        else
                        {
                            //changes += "<tr><td>Edited</td><td>" + minibranch.ID + "</td><td>" + minibranch.Parent.ID + "</td></tr>";
                        }
                    }
                }
                else
                {
                    minibranch = AddBranch(node, new TreeNode(node.Attributes["name"].Value));
                    GetValue(branch).Add(minibranch);
                    foreach (TreeNode n in GetValue(branch).GetChild(minibranch.ID))
                    {
                        //changes += "<tr><td>Added</td><td>" + n.ID + "</td><td>" + n.Parent.ID + "</td></tr>";
                    }
                }
            }
        }

        private TreeNode GetValue(TreeNode node)
        {
            List<string> identifiers = new List<string>
            {
                node.ID
            };
            while (node.Parent != null)
            {
                node = node.Parent;
                identifiers.Add(node.ID);
            }
            switch (identifiers.Count)
            {
                case 2:
                    return tree.GetChild(identifiers[0]);
                case 3:
                    return tree.GetChild(identifiers[1]).GetChild(identifiers[0]);
                case 4:
                    return tree.GetChild(identifiers[2]).GetChild(identifiers[1]).GetChild(identifiers[0]);
                case 5:
                    return tree.GetChild(identifiers[3]).GetChild(identifiers[2]).GetChild(identifiers[1]).GetChild(identifiers[0]);
                case 6:
                    return tree.GetChild(identifiers[4]).GetChild(identifiers[3]).GetChild(identifiers[2]).GetChild(identifiers[1]).GetChild(identifiers[0]);
                case 7:
                    return tree.GetChild(identifiers[5]).GetChild(identifiers[4]).GetChild(identifiers[3]).GetChild(identifiers[2]).GetChild(identifiers[1]).GetChild(identifiers[0]);
                case 8:
                    return tree.GetChild(identifiers[6]).GetChild(identifiers[5]).GetChild(identifiers[4]).GetChild(identifiers[3]).GetChild(identifiers[2]).GetChild(identifiers[1]).GetChild(identifiers[0]);
                case 9:
                    return tree.GetChild(identifiers[7]).GetChild(identifiers[6]).GetChild(identifiers[5]).GetChild(identifiers[4]).GetChild(identifiers[3]).GetChild(identifiers[2]).GetChild(identifiers[1]).GetChild(identifiers[0]);
                default:
                    Console.WriteLine("Too many");
                    return null;
            }
        }
        #endregion
    }
}

    