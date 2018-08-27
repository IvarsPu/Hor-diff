using System;
using System.Collections.Generic;
using System.Xml;
using controller;

namespace XmlController
{
    public class Program
    {
        private static TreeNode tree;

        static void Main(string[] args)
        {
            string location = "..//..//..//..//..//DiffApp/rest_sample/";
            string xml1 = location + "520/2/metadata.xml";
            string xml2 = location + "525/0/metadata.xml";

            tree = new TreeNode("Root");

            CompareFiles(xml1, xml2);
        }

        private static void CompareFiles(string xml1, string xml2)
        {
            tree = CreateTree(xml1);
            CheckTree(xml2, tree, true);//Shows what hasnt changed, what has changed and whats added
            CheckTree(xml1, CreateTree(xml2), false);//Shows what was removed
        }

        private static TreeNode CreateTree(string path)
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

        private static void CheckTree(string path, TreeNode tree, bool order)
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(path);
            //adds removed branches to tree
            foreach (XmlNode node in doc)
            {
                CheckBranch(node, tree.GetChild(node.Name), order);
            }
        }

        private static TreeNode AddBranch(XmlNode service_group, TreeNode branch)
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

        private static void CheckBranch(XmlNode nodes, TreeNode branch, bool order)
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
                            CheckBranch(node, minibranch, order);
                            break;
                        default:
                            if (order)
                            {
                                try
                                {
                                    Console.WriteLine(node.Attributes["hashCode"].Value);
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
                        //not added to tree
                        Console.WriteLine("Pielikts                     " + node.Attributes["name"].Value);
                    }
                    else
                    {
                        Console.WriteLine("Nonemts                     " + node.Attributes["name"].Value);
                    }
                }
            }
        }
    }
}
