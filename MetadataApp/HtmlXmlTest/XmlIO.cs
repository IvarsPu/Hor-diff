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

        internal async Task SaveXmlDocument (XDocument xmlDocument, string path, string fileName)
        {
            this.CreateFolder(path);
            Console.WriteLine(path + fileName);
            await Task.Run(() => xmlDocument.Save(path + fileName));

            // .NET Core 2.0 Has SaveAsync
        }





    }
}
