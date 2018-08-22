using System;
using System.Xml;
using controller;

namespace XmlController
{
    public class Program
    {        
        private static bool order = true;

        static void Main(string[] args)
        {
            string location = "..//..//..//..//..//DiffApp/rest_sample/";
            string xml1 = location + "520/2/metadata.xml";
            string xml2 = location + "525/0/metadata.xml";

            CompareFiles(xml1, xml2);

            order = false;
            CompareFiles(xml2, xml1);
        }

        private static void CompareFiles(string xml1, string xml2)
        {
            XmlDocument doc = new XmlDocument();
            TreeNode tree = new TreeNode("Root");

            doc.Load(xml1);
            //Creates a tree
            foreach (XmlNode node in doc)
            {
                tree.Add(AddGroups(node, new TreeNode(node.Name)));
            }
            
            doc.Load(xml2);
            //Searches the tree
            foreach (XmlNode node in doc)
            {
                CheckNodes(node, tree.GetChild(node.Name));
            }
        }

        private static TreeNode AddGroups(XmlNode service_group, TreeNode branch)
        {
            foreach (XmlNode node in service_group.ChildNodes)
            {
                switch (node.Name)
                {
                    case "service_group":
                        branch.Add(AddGroups(node, new TreeNode(node.Attributes["name"].Value)));
                        break;
                    case "service":
                        branch.Add(AddServices(node, new TreeNode(node.Attributes["name"].Value)));
                        break;
                    default:
                        Console.WriteLine(node.Name + " : Not a case");
                        break;
                }
            }
            return branch;
        }

        private static TreeNode AddServices(XmlNode serviceNode, TreeNode branch)
        {
            foreach (XmlNode node in serviceNode.ChildNodes)
            {
                if (node.Name.Equals("resource"))
                {
                    TreeNode smallBranch = new TreeNode(node.Attributes["name"].Value);
                    branch.Add(AddServices(node, smallBranch));
                }
                else
                {
                    TreeNode smallBranch = new TreeNode(node.Attributes["name"].Value);
                    smallBranch.Add(new TreeNode(node.Attributes["hashCode"].Value));
                    branch.Add(smallBranch);
                }
            }
            return branch;
        }

        private static void CheckNodes(XmlNode nodes, TreeNode branch)
        {
            TreeNode minibranch;
            foreach (XmlNode node in nodes.ChildNodes)
            {
                try
                {
                    minibranch = branch.GetChild(node.Attributes["name"].Value);
                    switch (node.Name)
                    {
                        case "service_group":
                        case "service":
                        case "resource":
                            CheckNodes(node, minibranch);
                            break;
                        default:
                            if (order)
                            {
                                try
                                {
                                    minibranch = minibranch.GetChild(node.Attributes["hashCode"].Value);
                                    Console.WriteLine(minibranch.ID);
                                }
                                catch
                                {
                                    Console.WriteLine("Mainīts                     " + node.Attributes["name"].Value);
                                }
                            }
                            break;
                    }
                }
                catch
                {
                    if (order)
                    {
                        Console.WriteLine("Pielikts                     " + node.Attributes["name"].Value);
                    }
                    else
                    {
                        Console.WriteLine("Nonemts                      " + node.Attributes["name"].Value);
                    }
                }
            }
        }
    }
}
