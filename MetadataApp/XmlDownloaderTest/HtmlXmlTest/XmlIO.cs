namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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

        internal List<string> GetfileList(string path)
        {
            List<string> filePaths = new List<string>();
            foreach (string filename in Directory.EnumerateFiles(path, "*.wadl"))
            {
                filePaths.Add(path+filename);
            }

            foreach (string dir in Directory.EnumerateDirectories(path))
            {
                filePaths.AddRange(this.GetfileList(path + dir));
            }

            return filePaths;
        }

        internal List<string> FindAttachments(List<string> filePaths)
        {
            List<string> AttachmentFiles = new List<string>();
            foreach (string filePath in filePaths)
            {
                string fileContents = File.ReadAllText(filePath);
                if (fileContents.Contains("<resource path=\"attachments\">"))
                {
                    AttachmentFiles.Add(fileContents);
                }
            }
            return AttachmentFiles;
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
