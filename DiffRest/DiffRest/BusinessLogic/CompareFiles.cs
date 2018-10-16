using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Models;

namespace BusinessLogic
{
    public class CompareFiles
    {
        public List<Service> Compare(string oldRelease, string newRelease, bool noChange, bool added, bool ignoreNamespaceChanges)
        {
            XmlDocument xml = new XmlDocument();
            string path = AppInfo.MetadataRootFolder + oldRelease + "/metadata.xml";
            if (!File.Exists(path))
            {
                return null;
            }
            xml.Load(path);//old file
            CompareFiles changeDetection = new CompareFiles();
            Dictionary<string, Service> services = changeDetection.GetServices(xml);

            path = AppInfo.MetadataRootFolder + newRelease + "/metadata.xml";
            if (!File.Exists(path))
            {
                return null;
            }
            xml.Load(path);//new file
            return changeDetection.CompareServices(services, xml, noChange, added, ignoreNamespaceChanges);
        }

        #region Comapre
        private Dictionary<string, Service> GetServices(XmlDocument xml)
        {
            Dictionary<string, Service> services = new Dictionary<string, Service>();
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
            return services;
        }

        private List<Service> CompareServices(Dictionary<string, Service> services, XmlDocument xml, bool noChange, bool added, bool ignoreNamespaceChanges)
        {
            foreach (XmlNode node in xml.SelectNodes("//service"))
            {
                Service service = services.TryGetValue(node.Attributes["name"].Value, out Service value) ? value : null;
                if (service == null)//new service
                {
                    if (added)
                    {
                        service = AddService(node);
                        service.Status = "added";
                        foreach (Resource resource in service.ResourceList)
                        {
                            resource.Status = "added";
                        }
                        services.Add(node.Attributes["name"].Value, service);
                    }
                }
                else//existing
                {
                    service = GetService(CompareResources(node, service, ignoreNamespaceChanges), noChange, added);
                    if (service == null)
                    {
                        services.Remove(node.Attributes["name"].Value);
                    }
                    else
                    {
                        services[node.Attributes["name"].Value] = service;
                    }
                }
            }

            return services.Values.ToList();
        }

        private Service AddService(XmlNode node)
        {
            Service service = new Service(node.Attributes["name"].Value, node.Attributes["description"].Value, "removed");
            foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
            {
                service.ResourceList.Add(new Resource(leaf.Attributes["name"].Value, leaf.Attributes["hashCode"].Value, leaf.Attributes["noNamspaceHashCode"].Value, "removed"));
            }
            return service;
        }

        private Service CompareResources(XmlNode node, Service service, bool ignoreNamespaceChanges)
        {
            foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
            {
                Resource resource = service.ResourceList.Find(r => r.Name.Equals(leaf.Attributes["name"].Value));
                if (resource != null)
                {
                    if ((!ignoreNamespaceChanges && resource.HashCode.Equals(leaf.Attributes["hashCode"].Value)) ||
                        (ignoreNamespaceChanges && resource.NoNamspaceHashCode.Equals(leaf.Attributes["noNamspaceHashCode"].Value)))
                    {
                        resource.Status = "no change";
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
        
        private Service GetService(Service service, bool noChange, bool added)
        {
            List<Resource> list = service.ResourceList;
            if (list.All(o => o.Status.Equals(list[0].Status)))
            {
                if (list.Count > 0)
                {
                    service.Status = list[0].Status;
                    if ((!noChange && service.Status.Equals("no change")) ||
                        (!added && service.Status.Equals("added")))
                    {
                        return null;
                    }
                    else
                    {
                        return service;
                    }
                }
                else
                {
                    if (!noChange)
                    {
                        return null;
                    }
                    else
                    {
                        service.Status = "no change";
                        return service;
                    }
                }
            }
            else
            {
                service.Status = "eddited";
                return service;
            }
        }
        #endregion
    }
}
