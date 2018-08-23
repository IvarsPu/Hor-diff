using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace controller
{
    [JsonConverter(typeof(TreeNodeConverter))]
    public class TreeNode : IEnumerable<TreeNode>
    {
        public readonly Dictionary<string, TreeNode> _children = new Dictionary<string, TreeNode>();

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
            try
            {
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

    class TreeNodeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // we can serialize everything that is a TreeNode
            return typeof(TreeNode).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // we currently support only writing of JSON
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // we serialize a node by just serializing the _children dictionary
            var node = value as TreeNode;
            serializer.Serialize(writer, node._children);
        }
    }
}
