using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffRest.Models;
using System.IO;
using System.Web.Configuration;
using System;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace DiffRest.Controllers
{
    [RoutePrefix("Change")]
    public class ChangeController : Controller
    {
        private static string FolderLocation = System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["testPlace"].ToString() + "Projects/");
        private static string MetadatRootFolder = System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["testPlace"].ToString() +  WebConfigurationManager.AppSettings["MetadataLocalFolder"].ToString());
        private static string JsonTreeFileName = "tree_data.js";
        private static string HtmlRootFolder = "REST_DIFF";

        private string FirstVersion;
        private string FirstRelease;
        private string SecondVersion;
        private string SecondRelease;
        private string Company;

        // GET: Change
        public ActionResult Index()
        {
            return View();
        }

        //http://localhost:51458/Change/GenerateReport?firstVersion=515&secondVersion=520&firstRelease=3&secondRelease=1&company=test
        [Route("GenerateReport")]
        [HttpGet]
        public void GenerateReport(string firstVersion, string firstRelease, string secondVersion, string secondRelease, string company)
        {
            FirstVersion = firstVersion;
            FirstRelease = firstRelease;
            SecondVersion = secondVersion;
            SecondRelease = secondRelease;
            Company = company;

            XmlDocument firstXml = new XmlDocument();
            firstXml.Load(MetadatRootFolder + FirstVersion + "/" + FirstRelease + "/metadata.xml");

            XmlDocument secondXml = new XmlDocument();
            secondXml.Load(MetadatRootFolder + SecondVersion + "/" + SecondRelease + "/metadata.xml");

            secondXml = Compare(firstXml, secondXml);
            secondXml.RemoveChild(secondXml.FirstChild);

            string json = "var JsonTree = " + JsonConvert.SerializeObject(AddClass(secondXml));
            System.IO.File.WriteAllText(FolderLocation + Company + "/" + JsonTreeFileName, json);

            MakeLocalCopy();
        }

        //http://localhost:51458/Change/LoadFile?fileName=test
        [Route("LoadFile")]
        [HttpGet]
        public HttpResponseMessage LoadFile(string fileName)
        {
            var path = FolderLocation + fileName + ".zip";
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }

        #region Make copy
        private void MakeLocalCopy()
        {
            Copy(FolderLocation + "Site", FolderLocation + Company);

            string zip = FolderLocation + Company + ".zip";
            if (System.IO.File.Exists(zip))
            {
                System.IO.File.Delete(zip);
            }
            ZipFile.CreateFromDirectory(FolderLocation + Company, zip);
        }

        private void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                System.IO.File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), overwrite: true);

            foreach (var directory in Directory.GetDirectories(sourceDir))
                Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }
        #endregion

        #region Text Color Generator
        private void GenerateDiffHtmlFile(string firstFile, string secondFile, string resultFilePath)
        {
            StringBuilder sb = new StringBuilder();

            string oldText = System.IO.File.ReadAllText(firstFile);
            string newText = System.IO.File.ReadAllText(secondFile);

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
            System.IO.File.WriteAllText(resultFilePath, sb.ToString(), Encoding.UTF8);
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

            string exportFolder = FolderLocation + Company + "/" + HtmlRootFolder + "/" + filePath;
            if (!Directory.Exists(exportFolder))
            {
                Directory.CreateDirectory(exportFolder);
            }

            string firstFilePath = MetadatRootFolder + FirstVersion + "/" + FirstRelease + "/" + filePath + "/" + fileName;
            string secondFilePath = MetadatRootFolder + SecondVersion + "/" + SecondRelease + "/" + filePath + "/" + fileName;

            string htmlFileName = fileName.Replace('.', '_') + ".html";
            string htmlFilePath = exportFolder + "/" + htmlFileName;

            try
            {
                GenerateDiffHtmlFile(firstFilePath, secondFilePath, htmlFilePath);
            }
            catch (Exception)
            {}
            filePath = filePath + htmlFileName;
            return filePath;
        }
        #endregion

        #region fill objects
        private Folder AddClass(XmlDocument xml)
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
                    {}

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

        private XmlNode Get(XmlNode node, XmlDocument xml)
        {
            XmlNode child = xml.SelectSingleNode("//" + node.ParentNode.Name + "[@name='" + node.ParentNode.Attributes["name"].Value + "']");
            if (child == null)
            {
                child = Get(node, xml);
            }

            return node;
        }
        #endregion
    }
}
