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

        internal async Task SaveAsync(Task<XDocument> xmlDocument, string path, string fileName)
        {
            this.CreateFolder(path);
            //Console.WriteLine(path + fileName);
            await Task.Run(() =>
            {
                xmlDocument.Result.Save(path + fileName);
            });

            // .NET Core 2.0 Has SaveAsync
        }

        internal async Task<XDocument> LoadAsync(Task<string> stringDocument, LoadOptions loadOptions = LoadOptions.PreserveWhitespace)
        {
            return await Task.Run<XDocument>(() => XDocument.Parse(stringDocument.Result, loadOptions));

        }





    }
}
