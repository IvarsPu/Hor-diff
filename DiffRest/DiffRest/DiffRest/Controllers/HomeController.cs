using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using DiffRest.Models;

namespace DiffRest.Controllers
{
    public class HomeController : Controller
    {
        [Route("Home/Index")]
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Versions = GetVersions();

            return View();
        }

        [Route("Home/GetVersions")]
        [HttpGet]
        public List<KeyValuePair<string, List<string>>> GetVersions()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(Server.MapPath(@"~/rest_sample/Versions.xml"));

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

        [Route("Home/CompareFiles")]
        [HttpGet]
        public Dictionary<string, Service> CompareFiles(string oldRelease, string newRelease, bool noChange = false, bool added = true)
        {
            XmlDocument xml = new XmlDocument();
            Dictionary<string, Service> services = new Dictionary<string, Service>();

            xml.Load(Server.MapPath(@"~/rest_sample/" + oldRelease + "/metadata.xml"));
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

            xml.Load(Server.MapPath(@"~/rest_sample/" + newRelease + "/metadata.xml"));
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

        #region Change detection
        private Service AddService(XmlNode node)
        {
            Service service = new Service(node.Attributes["description"].Value);
            foreach (XmlNode leaf in node.SelectNodes("*[count(child::*) = 0]"))
            {
                service.ResourceList.Add(new Resource(leaf.Attributes["name"].Value, leaf.Attributes["hashCode"].Value, leaf.Attributes["noNamspaceHashCode"].Value, "removed"));
            }
            return service;
        }

        private Service SearchService(XmlNode node, Service service, bool noChange, bool added)
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
        #endregion
    }
}