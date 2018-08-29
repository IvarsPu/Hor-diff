using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.XmlDiffPatch;
using System.Xml.XPath;

namespace TestApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            XmlDocument xml = new XmlDocument();
            Dictionary<string, Service> services = new Dictionary<string, Service>();

            xml.Load("rest_sample/515/3/metadata.xml");
            //fills services
            foreach(XmlNode node in xml.SelectNodes("//service"))
            {
                try
                {
                    services.Add(node.Attributes["name"].Value, AddService(node));
                }
                catch
                {
                    //element with this key already exists
                }
            }

            xml.Load("rest_sample/520/1/metadata.xml");
            //searches services
            foreach (XmlNode node in xml.SelectNodes("//service"))
            {
                Service service = services.TryGetValue(node.Attributes["name"].Value, out Service value) ? value : null;
                if (service == null)
                {
                    service = AddService(node);
                    foreach (Resource resource in service.ResourceList)
                    {
                        resource.Status = "added";
                    }
                    services.Add(node.Attributes["name"].Value, service);
                }
                else
                {
                    services[node.Attributes["name"].Value] = SearchService(node, service);
                }
            }

            //foreach (KeyValuePair<string, Service> t in services)
            //{
            //    foreach (Resource resource in t.Value.ResourceList)
            //    {
            //        if (resource.Status.Equals("not changed"))
            //        {
            //            Console.WriteLine(resource.Name);
            //        }
            //    }
            //}
        }

        private static Service AddService(XmlNode node)
        {
            Service service = new Service(node.Attributes["name"].Value, node.Attributes["description"].Value);
            foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
            {
                service.ResourceList.Add(new Resource(leaf.Attributes["name"].Value, leaf.Attributes["hashCode"].Value, leaf.Attributes["noNamspaceHashCode"].Value, "removed"));
            }
            return service;
        }

        private static Service SearchService(XmlNode node, Service service)
        {
            foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
            {
                Resource resource = service.ResourceList.Find(r => r.Name.Equals(leaf.Attributes["name"].Value));
                if (resource != null)
                {
                    if (resource.HashCode.Equals(leaf.Attributes["hashCode"].Value))
                    {
                        resource.Status = "not changed";
                    }
                    else
                    {
                        resource.Status = "eddited";
                    }
                }
                else
                {
                    service.ResourceList.Add(new Resource(leaf.Attributes["name"].Value, leaf.Attributes["hashCode"].Value, leaf.Attributes["noNamspaceHashCode"].Value, "added"));
                }
            }
            return service;
        }
    }

    public class Service
    {
        public Service(string name, string description)
        {
            Name = name;
            Description = description;
        }
        
        public string Name { get; set; } = String.Empty;
        
        public string Description { get; set; } = String.Empty;
        
        public List<Resource> ResourceList { get; set; } = new List<Resource>();
    }

    public class Resource
    {
        public Resource(string name, string hashCode, string noNamspaceHashCode, string status)
        {
            Name = name;
            HashCode = hashCode;
            NoNamspaceHashCode = noNamspaceHashCode;
            Status = status;
        }
        
        public string Name { get; set; } = String.Empty;

        public string HashCode { get; set; } = String.Empty;

        public string NoNamspaceHashCode { get; set; } = String.Empty;

        public string Status { get; set; } = String.Empty;
    }
}
