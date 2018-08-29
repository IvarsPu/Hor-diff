using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml;
using DiffRest.Models;
using RestLogService.Models;

namespace DiffRest.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
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

            List<bool> allowed = new List<bool>
            {
                noChange,
                eddited,
                added,
                removed
            };

            return Json(CompareFiles(xml1, xml2, allowed), JsonRequestBehavior.AllowGet);
        }

        #region Change detection
        private List<Service> CompareFiles(string xml1, string xml2, List<bool> allowed)
        {
            List<Service> changes = new List<Service>();
            changes.AddRange(CheckTree(xml2, CreateTree(xml1), true, allowed));//Shows what hasnt changed, what has changed and whats added
            changes.AddRange(CheckTree(xml1, CreateTree(xml2), false, allowed));//Shows what was removed
            return changes;
        }

        private TreeNode CreateTree(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            
            TreeNode treeNode = new TreeNode("Root");
            foreach (XmlNode node in doc)
            {
                treeNode.Add(CreateBranch(node, new TreeNode(node.Name)));
            }
            return treeNode;
        }

        private TreeNode CreateBranch(XmlNode service_group, TreeNode branch)
        {
            foreach (XmlNode node in service_group.ChildNodes)
            {
                if (node.ChildNodes.Count > 0)
                {
                    branch.Add(CreateBranch(node, new TreeNode(node.Attributes["name"].Value)));
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

        private List<Service> CheckTree(string path, TreeNode tree, bool order, List<bool> allowed)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            List<Service> changes = new List<Service>();
            foreach (XmlNode node in doc)
            {
                changes.AddRange(CheckBranch(node, tree.GetChild(node.Name), order, allowed));
            }
            return changes;
        }

        private List<Service> CheckBranch(XmlNode nodes, TreeNode branch, bool order, List<bool> allowed)
        {
            List<Service> changes = new List<Service>();
            foreach (XmlNode node in nodes.ChildNodes)
            {
                TreeNode miniBranch = branch.GetChild(node.Attributes["name"].Value);
                if (miniBranch != null)
                {
                    if (node.ChildNodes.Count > 0 || node.Attributes["hashCode"] == null)
                    {
                        changes.AddRange(CheckBranch(node, miniBranch, order, allowed));
                    }
                    else
                    {
                        if (order)
                        {
                            if (miniBranch.GetChild(node.Attributes["hashCode"].Value) != null)
                            {
                                if (allowed[0])
                                {
                                    changes.Add(new Service(miniBranch.ID, miniBranch.Parent.ID, "No changes"));
                                }
                            }
                            else
                            {
                                if (allowed[1])
                                {
                                    changes.Add(new Service(miniBranch.ID, miniBranch.Parent.ID, "Edited"));
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
                            miniBranch = CreateBranch(node, new TreeNode(node.Attributes["name"].Value));
                            foreach (TreeNode smallBranch in miniBranch)
                            {
                                changes.Add(new Service(smallBranch.ID, smallBranch.Parent.ID, "Added"));
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
            return changes;
        }
        #endregion
    }
}