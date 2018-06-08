namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {

            WebResourceLoader webResourceLoader = new WebResourceLoader();
            var rootXmlDocument = new XDocument();
            // XmlIO xmlIO = new XmlIO();
            // List<string> localPathList = new List<string>();
            string rootUrl = "https://intensapp003.internal.visma.com/rest/";
            string rootLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\rest\\";
            // string rootXmlString = webResourceLoader.FetchSiteAsString(rootUrl + "/rest/").Result;
            // rootXmlDocument = XDocument.Parse(rootXmlString);
            // xmlIO.SaveXmlDocument(rootXmlDocument, rootLocalPath, "rest.xml");
            List<Link> links = new List<Link>();

            WebResponse myResponse = GetResponseFromSite(rootUrl);
            Stream myStream = myResponse.GetResponseStream();
            XmlReader xmlReader = XmlReader.Create(myStream);

            Console.WriteLine(rootLocalPath);

            string localPath = rootLocalPath;
            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (xmlReader.Name == "group")
                        {
                            string desc = string.Empty;
                            while (xmlReader.NodeType != XmlNodeType.EndElement)
                            {
                                xmlReader.Read();
                                if (xmlReader.NodeType == XmlNodeType.Text)
                                {
                                    desc = xmlReader.Value;

                                    GetCorrectFilename(ref desc);
                                }
                            }

                            localPath += desc+"\\";

                            Directory.CreateDirectory(localPath);
                            Console.WriteLine(localPath);
                        }
                        else if (xmlReader.Name == "link")
                        {
                            Link tempLink = new Link();
                            while (xmlReader.NodeType != XmlNodeType.Text)
                                xmlReader.Read();

                            tempLink.Href = xmlReader.Value;

                            while (xmlReader.NodeType != XmlNodeType.Text)
                                xmlReader.Read();

                            tempLink.Description = xmlReader.Value;

                            tempLink.Filepath = localPath;

                            links.Add(tempLink);
                        }

                        // Console.WriteLine(xmlReader.Name + " EN");
                        // Console.WriteLine(xmlReader.Value + " EV");
                        break;
                    case XmlNodeType.Text:
                        // Console.WriteLine(xmlReader.Value + " TV");
                        break;
                    case XmlNodeType.EndElement:
                        if(xmlReader.Name == "group")
                        {
                            var lastFolder = Path.GetDirectoryName(localPath);
                            var pathWithoutLastFolder = Path.GetDirectoryName(lastFolder);
                            localPath = pathWithoutLastFolder + "\\";
                        }

                        // Console.WriteLine("END " + xmlReader.Name);
                        break;
                }
            }
            xmlReader.Close();
            myResponse.Close();

            // List of Links is ready

            //LinkList linkList = XmlToLinkList(rootXmlDocument);

            List<string> serverXmlPath = new List<string>();
            List<string> localFilePath = new List<string>();
            List<string> localFileName = new List<string>();
            foreach (Link link in links)
            {
                serverXmlPath.Add(rootUrl + link.Href.Substring(6) + link.Href.Substring(5) + ".wadl");
                serverXmlPath.Add(rootUrl + link.Href.Substring(6) + link.Href.Substring(5) + ".xsd");
                localFileName.Add(link.Href.Substring(6) + ".wadl");
                localFileName.Add(link.Href.Substring(6) + ".xsd");
                localFilePath.Add(link.Filepath);
                localFilePath.Add(link.Filepath);
            }

            webResourceLoader.FetchMultipleXmlAndSaveToDisk(serverXmlPath, localFilePath, localFileName, 60).Wait();

            Console.WriteLine("Complete!");
            Console.ReadKey();
        }

        private static LinkList XmlToLinkList(XDocument xmlDocument)
        {
            LinkList allLinks = new LinkList
            {
                /*
                Links = (from link in xmlDocument.Descendants("link")
                         select new Link
                         {
                             Href = link.Element("href").Value,
                             Description = link.Element("description").Value,
                         }).ToList()
                 */

            };
            return allLinks;
        }

        private static WebResponse GetResponseFromSite(string urlPath)
        {
            WebRequest request = WebRequest.Create(urlPath);
            var credentials = new NetworkCredential("test", "testrest");
            var handler = new HttpClientHandler { Credentials = credentials };
            using (var client = new HttpClient(handler))
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                Console.WriteLine(urlPath);
            }

            WebResponse response = request.GetResponse();

            return response;
            // WebResponse.Close();
        }

        private static void GetCorrectFilename(ref string filename)
        {
            Regex regexSet = new Regex(@"([:*?\<>|])");
            filename = filename.Replace("\\", "&");
            filename = filename.Replace("/", "&");
            filename = regexSet.Replace(filename, "$");
            filename = filename.Replace(" ", "_");
        }
    }
}
