namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    internal class Program
    {
        private static void Main(string[] args)
        {
            WebResourceLoader webResourceLoader = new WebResourceLoader();
            string rootUrl = ConfigurationManager.ConnectionStrings["Server"].ConnectionString;

            string rootLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\rest\\";

            List<Link> myLinks = WebResourceReader.MainReader(rootUrl, rootLocalPath);
            List<string> serverXmlPath = new List<string>();
            List<string> localFilePath = new List<string>();
            List<string> localFileName = new List<string>();
            foreach (Link link in myLinks)
            {
                serverXmlPath.Add(rootUrl + link.Href.Substring(6) + link.Href.Substring(5) + ".wadl");
                serverXmlPath.Add(rootUrl + link.Href.Substring(6) + link.Href.Substring(5) + ".xsd");
                localFileName.Add(link.Href.Substring(6) + ".wadl");
                localFileName.Add(link.Href.Substring(6) + ".xsd");
                localFilePath.Add(link.Filepath);
                localFilePath.Add(link.Filepath);
            }
            try
            {
                webResourceLoader.FetchMultipleXmlAndSaveToDisk(serverXmlPath, localFilePath, localFileName, 10).Wait();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }
            XmlIO xmlIO = new XmlIO();
            List<string> filepaths = xmlIO.GetfileList(rootLocalPath);
            List<string> resourcePathsSingle = xmlIO.FindAttachments(filepaths);
            List<string> resourcePathsDouble = xmlIO.DublicateData(resourcePathsSingle);
            List<string> attachmentNames = xmlIO.GetAttachmentNames(resourcePathsSingle);
            List<string> attachmentPaths = xmlIO.attachmentPathGen(resourcePathsDouble, attachmentNames);
            List<string> attachmentUrls = WebResourceLoader.GetAttachmentUrls(attachmentNames, resourcePathsDouble, rootUrl);
            foreach (string path in attachmentUrls)
            {
                Console.WriteLine(path);
            }
            webResourceLoader.FetchMultipleXmlAndSaveToDisk(attachmentUrls, attachmentPaths, attachmentNames, 10).Wait();
            Console.WriteLine("Complete!");
            Console.ReadKey();
        }
    }
}
