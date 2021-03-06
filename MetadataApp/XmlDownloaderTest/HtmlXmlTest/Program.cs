﻿namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    internal class Program
    {
        private static void Main(string[] args)
        {
            // Sorry, we just didn't have enough time to make it pretty & readable

            XmlIO xmlIO = new XmlIO();
            WebResourceLoader webResourceLoader = new WebResourceLoader();
            AttachmentLoader attachmentLoader = new AttachmentLoader();
            string rootUrl = ConfigurationManager.ConnectionStrings["Server"].ConnectionString;
            string rootLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\rest\\";
            List<XmlFile> wadlAndXmsData = new List<XmlFile>();
            List<XmlFile> attachmentData = new List<XmlFile>();
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
               wadlAndXmsData = webResourceLoader.FetchMultipleXmlAndSaveToDisk(serverXmlPath, localFilePath, localFileName, 10).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            attachmentData = attachmentLoader.GetAllAttachments(rootLocalPath, rootUrl);
            Console.WriteLine("Complete!");
            Console.ReadKey();
        }
    }
}
