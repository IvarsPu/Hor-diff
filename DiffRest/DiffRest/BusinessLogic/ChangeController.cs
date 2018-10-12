using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Xml;
using DataLoader;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Models;

namespace BusinessLogic
{
    public class ChangeController
    {
        public List<HorizonVersion> GetHorizonVersions()
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

        public HttpResponseMessage LoadFile(string first, string second)
        {
            ManageFiles manageFiles = new ManageFiles();
            manageFiles.First = first;
            manageFiles.Second = second;
            manageFiles.Result = (first + "_" + second).Replace('/', '.');

            string path = AppInfo.FolderLocation + manageFiles.Result + ".zip";
            if (!File.Exists(path))
            {
                manageFiles.GenerateReport(first, second);
            }

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = manageFiles.Result + ".zip"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            return result;
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
            try
            {
                if (File.Exists(AppInfo.MetadataRootFolder + filePath))
                {
                    return File.ReadAllText(AppInfo.MetadataRootFolder + filePath);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
