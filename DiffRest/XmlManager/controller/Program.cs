using System;
using System.Xml;
using controller;
using Newtonsoft.Json;

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
            //should add to json file: Shows what has been changed
        }

        private static void CompareFiles(string xml1, string xml2)
        {
            XmlDocument doc = new XmlDocument();
            TreeNode tree = new TreeNode("Root");

            doc.Load(xml1);
            //Creates a tree
            foreach (XmlNode node in doc)
            {
                tree.Add(AddBranch(node, new TreeNode(node.Name)));
            }

            doc.Load(xml2);
            //Searches the tree
            foreach (XmlNode node in doc)
            {
                CheckBranch(node, tree.GetChild(node.Name));
            }

            //saves the tree into json file
            string json = JsonConvert.SerializeObject(tree);
            System.IO.File.WriteAllText(@"comparison.json", json);
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

        private static void CheckBranch(XmlNode nodes, TreeNode branch)
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
                            CheckBranch(node, minibranch);
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
