namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {

            WebResourceLoader webResourceLoader = new WebResourceLoader();
            var rootXmlDocument = new XDocument();
            XmlIO xmlIO = new XmlIO();
            List<string> localPathList = new List<string>();
            string rootUrl = "https://intensapp003.internal.visma.com";
            string rootLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\rest\\";
            string rootXmlString = webResourceLoader.FetchSiteAsString(rootUrl + "/rest/").Result;
            rootXmlDocument = XDocument.Parse(rootXmlString);
            xmlIO.SaveXmlDocument(rootXmlDocument, rootLocalPath, "rest.xml");


            LinkList linkList = XmlToLinkList(rootXmlDocument);

            List<string> serverXmlPath = new List<string>();
            List<string> localFilePath = new List<string>();
            List<string> localFileName = new List<string>();
            foreach (Link link in linkList)
            {
                serverXmlPath.Add(rootUrl + link.Href);
                localFileName.Add(link.Href.Substring(6) + ".xml");
                localFilePath.Add(rootLocalPath);
            }

            Task.WaitAll(webResourceLoader.FetchMultipleXmlAndSaveToDisk(serverXmlPath, localFilePath, localFileName, 15));

            Console.WriteLine("Complete!");
            Console.ReadKey();
        }

        private static LinkList XmlToLinkList(XDocument xmlDocument)
        {
            LinkList allLinks = new LinkList
            {
                Links = (from link in xmlDocument.Elements("link")
                         select new Link
                         {
                             Href = link.Element("href").Value,
                             Description = link.Element("description").Value,
                         }).ToList()
            };
            return allLinks;
        }
    }
}
