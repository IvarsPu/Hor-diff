using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DiffRest.Models;
using System.Web.Http;
using System.Web.Configuration;

namespace DiffRest.Controllers
{
    [RoutePrefix("Home")]
    public class HomeController : ApiController
    {
        [Route("GetVersions")]
        [HttpGet]
        public IList<HorizonVersion> GetVersions()
        {
            XmlDocument xml = new XmlDocument();
            string path = WebConfigurationManager.AppSettings["MetadataLocalFolder"].ToString();
            xml.Load(System.Web.HttpContext.Current.Server.MapPath(path+ "Versions.xml"));

            IList<HorizonVersion> versions = new List<HorizonVersion>();

            foreach (XmlNode node in xml.SelectNodes("//version"))
            {
                HorizonVersion version = new HorizonVersion(node.Attributes["name"].Value);
                foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
                {
                    version.ReleaseList.Add(new HorizonRelease(leaf.Attributes["name"].Value));
                }
                versions.Add(version);
            }

            return versions;
        }

        [Route("CompareFiles")]
        [HttpGet]
        public IList<Service> CompareFiles(string oldRelease, string newRelease, bool noChange = false, bool added = true, bool ignoreNamespaceChanges = false)
        {
            XmlDocument xml = new XmlDocument();
            string path = WebConfigurationManager.AppSettings["MetadataLocalFolder"].ToString();
            xml.Load(System.Web.HttpContext.Current.Server.MapPath(path + oldRelease + "/metadata.xml"));//old file
            Dictionary<string, Service> services = GetServices(xml);

            xml.Load(System.Web.HttpContext.Current.Server.MapPath(path + newRelease + "/metadata.xml"));//new file
            return CompareServices(services, xml, noChange, added, ignoreNamespaceChanges);
        }

        #region Change detection
        /// <summary>
        /// Gets all services and resources in xml file
        /// </summary>
        /// <param name="xml">Old Xml file</param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Create service from xml service node
        /// </summary>
        /// <param name="node">Service node</param>
        /// <returns></returns>
        private Service AddService(XmlNode node)
        {
            Service service = new Service(node.Attributes["name"].Value, node.Attributes["description"].Value, "removed");
            foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
            {
                service.ResourceList.Add(new Resource(leaf.Attributes["name"].Value, leaf.Attributes["hashCode"].Value, leaf.Attributes["noNamspaceHashCode"].Value, "removed"));
            }
            return service;
        }

        /// <summary>
        /// Compares all services in the two xml files
        /// </summary>
        /// <param name="services">Services from old xml</param>
        /// <param name="xml">New xml file</param>
        /// <param name="noChange">Show services with no change</param>
        /// <param name="added">Show services that were added</param>
        /// <param name="ignoreNamespaceChanges">Compare using noNamspaceHashCode instead of hashCode</param>
        /// <returns></returns>
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

        /// <summary>
        /// Compares resources to determine if and how they they have been changed
        /// </summary>
        /// <param name="node">Service node</param>
        /// <param name="service">Existing service</param>
        /// <param name="ignoreNamespaceChanges">Compare using noNamspaceHashCode instead of hashCode</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets service and check if its what consumer asked for
        /// </summary>
        /// <param name="service">Service to be checked</param>
        /// <param name="noChange">If false services that were not changed wont show</param>
        /// <param name="added">If false services that were added wont show</param>
        /// <returns></returns>
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

/*
 *  [HttpGet("{fileName}")]
       public HttpResponseMessage LoadFile(string fileName, string tenantId = null)
       {
           var path = @"C:\Temp\" + fileName;
           HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
           var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
           result.Content = new StreamContent(stream);
           result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
           return result;
       }
 */
