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
            Dictionary<string, Service> services = CompareFiles("rest_sample/515/3/metadata.xml", "rest_sample/520/1/metadata.xml",true,false);

            foreach (KeyValuePair<string, Service> t in services)
            {
                foreach (Resource resource in t.Value.ResourceList)
                {
                    if (resource.Status.Equals("removed"))
                    {
                        Console.WriteLine(resource.Name);
                    }
                }
            }
            
            foreach(KeyValuePair<string, List<string>> x in GetVersions())
            {
                foreach(string i in x.Value)
                {
                    Console.WriteLine(x.Key +"."+ i);
                }
            }
        }

        private static List<KeyValuePair<string, List<string>>> GetVersions()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("rest_sample/Versions.xml");

            List<KeyValuePair<string, List<string>>> versions = new List<KeyValuePair<string, List<string>>>();

            foreach (XmlNode node in xml.SelectNodes("//version"))
            {
                List<string> releases = new List<string>();
                foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
                {
                    releases.Add(leaf.Attributes["name"].Value);
                }
                versions.Add(new KeyValuePair<string, List<string>>(node.Attributes["name"].Value, releases));
            }

            return versions;
        }

        private static Dictionary<string, Service> CompareFiles(string oldRelease, string newRelease, bool noChange = false, bool added = true)
        {
            XmlDocument xml = new XmlDocument();
            Dictionary<string, Service> services = new Dictionary<string, Service>();

            xml.Load(oldRelease);
            //fills services
            foreach (XmlNode node in xml.SelectNodes("//service"))
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

            xml.Load(newRelease);
            //searches services
            foreach (XmlNode node in xml.SelectNodes("//service"))
            {
                Service service = services.TryGetValue(node.Attributes["name"].Value, out Service value) ? value : null;
                if (service == null)
                {
                    if (added)//adds service only if user want to see it
                    {
                        service = AddService(node);
                        foreach (Resource resource in service.ResourceList)
                        {
                            resource.Status = "added";
                        }
                        services.Add(node.Attributes["name"].Value, service);
                    }
                }
                else
                {
                    services[node.Attributes["name"].Value] = SearchService(node, service, noChange, added);
                }
            }

            return services;
        }

        private static Service AddService(XmlNode node)
        {
            Service service = new Service(node.Attributes["description"].Value);
            foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
            {
                service.ResourceList.Add(new Resource(leaf.Attributes["name"].Value, leaf.Attributes["hashCode"].Value, leaf.Attributes["noNamspaceHashCode"].Value, "removed"));
            }
            return service;
        }

        private static Service SearchService(XmlNode node, Service service, bool noChange, bool added)
        {
            foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
            {
                Resource resource = service.ResourceList.Find(r => r.Name.Equals(leaf.Attributes["name"].Value));
                if (resource != null)
                {
                    if (resource.HashCode.Equals(leaf.Attributes["hashCode"].Value))
                    {
                        if (noChange)
                        {
                            resource.Status = "not changed";
                        }
                        else
                        {
                            service.ResourceList.Remove(resource);
                        }
                    }
                    else
                    {
                        resource.Status = "eddited";
                    }
                }
                else
                {
                    if (added)
                    {
                        service.ResourceList.Add(new Resource(leaf.Attributes["name"].Value, leaf.Attributes["hashCode"].Value, leaf.Attributes["noNamspaceHashCode"].Value, "added"));
                    }
                }
            }
            //service could be empty if no changes were made, or it was empty and after adding new element decides not to see it
            return service;
        }
    }

    public class Service
    {
        public Service(string description)
        {
            Description = description;
        }
        
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
