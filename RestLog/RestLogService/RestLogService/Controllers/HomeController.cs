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
        private List<KeyValuePair<int, string>> changes;

        [HttpGet]
        public string List(string oldRelease, string newRelease)
        {
            string xml1 = Server.MapPath(@"~/rest_sample/" + oldRelease + "/metadata.xml");
            string xml2 = Server.MapPath(@"~/rest_sample/" + newRelease + "/metadata.xml");

            tree = new TreeNode("Root");
            changes = new List<KeyValuePair<int, string>>();

            CompareFiles(xml1, xml2);

            string html = "<table>";
            foreach (KeyValuePair<int, string> pair in changes)
            {
                html = html + string.Format("<tr><td>{0}</td><td>{1}</td></tr>", pair.Key.ToString(), pair.Value);
            }
            html = html + "</table>";

            return html;
        }

        #region change
        private void CompareFiles(string xml1, string xml2)
        {
            tree = CreateTree(xml1);
            CheckTree(xml2, tree, true);//Shows what hasnt changed, what has changed and whats added
            CheckTree(xml1, CreateTree(xml2), false);//Shows what was removed
        }

        private TreeNode CreateTree(string path)
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(path);
            //Creates a tree
            TreeNode treeNode = new TreeNode("Root");
            foreach (XmlNode node in doc)
            {
                treeNode.Add(AddBranch(node, new TreeNode(node.Name)));
            }
            return treeNode;
        }

        private void CheckTree(string path, TreeNode tree, bool order)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            foreach (XmlNode node in doc)
            {
                CheckBranch(node, tree.GetChild(node.Name), order);
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

        private void CheckBranch(XmlNode nodes, TreeNode branch, bool order)
        {
            TreeNode minibranch;
            foreach (XmlNode node in nodes.ChildNodes)
            {
                try
                {
                    minibranch = branch.GetChild(node.Attributes["name"].Value);
                    if (node.ChildNodes.Count > 0 || node.Attributes["hashCode"] == null)
                    {
                        CheckBranch(node, minibranch, order);
                    }
                    else
                    {
                        if (order)
                        {
                            try
                            {
                                minibranch.GetChild(node.Attributes["hashCode"].Value);
                                changes.Add(new KeyValuePair<int, string>(0, node.Attributes["name"].Value));
                            }
                            catch (Exception)
                            {
                                changes.Add(new KeyValuePair<int, string>(1, node.Attributes["name"].Value));
                            }
                        }
                    }
                }
                catch
                {
                    if (order)
                    {
                        minibranch = AddBranch(node, new TreeNode(node.Attributes["name"].Value));
                        GetValue(branch).Add(minibranch);
                        foreach (TreeNode n in GetValue(branch).GetChild(minibranch.ID))
                        {
                            changes.Add(new KeyValuePair<int, string>(2, n.ID));
                        }
                    }
                    else
                    {
                        changes.Add(new KeyValuePair<int, string>(3, node.Attributes["name"].Value));
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
