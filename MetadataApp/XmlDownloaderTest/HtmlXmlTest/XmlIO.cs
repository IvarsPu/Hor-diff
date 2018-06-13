namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    internal class XmlIO
    {
        internal void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        internal List<string> attachmentPathGen(List<string> ResourcePath, List<string> AttachmentName)
        {
            List<string> attachmentPaths = new List<string>();
            for (int i = 0; i < ResourcePath.Count; i++)
            {
                attachmentPaths.Add(ResourcePath[i].Substring(0, ResourcePath[i].LastIndexOf("\\") + 1) + "attachments\\");
            }
            return attachmentPaths;
        }
        internal List<string> DublicateData(List<string> toDublicate)
        {
            List<string> dublicated = new List<string>();
            foreach(string current in toDublicate)
            {
                dublicated.Add(current);
                dublicated.Add(current);
            }
            return dublicated;
        }

        internal List<string> GetfileList(string path)
        {
            List<string> filePaths = new List<string>();
            foreach (string filename in Directory.EnumerateFiles(path, "*.wadl"))
            {
                filePaths.Add(Path.Combine(path, filename));
            }

            foreach (string dir in Directory.EnumerateDirectories(path))
            {
                filePaths.AddRange(this.GetfileList(Path.Combine(path, dir)));
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
                    select el.Attribute("path").Value
                    );
            }
            return attachmentNames;
        }

        internal async Task<string> SaveAsync(Task<XmlData> xDocument, string path, string fileName)
        {
            this.CreateFolder(path);
            string errorMsg = string.Empty;
            // Console.WriteLine(path + fileName);
            try
            {
                errorMsg = await Task<string>.Run(() =>
                {
                    if (xDocument.Result.Error == string.Empty && xDocument.Result.XDocument != null)
                    {
                        try
                        {
                            xDocument.Result.XDocument.Save(path + fileName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception: {0} whilst saving {1}{2}", ex.Message, path, fileName);
                            throw;
                        }
                    }
                    else
                    {
                        return xDocument.Result.Error;
                    }
                    return string.Empty;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0} whilst saving {1}{2}", ex.Message, path, fileName);
            }

            return errorMsg;

        }

        internal async Task<XmlData> LoadAsync(Task<DownloadData> stringDocument, LoadOptions loadOptions = LoadOptions.PreserveWhitespace)
        {
            try
            {
                return await Task.Run<XmlData>(() =>
                {
                    XmlData xmlData = new XmlData();
                    if (stringDocument.Result.Error == string.Empty && stringDocument.Result.ResponseString.Result != null)
                    {
                        try
                        {
                            xmlData.XDocument = XDocument.Parse(stringDocument.Result.ResponseString.Result, loadOptions);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception: {0} whilst parsing {1}", ex.Message, stringDocument);
                            throw;
                        }
                    }
                    else
                    {
                        xmlData.Error = stringDocument.Result.Error;
                    }

                    return xmlData;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0} whilst parsing {1}", ex.Message, stringDocument);
                throw;
            }
        }
    }
}
