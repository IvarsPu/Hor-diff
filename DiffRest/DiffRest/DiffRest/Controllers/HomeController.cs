using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DiffRest.Models;
using System.Web.Http;
using System.Web.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Net;
using System.IO.Compression;
using System.Text;
using DiffPlex.DiffBuilder;
using DiffPlex;
using DiffPlex.DiffBuilder.Model;
using System.Web;
using System;

namespace DiffRest.Controllers
{
    [RoutePrefix("Home")]
    public class HomeController : ApiController
    {
        private static string FolderLocation = HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["testPlace"].ToString() + "Projects/");
        private static string MetadatRootFolder = HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["testPlace"].ToString() + WebConfigurationManager.AppSettings["MetadataLocalFolder"].ToString());
        private static string JsonTreeFileName = "tree_data.js";
        private static string HtmlRootFolder = "REST_DIFF";

        private string First;
        private string Second;
        private string Result;

        [Route("GetVersions")]
        [HttpGet]
        public IList<HorizonVersion> GetVersions()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(MetadatRootFolder + "Versions.xml");

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
            xml.Load(MetadatRootFolder + oldRelease + "/metadata.xml");//old file
            Dictionary<string, Service> services = GetServices(xml);

            xml.Load(MetadatRootFolder + newRelease + "/metadata.xml");//new file
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
        
        [Route("GenerateReport")]
        [HttpGet]
        public object GenerateReport(string first, string second)
        {
            if (!first.Equals(second))
            {
                First = first;
                Second = second;
                Result = (First + "_" + Second).Replace('/', '.');
                if (File.Exists(FolderLocation + Result + "/" + JsonTreeFileName))
                {
                    string j = File.ReadAllText(FolderLocation + Result + "/" + JsonTreeFileName);
                    j = j.Substring(15);
                    object obj = JsonConvert.DeserializeObject(j);
                    return obj;
                }

                XmlDocument firstXml = new XmlDocument();
                firstXml.Load(MetadatRootFolder + First + "/metadata.xml");

                XmlDocument secondXml = new XmlDocument();
                secondXml.Load(MetadatRootFolder + Second + "/metadata.xml");

                secondXml = Compare(firstXml, secondXml);
                secondXml.RemoveChild(secondXml.FirstChild);

                Folder folder = AddClass(secondXml);

                string json = "var JsonTree = " + JsonConvert.SerializeObject(folder);
                File.WriteAllText(FolderLocation + Result + "/" + JsonTreeFileName, json);

                MakeLocalCopy();

                return folder;
            }
            else
            {
                return null;
            }
        }
        
        [Route("LoadFile")]
        [HttpGet]
        public HttpResponseMessage LoadFile(string first, string second)
        {
            string fileName = (first + "_" + second).Replace('/', '.') + ".zip";
            string path = FolderLocation + fileName;
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            return result;
        }

        #region Make copy
        private void MakeLocalCopy()
        {
            Copy(FolderLocation + "Site", FolderLocation + Result);

            string zip = FolderLocation + Result + ".zip";
            if (File.Exists(zip))
            {
                File.Delete(zip);
            }
            ZipFile.CreateFromDirectory(FolderLocation + Result, zip);
        }

        private void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), overwrite: true);

            foreach (var directory in Directory.GetDirectories(sourceDir))
                Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }
        #endregion

        #region Text Color Generator
        private void GenerateDiffHtmlFile(string firstFile, string secondFile, string resultFilePath)
        {
            StringBuilder sb = new StringBuilder();

            string oldText = File.ReadAllText(firstFile);
            string newText = File.ReadAllText(secondFile);

            var d = new Differ();
            var builder = new InlineDiffBuilder(d);
            var result = builder.BuildDiffModel(oldText, newText);
            sb.Append("<html>\n" +
                "<head>\n" +
                "<meta charset='UTF-8'>\n" +
                "<style>\n" +
                "span { color: gray;  white-space: pre; }\n" +
                ".deleted { color:black; background-color:red; } \n" +
                ".new { color:black; background-color:yellow; }\n " +
                "</style>\n" +
                "</head>\n" +
                "<body>\n");
            foreach (var line in result.Lines)
            {
                if (line.Type == ChangeType.Inserted)
                {
                    sb.Append("<span class='new'>");
                }
                else if (line.Type == ChangeType.Deleted)
                {
                    sb.Append("<span class='deleted'>");
                }
                else if (line.Type == ChangeType.Unchanged)
                {
                    sb.Append("<span>");
                }
                sb.Append(HttpUtility.HtmlEncode(line.Text) + "</span><br/>\n");
            }
            sb.Append("</body>\n" +
                "</html>");
            File.WriteAllText(resultFilePath, sb.ToString(), Encoding.UTF8);
        }

        private string GenerateHtmlDiff(XmlNode node)
        {
            String fileName = node.Attributes["name"].Value;
            String filePath = "";

            //Get file path
            XmlNode fileNode = node.ParentNode;
            do
            {
                filePath = fileNode.Attributes["name"].Value + "/" + filePath;
                fileNode = fileNode.ParentNode;
            } while (!fileNode.Name.Equals("rest_api_metadata"));

            string exportFolder = FolderLocation + Result + "/" + HtmlRootFolder + "/" + filePath;
            if (!Directory.Exists(exportFolder))
            {
                Directory.CreateDirectory(exportFolder);
            }

            string firstFilePath = MetadatRootFolder + First + "/" + filePath + "/" + fileName;
            string secondFilePath = MetadatRootFolder + Second + "/" + filePath + "/" + fileName;

            string htmlFileName = fileName.Replace('.', '_') + ".html";
            string htmlFilePath = exportFolder + "/" + htmlFileName;

            try
            {
                GenerateDiffHtmlFile(firstFilePath, secondFilePath, htmlFilePath);
            }
            catch (Exception)
            { }
            filePath = filePath + htmlFileName;
            return filePath;
        }
        #endregion

        #region fill objects
        public Folder AddClass(XmlDocument xml)
        {
            Folder folder = new Folder();
            XmlNode node = xml.SelectSingleNode("//rest_api_metadata");
            folder.title = "root";
            folder.type = node.Name;
            folder.children = GetChildren(node);

            return folder;
        }

        private IList<IElement> GetChildren(XmlNode node)
        {
            IList<IElement> elements = new List<IElement>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name.Equals("service") || child.Name.Equals("service_group") || child.Name.Equals("resource"))
                {
                    Folder folder = new Folder();
                    folder.title = child.Attributes["name"].Value;
                    folder.type = child.Name;
                    folder.children = GetChildren(child);
                    elements.Add(folder);
                }
                else
                {
                    Schema schema = new Schema();
                    schema.title = child.Attributes["name"].Value;
                    schema.hashCode = Int32.Parse(child.Attributes["hashCode"].Value);
                    schema.noNamspaceHashCode = Int32.Parse(child.Attributes["noNamspaceHashCode"].Value);
                    schema.type = child.Name;
                    try
                    {
                        schema.diffHtmlFile = child.Attributes["diffHtmlFile"].Value;
                    }
                    catch (Exception)
                    { }

                    elements.Add(schema);
                }
            }
            return elements;
        }
        #endregion

        #region compare
        private XmlDocument Compare(XmlDocument firstXml, XmlDocument secondXml)
        {
            foreach (XmlNode node in firstXml.SelectNodes("//service/*[count(child::*) = 0]"))
            {
                string serviceName = node.ParentNode.Attributes["name"].Value;
                XmlNode child = secondXml.SelectSingleNode("//service[@name='" + serviceName + "']/" + node.Name + "[@name='" + node.Attributes["name"].Value + "']");
                if (child != null)
                {
                    if (child.Attributes["hashCode"].Value.Equals(node.Attributes["hashCode"].Value)
                        || child.Attributes["hashCode"].Value.Equals("-1")
                        || node.Attributes["hashCode"].Value.Equals("-1")) //Do not export errors
                    {
                        //not changed
                        node.ParentNode.RemoveChild(node);
                    }
                    else
                    {
                        AddXmlAttribute(node, "diffHtmlFile", GenerateHtmlDiff(node));
                    }
                }
                /*          else
                          {
                              //removed
                              XmlNode t = Get(node, secondXml);
                              child = secondXml.SelectSingleNode("//" + t.ParentNode.Name + "[@name='" + t.ParentNode.Attributes["name"].Value + "']");

                              XmlNode newBook = secondXml.ImportNode(t, true);
                              child.AppendChild(newBook);
                          } */
            }


            //The same for attachments
            foreach (XmlNode node in firstXml.SelectNodes("//service/resource/*[count(child::*) = 0]"))
            {
                string serviceName = node.ParentNode.ParentNode.Attributes["name"].Value;
                XmlNode child = secondXml.SelectSingleNode("//service[@name='" + serviceName + "']/resource/" + node.Name + "[@name='" + node.Attributes["name"].Value + "']");
                if (child != null)
                {
                    if (child.Attributes["hashCode"].Value.Equals(node.Attributes["hashCode"].Value)
                        || child.Attributes["hashCode"].Value.Equals("-1")
                        || node.Attributes["hashCode"].Value.Equals("-1")) //Do not export errors
                    {
                        //not changed
                        node.ParentNode.RemoveChild(node);
                    }
                    else
                    {
                        AddXmlAttribute(node, "diffHtmlFile", GenerateHtmlDiff(node));
                    }
                }
            }
            //Remove unmodified attachments
            foreach (XmlNode node in firstXml.SelectNodes("//resource[count(child::*) = 0]"))
            {
                node.ParentNode.RemoveChild(node);
            }
            //Remove unmodified services
            foreach (XmlNode node in firstXml.SelectNodes("//service[count(child::*) = 0]"))
            {
                node.ParentNode.RemoveChild(node);
            }

            //Remove unmodified service groups
            foreach (XmlNode node in firstXml.SelectNodes("//service_group[count(child::*) = 0]"))
            {
                node.ParentNode.RemoveChild(node);
            }
            //Remove unmodified service parent groups
            foreach (XmlNode node in firstXml.SelectNodes("//service_group[count(child::*) = 0]"))
            {
                node.ParentNode.RemoveChild(node);
            }
            return firstXml;
        }

        private void AddXmlAttribute(XmlNode node, String attrName, String attrValue)
        {
            XmlDocument doc = node.OwnerDocument;
            XmlAttribute attr = doc.CreateAttribute(attrName);
            attr.Value = attrValue;
            node.Attributes.SetNamedItem(attr);
        }
        #endregion
    }
}
