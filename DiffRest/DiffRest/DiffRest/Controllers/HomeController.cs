using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using DiffRest.Models;
using RestLogService.Models;

namespace DiffRest.Controllers
{
    public class HomeController : Controller
    {
        private TreeNode tree;
        private List<Service> changes;
        List<bool> allowed;

        public ActionResult Index()
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

        [HttpGet]
        public JsonResult List(string oldRelease, string newRelease, bool noChange, bool eddited, bool added, bool removed)
        {
            string xml1 = Server.MapPath(@"~/rest_sample/" + oldRelease + "/metadata.xml");
            string xml2 = Server.MapPath(@"~/rest_sample/" + newRelease + "/metadata.xml");

            allowed = new List<bool>
            {
                noChange,
                eddited,
                added,
                removed
            };

            changes = new List<Service>();
            CompareFiles(xml1, xml2);

            return Json(changes, JsonRequestBehavior.AllowGet);
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
                minibranch = branch.GetChild(node.Attributes["name"].Value);
                if (minibranch != null)
                {
                    if (node.ChildNodes.Count > 0 || node.Attributes["hashCode"] == null)
                    {
                        CheckBranch(node, minibranch, order);
                    }
                    else
                    {
                        if (order)
                        {
                            if (minibranch.GetChild(node.Attributes["hashCode"].Value) != null)
                            {
                                if (allowed[0])
                                {
                                    changes.Add(new Service(minibranch.ID, minibranch.Parent.ID, "No changes"));
                                }
                            }
                            else
                            {
                                if (allowed[1])
                                {
                                    changes.Add(new Service(minibranch.ID, minibranch.Parent.ID, "Edited"));
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (order)
                    {
                        if (allowed[2])
                        {
                            minibranch = AddBranch(node, new TreeNode(node.Attributes["name"].Value));
                            GetValue(branch).Add(minibranch);
                            foreach (TreeNode n in GetValue(branch).GetChild(minibranch.ID))
                            {
                                changes.Add(new Service(n.ID, n.Parent.ID, "Added"));
                            }
                        }
                    }
                    else
                    {
                        if (allowed[3])
                        {
                            changes.Add(new Service(node.Attributes["name"].Value, branch.ID, "Removed"));
                        }
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