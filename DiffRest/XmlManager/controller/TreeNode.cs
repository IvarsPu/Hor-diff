using System;
using System.Collections.Generic;

namespace controller
{
    public class TreeNode
    {
        private readonly Dictionary<string, TreeNode> branches = new Dictionary<string, TreeNode>();

        public readonly string ID;
        public TreeNode Parent { get; private set; }

        public TreeNode(string id)
        {
            ID = id;
        }

        public TreeNode GetChild(string id)
        {
            return branches[id];
        }

        public void Add(TreeNode item)
        {
            try
            {
                if (item.Parent != null)
                {
                    item.Parent.branches.Remove(item.ID);
                }

                item.Parent = this;
                branches.Add(item.ID, item);
            }
            catch
            {
                Console.WriteLine("Jau eksistē/nevar pievienot: " + item.ID);
            }
        }

        public int Count
        {
            get { return branches.Count; }
        }
    }
}
