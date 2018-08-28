using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestLogService.Models
{
    public class TreeNode
    {
        public Dictionary<string, TreeNode> _children = new Dictionary<string, TreeNode>();

        public readonly string ID;
        public TreeNode Parent { get; private set; }

        public TreeNode(string id)
        {
            ID = id;
        }

        public TreeNode GetChild(string id)
        {
            return _children.TryGetValue(id, out TreeNode value) ? value : null;
        }

        public void Add(TreeNode item)
        {
            try
            {
                if (item.Parent != null)
                {
                    item.Parent._children.Remove(item.ID);
                }

                item.Parent = this;
                _children.Add(item.ID, item);
            }
            catch
            {
                //Console.WriteLine("Jau eksistē/nevar pievienot: " + item.ID);
            }
        }

        public TreeNode Remove(TreeNode item)
        {
            _children.Remove(item.ID);
            return this;
        }

        public IEnumerator<TreeNode> GetEnumerator()
        {
            return _children.Values.GetEnumerator();
        }

        public int Count
        {
            get { return _children.Count; }
        }
    }
}