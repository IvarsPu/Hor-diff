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
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public string DiffColor(string firstFile, string secondFile)
        {
            return GenerateDiffHtmlFile(AppInfo.MetadataRootFolder + firstFile, AppInfo.MetadataRootFolder + secondFile);
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
        private string GenerateDiffHtmlFile(string firstFile, string secondFile)
        {
            StringBuilder sb = new StringBuilder();

            string oldText = "";
            if (File.Exists(firstFile))
            {
                oldText = File.ReadAllText(firstFile);
            }

            string newText = "";
            if (File.Exists(secondFile))
            {
                newText = File.ReadAllText(secondFile);
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

        private string GenerateHtmlDiff(Schema schema, string path)
        {
            String fileName = schema.Title;
            String filePath = path;

            string exportFolder = AppInfo.FolderLocation + Result + "/" + AppInfo.HtmlRootFolder + "/" + filePath;
            if (!Directory.Exists(exportFolder))
            {
                Directory.CreateDirectory(exportFolder);
            }

            string firstFilePath = AppInfo.MetadataRootFolder + First + "/" + filePath + fileName;
            string secondFilePath = AppInfo.MetadataRootFolder + Second + "/" + filePath + fileName;

            string htmlFileName = fileName.Replace('.', '_') + ".html";
            string htmlFilePath = exportFolder + "/" + htmlFileName;

            try
            {
                File.WriteAllText(htmlFilePath, GenerateDiffHtmlFile(firstFilePath, secondFilePath), Encoding.UTF8);
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

        private List<object> GetChildren(XmlNode node)
        {
            List<object> elements = new List<object>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name.Equals("service") || child.Name.Equals("service_group") || child.Name.Equals("resource"))
                {
                    Folder folder = new Folder();
                    folder.Title = child.Attributes["name"].Value;
                    folder.Type = child.Name;
                    folder.Children = GetChildren(child);
                    folder.ExtraClasses = "service_changed";
                    elements.Add(folder);
                }
                else
                {
                    Schema schema = new Schema();
                    schema.Title = child.Attributes["name"].Value;
                    schema.HashCode = child.Attributes["hashCode"].Value;
                    schema.NoNamspaceHashCode = child.Attributes["noNamspaceHashCode"].Value;
                    schema.Type = child.Name;
                    schema.ExtraClasses = "doc_changed";
                    elements.Add(schema);
                }
            }
            return elements;
        }
        #endregion

        #region compare
        private void GenerateReport(string first, string second)
        {
            XmlDocument firstXml = new XmlDocument();
            firstXml.Load(AppInfo.MetadataRootFolder + first + "/metadata.xml");

            XmlDocument secondXml = new XmlDocument();
            secondXml.Load(AppInfo.MetadataRootFolder + second + "/metadata.xml");

            Folder firstObjects = AddClass(firstXml);
            Folder secondObjects = AddClass(secondXml);
            Test(firstObjects, secondObjects, "");
            
            string json = "var JsonTree = " + JsonConvert.SerializeObject(secondObjects);
            File.WriteAllText(AppInfo.FolderLocation + Result + "/" + AppInfo.JsonTreeFileName, json);

            MakeLocalZip(first, second);
        }
        
        private void Test(Folder first, Folder second, string path)
        {
            List<object> remove = new List<object>();
            foreach (object element2 in second.Children)
            {
                bool found = false;
                foreach (object element1 in first.Children)
                {
                    if (element1.GetType() == typeof(Folder) && element2.GetType() == typeof(Folder))
                    {
                        Folder folder1 = (Folder)element1;
                        Folder folder2 = (Folder)element2;
                        if (folder1.Title.Equals(folder2.Title))
                        {
                            //exists
                            found = true;
                            Test(folder1, folder2, path + folder2.Title + "/");
                            if (folder2.Children.Count == 0)
                            {
                                remove.Add(folder2);
                            }
                        }
                    }
                    else if (element1.GetType() == typeof(Schema) && element2.GetType() == typeof(Schema))
                    {
                        Schema schema1 = (Schema)element1;
                        Schema schema2 = (Schema)element2;
                        if (schema1.Title.Equals(schema2.Title))
                        {
                            //exists
                            found = true;
                            if (schema1.HashCode == schema2.HashCode || String.IsNullOrEmpty(schema1.HashCode) || String.IsNullOrEmpty(schema2.HashCode))
                            {
                                //no change
                                remove.Add(schema2);
                            }
                            else
                            {
                                //change
                                schema2.DiffHtmlFile = GenerateHtmlDiff(schema2, path);
                            }
                        }
                    }
                }
                if (!found)
                {
                    //added
                    if (element2.GetType() == typeof(Folder))
                    {
                        Folder folder2 = (Folder)element2;
                        folder2.ExtraClasses = "service_new";
                        MarkElements(folder2, path + folder2.Title + "/");
                    }
                    else if (element2.GetType() == typeof(Schema))
                    {
                        Schema schema2 = (Schema)element2;
                        schema2.DiffHtmlFile = GenerateHtmlDiff(schema2, path);
                        schema2.ExtraClasses = "doc_new";
                    }
                }
            }

            foreach (object obj in remove) second.Children.Remove(obj);
        }

        private void MarkElements(Folder folder, string path)
        {
            foreach(object element in folder.Children)
            {
                if (element.GetType() == typeof(Folder))
                {
                    Folder childFolder = (Folder)element;
                    childFolder.ExtraClasses = "service_new";
                    MarkElements(childFolder, path + childFolder.Title + "/");
                }
                else if (element.GetType() == typeof(Schema))
                {
                    Schema schema = (Schema)element;
                    schema.DiffHtmlFile = GenerateHtmlDiff(schema, path);
                    schema.ExtraClasses = "doc_new";
                }
            }
        }
        #endregion
    }
}
