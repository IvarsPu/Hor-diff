namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    internal class WebResourceLoader
    {
        internal async Task<string> FetchSiteAsString(string urlPath)
        {
            string siteContentString;
            var credentials = new NetworkCredential("test", "testrest");
            var handler = new HttpClientHandler { Credentials = credentials };
            using (var client = new HttpClient(handler))
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                siteContentString = await client.GetStringAsync(urlPath);
                Console.WriteLine(urlPath);
            }

            return siteContentString;
        }

        internal async Task FetchMultipleXmlAndSaveToDisk(List<string> urlPath, List<string> localPath, List<string> localFilename, int serviceCount)
        {
            var credentials = new NetworkCredential("test", "testrest");
            var handler = new HttpClientHandler { Credentials = credentials };
            var client = new HttpClient(handler);
            List<HttpClient> httpClients = new List<HttpClient>();
            List<XmlIO> xmlIOs = new List<XmlIO>();
            List<Task> taskList = new List<Task>();
            int elementCount = urlPath.Count;
            for (int z = 0; z < serviceCount; z++)
            {
                httpClients.Add(new HttpClient(handler));
                xmlIOs.Add(new XmlIO());
            }

            for (int e = 0; e < elementCount; e++)
            {
                taskList.Add(xmlIOs[e % serviceCount].SaveXmlDocument(XDocument.Parse(httpClients[e % serviceCount].GetStringAsync(urlPath[e]).Result), localPath[e], localFilename[e]));
            }

            await Task.WhenAll(taskList.ToArray());

        }

    }
}
