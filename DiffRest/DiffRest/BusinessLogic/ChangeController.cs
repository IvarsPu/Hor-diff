using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Xml;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Models;
using Newtonsoft.Json;

namespace BusinessLogic
{
    public class ChangeController
    {
        public string Result, First, Second;

        public List<HorizonVersion> GetHorizonVersions()
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(AppInfo.MetadataRootFolder + "Versions.xml");

                List<HorizonVersion> versions = new List<HorizonVersion>();

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
            catch
            {
                return null;
            }
        }

        public HttpResponseMessage LoadFile(string first, string second)
        {
            try
            {
                First = first;
                Second = second;
                Result = (first + "_" + second).Replace('/', '.');

                string path = AppInfo.FolderLocation + Result + ".zip";
                if (!File.Exists(path))
                {
                    GenerateReport(first, second);
                }

                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Result + ".zip"
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                return result;
            }
            catch
            {
                return null;
            }
        }

        public string DiffColor(string firstFile, string secondFile)
        {
            StringBuilder sb = new StringBuilder();

            string oldText = "";
            if (File.Exists(AppInfo.MetadataRootFolder + firstFile))
            {
                oldText = File.ReadAllText(AppInfo.MetadataRootFolder + firstFile);
            }

            string newText = "";
            if (File.Exists(AppInfo.MetadataRootFolder + secondFile))
            {
                newText = File.ReadAllText(AppInfo.MetadataRootFolder + secondFile);
            }

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

            return sb.ToString();
        }

        public string GetFile(string filePath)
        {
            if (File.Exists(AppInfo.MetadataRootFolder + filePath))
            {
                return File.ReadAllText(AppInfo.MetadataRootFolder + filePath);
            }
            return null;
        }

        #region Make zip
        private void MakeLocalZip(string first, string second)
        {
            Copy(AppInfo.FolderLocation + "Site", AppInfo.FolderLocation + Result);

            string[] arrLine = File.ReadAllLines(AppInfo.FolderLocation + Result + "/js/main.js");
            arrLine[5 - 1] = "var firstVersion = '" + first + "';";
            arrLine[6 - 1] = "var secondVersion = '" + second + "';";
            File.WriteAllLines(AppInfo.FolderLocation + Result + "/js/main.js", arrLine);

            string zip = AppInfo.FolderLocation + Result + ".zip";
            foreach (string file in Directory.GetFiles(AppInfo.FolderLocation, "*.zip"))
            {
                File.Delete(file);
            }
            ZipFile.CreateFromDirectory(AppInfo.FolderLocation + Result, zip);

            Delete();
        }

        private void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), overwrite: true);

            foreach (var directory in Directory.GetDirectories(sourceDir))
                Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }

        private void Delete()
        {
            string[] directoryPaths = Directory.GetDirectories(AppInfo.FolderLocation);
            foreach (string directoryPath in directoryPaths)
            {
                var name = new DirectoryInfo(directoryPath).Name;
                if (name != "Site")
                {
                    Directory.Delete(directoryPath, true);
                }
            }
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

            string exportFolder = AppInfo.FolderLocation + Result + "/" + AppInfo.HtmlRootFolder + "/" + filePath;
            if (!Directory.Exists(exportFolder))
            {
                Directory.CreateDirectory(exportFolder);
            }

            string firstFilePath = AppInfo.MetadataRootFolder + First + "/" + filePath + "/" + fileName;
            string secondFilePath = AppInfo.MetadataRootFolder + Second + "/" + filePath + "/" + fileName;

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
        private Folder AddClass(XmlDocument xml)
        {
            Folder folder = new Folder();
            XmlNode node = xml.SelectSingleNode("//rest_api_metadata");
            folder.Title = "root";
            folder.Type = node.Name;
            folder.Children = GetChildren(node);

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
                    folder.Title = child.Attributes["name"].Value;
                    folder.Type = child.Name;
                    folder.Children = GetChildren(child);
                    elements.Add(folder);
                }
                else
                {
                    Schema schema = new Schema();
                    schema.Title = child.Attributes["name"].Value;
                    schema.HashCode = Int32.Parse(child.Attributes["hashCode"].Value);
                    schema.NoNamspaceHashCode = Int32.Parse(child.Attributes["noNamspaceHashCode"].Value);
                    schema.Type = child.Name;
                    try
                    {
                        schema.DiffHtmlFile = child.Attributes["diffHtmlFile"].Value;
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
        public void GenerateReport(string first, string second)
        {
            XmlDocument firstXml = new XmlDocument();
            firstXml.Load(AppInfo.MetadataRootFolder + first + "/metadata.xml");

            XmlDocument secondXml = new XmlDocument();
            secondXml.Load(AppInfo.MetadataRootFolder + second + "/metadata.xml");

            secondXml = Compare(firstXml, secondXml);
            secondXml.RemoveChild(secondXml.FirstChild);

            string json = "var JsonTree = " + JsonConvert.SerializeObject(AddClass(secondXml));
            File.WriteAllText(AppInfo.FolderLocation + Result + "/" + AppInfo.JsonTreeFileName, json);

            MakeLocalZip(first, second);
        }

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
