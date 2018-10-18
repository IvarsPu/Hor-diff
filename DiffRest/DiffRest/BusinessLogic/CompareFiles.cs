using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Models;

namespace BusinessLogic
{
    public class CompareFiles
    {
        private static readonly string noChangeStatus = "not changed", addedStatus = "added", editStatus = "eddited", removeStatus = "removed";
        private bool noChange, added, ignoreNamespaceChanges;

        public List<Service> Compare(string oldRelease, string newRelease, bool noChange, bool added, bool ignoreNamespaceChanges)
        {
            XmlDocument xml = new XmlDocument();
            string path = AppInfo.MetadataRootFolder + oldRelease + "/metadata.xml";
            if (!File.Exists(path))
            {
                return null;
            }
            xml.Load(path);//old file
            List<Service> services = GetServices(xml);

            path = AppInfo.MetadataRootFolder + newRelease + "/metadata.xml";
            if (!File.Exists(path))
            {
                return null;
            }
            xml.Load(path);//new file

            this.noChange = noChange;
            this.added = added;
            this.ignoreNamespaceChanges = ignoreNamespaceChanges;

            return CompareServices(services, xml);
        }

        #region Compare
        private List<Service> GetServices(XmlDocument xml)
        {
            List<Service> services = new List<Service>();
            foreach (XmlNode node in xml.SelectNodes("//service"))
            {
                try
                {
                    services.Add(AddService(node));
                }
                catch
                {
                    //element with this key already exists
                }
            }
            return services;
        }

        private List<Service> CompareServices(List<Service> services, XmlDocument xml)
        {
            foreach (XmlNode node in xml.SelectNodes("//service"))
            {
                Service service = services.Find(r => r.Name.Equals(node.Attributes["name"].Value));
                if (service == null)//new service
                {
                    if (added)
                    {
                        service = AddService(node);
                        service.Status = addedStatus;
                        foreach (Resource resource in service.ResourceList)
                        {
                            resource.Status = addedStatus;
                        }
                        services.Add(service);
                    }
                }
                else//existing
                {
                    service = GetService(CompareResources(node, service));
                    if (service == null)
                    {
                        services.Remove(services.Find(r => r.Name.Equals(node.Attributes["name"].Value)));
                    }
                }
            }
            return services;
        }

        //creates service from info in xml node
        private Service AddService(XmlNode node)
        {
            Service service = new Service(node.Attributes["name"].Value, node.Attributes["description"].Value, removeStatus);
            foreach (XmlNode leaf in node.SelectNodes(".//*[not(child::*)]"))
            {
                service.ResourceList.Add(new Resource(leaf.Attributes["name"].Value, leaf.Attributes["hashCode"].Value, leaf.Attributes["noNamspaceHashCode"].Value, removeStatus));
            }
            return service;
        }

        private Service CompareResources(XmlNode node, Service service)
        {
            foreach (XmlNode leaf in node.SelectNodes(".//*[not(child::*)]"))
            {
                Resource resource = service.ResourceList.Find(r => r.Name.Equals(leaf.Attributes["name"].Value));
                if (resource != null)
                {
                    if ((!ignoreNamespaceChanges && resource.HashCode.Equals(leaf.Attributes["hashCode"].Value)) ||
                        (ignoreNamespaceChanges && resource.NoNamspaceHashCode.Equals(leaf.Attributes["noNamspaceHashCode"].Value)))
                    {
                        resource.Status = noChangeStatus;
                    }
                    else
                    {
                        resource.Status = editStatus;
                    }
                }
                else
                {
                    service.ResourceList.Add(new Resource(leaf.Attributes["name"].Value, leaf.Attributes["hashCode"].Value, leaf.Attributes["noNamspaceHashCode"].Value, addedStatus));
                }
            }
            return service;
        }

        private Service GetService(Service service)
        {
            List<Resource> list = service.ResourceList;
            if (list.All(o => o.Status.Equals(list[0].Status)))
            {
                if (list.Count > 0)
                {
                    service.Status = list[0].Status;
                    if ((!noChange && service.Status.Equals(noChangeStatus)) ||
                        (!added && service.Status.Equals(addedStatus)))
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
                        service.Status = noChangeStatus;
                        return service;
                    }
                }
            }
            else
            {
                service.Status = editStatus;
                return service;
            }
        }
        #endregion
    }
}
