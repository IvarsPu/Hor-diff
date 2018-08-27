using System;
using System.Collections.Generic;
using System.Xml;
using controller;

namespace XmlController
{
    public class Program
    {
        private static TreeNode tree;
        private static List<KeyValuePair<int, string>> texts;

        //0 no change
        //1 change
        //2 added
        //3 removed

        static void Main(string[] args)
        {
            string location = "..//..//..//..//..//DiffApp/rest_sample/";
            string xml1 = location + "520/2/metadata.xml";
            string xml2 = location + "525/0/metadata.xml";

            tree = new TreeNode("Root");
            texts = new List<KeyValuePair<int, string>>();

            CompareFiles(xml1, xml2);

            foreach(KeyValuePair<int, string> t in texts)
            {
                if(t.Key == 1)
                {
                    Console.WriteLine(t.Value);
                }
            }
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
                                texts.Add(new KeyValuePair<int, string>(0,node.Attributes["name"].Value));
                            }
                            catch (Exception)
                            {
                                texts.Add(new KeyValuePair<int, string>(1, node.Attributes["name"].Value));
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
                            texts.Add(new KeyValuePair<int, string>(2, n.ID));
                        }
                    }
                    else
                    {
                        texts.Add(new KeyValuePair<int, string>(3, node.Attributes["name"].Value));
                    }
                }
            }
        }

        private static TreeNode GetValue(TreeNode node)
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

    }
}
