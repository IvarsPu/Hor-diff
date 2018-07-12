namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    internal class Program
    {
        private static void Main(string[] args)
        {
            XmlIO xmlIO = new XmlIO();
            WebResourceLoader webResourceLoader = new WebResourceLoader();
            AttachmentLoader attachmentLoader = new AttachmentLoader();
            string rootUrl = ConfigurationManager.ConnectionStrings["Server"].ConnectionString;
            string rootLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\rest\\";
            string metadataPath = null;
            List<XmlFile> wadlAndXmsData = new List<XmlFile>();
            List<XmlFile> attachmentData = new List<XmlFile>();
            HashSet<string> noSchemaServices = new HashSet<string>();
            noSchemaServices.Add("global");
            noSchemaServices.Add("user");
            noSchemaServices.Add("system");
            noSchemaServices.Add("umlmodel");
            noSchemaServices.Add("TdmDimObjSL");
            noSchemaServices.Add("TdmDimObjBL");

            Logger.LogInfo("Loading REST service structure");
            List<Link> myLinks = WebResourceReader.MainReader(rootUrl, ref rootLocalPath, ref metadataPath);
            List<string> serverXmlPath = new List<string>();
            List<string> localFilePath = new List<string>();
            List<string> localFileName = new List<string>();
            foreach (Link link in myLinks)
            {
                string serviceName = link.Href.Substring(6);

                if (!noSchemaServices.Contains(serviceName))
                {
                    serverXmlPath.Add(rootUrl + serviceName + link.Href.Substring(5) + ".wadl");
                    serverXmlPath.Add(rootUrl + serviceName + link.Href.Substring(5) + ".xsd");
                    localFileName.Add(serviceName + ".wadl");
                    localFileName.Add(serviceName + ".xsd");
                    localFilePath.Add(link.Filepath);
                    localFilePath.Add(link.Filepath);
                }
            }            

            try
            {
                Logger.LogInfo("Loading " + myLinks.Count + " service schemas");
                wadlAndXmsData = webResourceLoader.FetchMultipleXmlAndSaveToDisk(serverXmlPath, localFilePath, localFileName, 10).Result;

                Logger.LogInfo("Loading service attachment schemas");
                attachmentData = attachmentLoader.GetAllAttachments(rootLocalPath, rootUrl);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }                             

            Logger.LogInfo("Adjusting metadata");
            wadlAndXmsData.AddRange(attachmentData);
            WebResourceReader.AddFilesToXml(metadataPath, wadlAndXmsData);

            Logger.LogInfo("Generating json tree data");
            JsonGenerator.generateJSONMetadata(rootLocalPath, metadataPath);
            Console.WriteLine("Work complete!");
            Console.ReadKey();
        }


    }
}
