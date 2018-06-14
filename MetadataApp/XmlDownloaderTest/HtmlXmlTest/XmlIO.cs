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

        internal async Task<XmlData> SaveAsync(Task<XmlData> xDocument, string path, string fileName)
        {
            this.CreateFolder(path);
            // Console.WriteLine(path + fileName);
            try
            {
                await Task.Run(() =>
                {
                    if (xDocument.Result.Error == string.Empty && xDocument.Result.XDocument != null)
                    {
                        try
                        {
                            xDocument.Result.XDocument.Save(path + fileName);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(string.Format("Exception: {0} whilst saving {1}{2}", ex.Message, path, fileName));
                            throw;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("Exception: {0} whilst saving {1}{2}", ex.Message, path, fileName));
            }

            return xDocument.Result;

        }

        internal async Task<XmlData> LoadAsync(Task<DownloadData> stringDocument, LoadOptions loadOptions = LoadOptions.PreserveWhitespace)
        {
            try
            {
                return await Task.Run<XmlData>(() =>
                {
                    XmlData xmlData = new XmlData();
                    if (stringDocument.Result.Error == string.Empty && stringDocument.Result.ResponseTask.Result != null)
                    {
                        try
                        {
                            xmlData.XDocument = XDocument.Parse(stringDocument.Result.ResponseTask.Result, loadOptions);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(string.Format("Exception: {0} whilst parsing {1}", ex.Message, stringDocument));
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
                Logger.Log(string.Format("Exception: {0} whilst parsing {1}", ex.Message, stringDocument));
                throw;
            }
        }
    }
}
