using System;
using System.Collections.Generic;
using System.Xml;
using controller;
using Newtonsoft.Json;

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
            tree = CreateTreeNode(xml1);
            CheckTreeNode(xml2, tree, true);//Shows what hasnt changed, what has changed and whats added
            CheckTreeNode(xml1, CreateTreeNode(xml2), false);//Shows what was removed

            //saves the tree into json file
            string json = JsonConvert.SerializeObject(tree);
            System.IO.File.WriteAllText("comparison.json", json);
        }

        private static TreeNode CreateTreeNode(string path)
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

        private static void CheckTreeNode(string path, TreeNode tree, bool order)
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
                switch (node.Name)
                {
                    case "service_group":
                    case "service":
                    case "resource":
                        branch.Add(AddBranch(node, new TreeNode(node.Attributes["name"].Value)));
                        break;
                    default:
                        TreeNode smallBranch = new TreeNode(node.Attributes["name"].Value);
                        smallBranch.Add(new TreeNode(node.Attributes["hashCode"].Value));
                        branch.Add(smallBranch);
                        break;
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
                                    GetValue(minibranch).Remove(minibranch.GetChild(node.Attributes["hashCode"].Value));
                                    GetValue(minibranch).Add(new TreeNode("no change"));
                                }
                                catch
                                {
                                    GetValue(minibranch)._children = new Dictionary<string, TreeNode>();
                                    GetValue(minibranch).Add(new TreeNode("change"));
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
                        Console.WriteLine("Nonemts                     " + node.Attributes["name"].Value);
                    }
                }
            }
        }

        private static TreeNode GetValue(TreeNode node)
        {
            List<string> identifiers = new List<string>();
            identifiers.Add(node.ID);
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
    }
}
