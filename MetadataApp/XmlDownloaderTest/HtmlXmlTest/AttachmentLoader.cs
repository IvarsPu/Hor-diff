namespace HtmlXmlTest
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    internal class AttachmentLoader
    {
        internal List<XmlFile> GetAllAttachments(string rootLocalPath, string rootUrl)
        {
            WebResourceLoader webResourceLoader = new WebResourceLoader();
            List<XmlFile> attachmentData = new List<XmlFile>();
            List<string> filepaths = this.GetWadlfileList(rootLocalPath);
            List<string> resourcePathsSingle = this.FindAttachments(filepaths);
            List<string> resourcePathsDouble = this.DublicateData(resourcePathsSingle);
            List<string> attachmentNames = this.GetAttachmentNames(resourcePathsSingle);
            List<string> attachmentPaths = this.AttachmentPathGen(resourcePathsDouble);
            List<string> attachmentUrls = WebResourceLoader.GetAttachmentUrls(attachmentNames, resourcePathsDouble, rootUrl);
            attachmentData = webResourceLoader.FetchMultipleXmlAndSaveToDisk(attachmentUrls, attachmentPaths, attachmentNames, 10).Result;
            foreach (XmlFile attachment in attachmentData)
            {
                attachment.Attachment = true;
            }

            return attachmentData;
        }

        internal List<string> AttachmentPathGen(List<string> resourcePath)
        {
            List<string> attachmentPaths = new List<string>();
            for (int i = 0; i < resourcePath.Count; i++)
            {
                attachmentPaths.Add(resourcePath[i].Substring(0, resourcePath[i].LastIndexOf("\\") + 1) + "attachments\\");
            }

            return attachmentPaths;
        }

        internal List<string> DublicateData(List<string> toDublicate)
        {
            List<string> dublicated = new List<string>();
            foreach (string current in toDublicate)
            {
                dublicated.Add(current);
                dublicated.Add(current);
            }

            return dublicated;
        }

        internal List<string> GetWadlfileList(string path)
        {
            List<string> filePaths = new List<string>();
            foreach (string filename in Directory.EnumerateFiles(path, "*.wadl"))
            {
                filePaths.Add(Path.Combine(path, filename));
            }

            foreach (string dir in Directory.EnumerateDirectories(path))
            {
                filePaths.AddRange(this.GetWadlfileList(Path.Combine(path, dir)));
            }

            return filePaths;
        }

        internal List<string> FindAttachments(List<string> filePaths)
        {
            List<string> attachmentPaths = new List<string>();
            foreach (string filePath in filePaths)
            {
                string fileContents = File.ReadAllText(filePath);
                if (fileContents.Contains("<resource path=\"attachments\">"))
                {
                    attachmentPaths.Add(filePath);
                }
            }
            return attachmentPaths;
        }

        internal List<string> GetAttachmentNames(List<string> attachmentPaths) // Don't touch, it works
        {
            List<string> attachmentNames = new List<string>();
            List<XElement> attachments = new List<XElement>();
            string xmlNameSpace = "{http://wadl.dev.java.net/2009/02}";
            foreach (string attachment in attachmentPaths)
            {
                XDocument xmlAttachment = XDocument.Parse(File.ReadAllText(attachment));
                attachments.AddRange(
                    from el in xmlAttachment.Descendants()
                    where el.Name.LocalName == "resource" && el.Attribute("path") != null && el.Attribute("path").Value == "attachments"
                    select el);
            }

            foreach (XElement attachment in attachments)
            {
                attachmentNames.AddRange(
                    from el in attachment.Descendants(xmlNameSpace + "resource")
                    where el.Attribute("path") != null && el.Attribute("path").Value != @"{pk}" && el.Attribute("path").Value != @"{filename}"
                    select el.Attribute("path").Value);
            }

            return attachmentNames;
        }
    }
}
