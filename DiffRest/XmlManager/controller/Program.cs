using System;
using System.Xml;
using controller;

namespace XmlController
{
    public class Program
    {        
        static void Main(string[] args)
        {
            string location = "..//..//..//..//..//DiffApp/rest_sample/";
            XmlDocument doc = new XmlDocument();
            doc.Load(location+"520/1/metadata.xml");
            
            TreeNode tree = new TreeNode("Root");

            //Creates a tree
            foreach (XmlNode node in doc)
            {
                tree.Add(AddServiceGroups(node, new TreeNode(node.Name)));
            }
            
            doc.Load(location + "520/2/metadata.xml");

            //Searches the tree
            foreach (XmlNode node in doc)
            {
                CheckNodes(node, tree.GetChild(node.Name));
            }
            Console.ReadKey();
        }

        private static TreeNode AddServiceGroups(XmlNode service_group, TreeNode branch)
        {
            foreach (XmlNode node in service_group.ChildNodes)
            {
                switch (node.Name)
                {
                    case "service_group":
                        branch.Add(AddServiceGroups(node, new TreeNode(node.Attributes["name"].Value)));
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
                    smallBranch.Add(new TreeNode(node.Attributes["noNamspaceHashCode"].Value));
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
                            try
                            {
                                minibranch = minibranch.GetChild(node.Attributes["noNamspaceHashCode"].Value);
                                Console.WriteLine(minibranch.ID);
                            }
                            catch
                            {
                                Console.WriteLine("Mainīts                     " + node.Attributes["name"].Value);
                            }
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine("pielikts                     " + node.Attributes["name"].Value);
                }
            }
        }
    }
}
