using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DiffRest.Models;
using System.Web.Http;

namespace DiffRest.Controllers
{
    [RoutePrefix("api/Home")]
    public class HomeController : ApiController
    {
        [Route("GetVersions")]
        [HttpGet]
        public IList<HorizonVersion> GetVersions()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(System.Web.HttpContext.Current.Server.MapPath("~/rest_sample/Versions.xml"));

            IList<HorizonVersion> versions = new List<HorizonVersion>();

            foreach (XmlNode node in xml.SelectNodes("//version"))
            {
                List<string> releases = new List<string>();
                foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
                {
                    releases.Add(leaf.Attributes["name"].Value);
                }
                HorizonVersion version = new HorizonVersion(node.Attributes["name"].Value)
                {
                    ReleaseList = releases
                };
                versions.Add(version);
            }

            return versions;
        }

        [Route("CompareFiles")]
        [HttpGet]
        public IList<Service> CompareFiles(string oldRelease, string newRelease, bool noChange = false, bool added = true)
        {
            XmlDocument xml = new XmlDocument();
            Dictionary<string, Service> services = new Dictionary<string, Service>();
            
            xml.Load(System.Web.HttpContext.Current.Server.MapPath("~/rest_sample/" + oldRelease + "/metadata.xml"));
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

            xml.Load(System.Web.HttpContext.Current.Server.MapPath("~/rest_sample/" + newRelease + "/metadata.xml"));
            //searches services
            foreach (XmlNode node in xml.SelectNodes("//service"))
            {
                Service service = services.TryGetValue(node.Attributes["name"].Value, out Service value) ? value : null;
                if (service == null)
                {
                    if (added)
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
                    service = CompareResources(node, service, noChange, added);
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

        #region Change detection
        private Service AddService(XmlNode node)
        {
            Service service = new Service(node.Attributes["name"].Value, node.Attributes["description"].Value);
            foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
            {
                service.ResourceList.Add(new Resource(leaf.Attributes["name"].Value, leaf.Attributes["hashCode"].Value, leaf.Attributes["noNamspaceHashCode"].Value, "removed"));
            }
            return service;
        }

        private Service CompareResources(XmlNode node, Service service, bool noChange, bool added)
        {
            bool serviceMeetsNeeds = false;
            foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
            {
                Resource resource = service.ResourceList.Find(r => r.Name.Equals(leaf.Attributes["name"].Value));
                if (resource != null)
                {
                    if (resource.HashCode.Equals(leaf.Attributes["hashCode"].Value))
                    {
                        resource.Status = "not changed";
                        if (noChange)
                        {
                            serviceMeetsNeeds = true;
                        }
                    }
                    else
                    {
                        resource.Status = "eddited";
                        serviceMeetsNeeds = true;
                    }
                }
                else
                {
                    service.ResourceList.Add(new Resource(leaf.Attributes["name"].Value, leaf.Attributes["hashCode"].Value, leaf.Attributes["noNamspaceHashCode"].Value, "added"));
                    if (added)
                    {
                        serviceMeetsNeeds = true;
                    }
                }
            }
            if (serviceMeetsNeeds || service.ResourceList.Find(r => r.Status.Equals("removed")) != null)
            {
                return service;
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}