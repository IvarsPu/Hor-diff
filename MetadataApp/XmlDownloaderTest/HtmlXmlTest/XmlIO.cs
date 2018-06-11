namespace HtmlXmlTest
{
    using System;
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

        internal async Task<string> SaveAsync(Task<XmlData> xDocument, string path, string fileName)
        {
            this.CreateFolder(path);

            // Console.WriteLine(path + fileName);
            string errorMsg = await Task<string>.Run(() =>
            {
                if (xDocument.Result.Error == string.Empty && xDocument.Result.XDocument != null)
                {
                    xDocument.Result.XDocument.Save(path + fileName);
                }
                else
                {
                    return xDocument.Result.Error;
                }
                return string.Empty;
            });
            return errorMsg;

            // .NET Core 2.0 Has SaveAsync
        }

        internal async Task<XmlData> LoadAsync(Task<DownloadData> stringDocument, LoadOptions loadOptions = LoadOptions.PreserveWhitespace)
        {
            return await Task.Run<XmlData>(() =>
            {
                XmlData xmlData = new XmlData();
                if (stringDocument.Result.Error == string.Empty && stringDocument.Result.ResponseString.Result != null)
                {
                    xmlData.XDocument = XDocument.Parse(stringDocument.Result.ResponseString.Result, loadOptions);
                }
                else
                {
                    xmlData.Error = stringDocument.Result.Error;
                }

                return xmlData;
            });
        }
    }
}
