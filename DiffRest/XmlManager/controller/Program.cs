using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace XmlController
{
    public class Program
    {
        private static Dictionary<string, int> serviceGroups = new Dictionary<string, int>();

        //change
        //new
        //removed
        //no difference
        
        static void Main(string[] args)
        {
            XmlDocument doc1 = new XmlDocument();
            doc1.Load("520/1/metadata.xml");

            TreeNode tree = new TreeNode("Root");
            //foreach (XmlNode node in doc1.SelectNodes("rest_api_metadata/service_group"))
            //{
            //    tree.Add(AddServiceGroups(node, new TreeNode(node.Attributes["name"].Value)));
            //}

            foreach (XmlNode node in doc1)
            {
                tree.Add(AddServiceGroups(node, new TreeNode(node.Name)));
            }

            //Console.WriteLine(tree.GetChild("Virsgrāmata").GetChild("TdmPDok").GetChild("TdmPDok.wadsl").Count);

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
                    TreeNode smallBranch = new TreeNode(node.Name);
                    AddServices(node, smallBranch);
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
    }

    public class TreeNode : IEnumerable<TreeNode>
    {
        private readonly Dictionary<string, TreeNode> _children =
                                            new Dictionary<string, TreeNode>();

        public readonly string ID;
        public TreeNode Parent { get; private set; }

        public TreeNode(string id)
        {
            this.ID = id;
        }

        public TreeNode GetChild(string id)
        {
            return this._children[id];
        }

        public void Add(TreeNode item)
        {
            try { 
            if (item.Parent != null)
            {
                item.Parent._children.Remove(item.ID);
            }

            item.Parent = this;
            this._children.Add(item.ID, item);
            }
            catch
            {
                Console.WriteLine("Jau eksistē/nevar pievienot: " + item.ID);
            }
        }

        public IEnumerator<TreeNode> GetEnumerator()
        {
            return this._children.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get { return this._children.Count; }
        }
    }

}
