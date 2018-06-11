namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Xml;

    internal class WebResourceReader
    {
        public static List<Link> MainReader(string url, string localPath)
        {
            List<Link> links = new List<Link>();

            WebResponse myResponse = GetResponseFromSite(url);
            Stream myStream = myResponse.GetResponseStream();
            XmlReader xmlReader = XmlReader.Create(myStream);

            XmlWriter xmlWriter = XmlWriter.Create(localPath + "metadata.xml");
            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement("rest_api_metadata");
            xmlWriter.WriteAttributeString("release", "36"); // EXAMPLE DATA
            xmlWriter.WriteAttributeString("version", "510"); // EXAMPLE DATA


            Console.WriteLine(localPath);

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

                                    FixFilename(ref desc);
                                }
                            }

                            localPath += desc + "\\";

                            Directory.CreateDirectory(localPath);
                            Console.WriteLine(localPath);

                            xmlWriter.WriteStartElement("service_group");
                            xmlWriter.WriteAttributeString("name", desc);
                        }
                        else if (xmlReader.Name == "link")
                        {
                            Link tempLink = new Link();
                            while (xmlReader.NodeType != XmlNodeType.Text)
                                xmlReader.Read();

                            tempLink.Href = xmlReader.Value;

                            xmlReader.Read();

                            while (xmlReader.NodeType != XmlNodeType.Text)
                                xmlReader.Read();

                            tempLink.Description = xmlReader.Value;

                            tempLink.Filepath = localPath;

                            links.Add(tempLink);

                            string name = tempLink.Href;
                            int i = name.LastIndexOf("/");
                            name = name.Substring(i + 1);

                            xmlWriter.WriteStartElement("service");
                            xmlWriter.WriteAttributeString("description", tempLink.Description);
                            xmlWriter.WriteAttributeString("name", name);
                            xmlWriter.WriteEndElement();
                        }

                        // Console.WriteLine(xmlReader.Name + " EN");
                        // Console.WriteLine(xmlReader.Value + " EV");
                        break;
                    case XmlNodeType.Text:
                        // Console.WriteLine(xmlReader.Value + " TV");
                        break;
                    case XmlNodeType.EndElement:
                        if (xmlReader.Name == "group")
                        {
                            var lastFolder = Path.GetDirectoryName(localPath);
                            var pathWithoutLastFolder = Path.GetDirectoryName(lastFolder);
                            localPath = pathWithoutLastFolder + "\\";

                            xmlWriter.WriteEndElement();
                        }


                        // Console.WriteLine("END " + xmlReader.Name);
                        break;
                }
            }
            xmlReader.Close();
            myResponse.Close();

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();

            return links;
        }

        private static void FixFilename(ref string filename)
        {
            Regex regexSet = new Regex(@"([:*?\<>|])");
            filename = filename.Replace("\\", "&");
            filename = filename.Replace("/", "&");
            filename = regexSet.Replace(filename, "$");
            filename = filename.Replace(" ", "_");
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
        }
    }
}
