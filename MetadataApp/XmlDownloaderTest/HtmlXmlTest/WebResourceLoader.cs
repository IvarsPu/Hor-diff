namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;


    internal class WebResourceLoader
    {
        internal async Task<Stream> FetchSiteAsString(string urlPath)
        {
            Stream siteContentString;
            var credentials = new NetworkCredential("test", "testrest");
            var handler = new HttpClientHandler { Credentials = credentials };
            using (var client = new HttpClient(handler))
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                siteContentString = await client.GetStreamAsync(urlPath);
                Console.WriteLine(urlPath);
            }

            return siteContentString;
        }

        internal async Task FetchMultipleXmlAndSaveToDisk(List<string> urlPath, List<string> localPath, List<string> localFilename, int serviceCount)
        {
            var credentials = new NetworkCredential("test", "testrest");
            var handler = new HttpClientHandler { Credentials = credentials };
            var client = new HttpClient(handler);
            XDocument currentDocument = new XDocument();
            List<HttpClient> httpClients = new List<HttpClient>();
            List<XmlIO> xmlIOs = new List<XmlIO>();
            List<Task> taskList = new List<Task>();
            int totalElementCount = urlPath.Count;
            int currentService;
            for (int z = 0; z < serviceCount; z++)
            {
                httpClients.Add(new HttpClient(handler));
                xmlIOs.Add(new XmlIO());
            }

            for (int currentElement = 0; currentElement < totalElementCount; currentElement++)
            {
                currentService = currentElement % serviceCount;
                taskList.Add(xmlIOs[currentService].SaveAsync(xmlIOs[currentService].LoadAsync(this.GetHttpResonse(httpClients[currentService], urlPath[currentElement])), localPath[currentElement], localFilename[currentElement]));
            }

            await Task.WhenAll(taskList.ToArray());

        }

        private async Task<string> GetHttpResonse(HttpClient client, string url)
        {
            string responseString = null;
            try
            {

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    responseString = await response.Content.ReadAsStringAsync();
                } else
                {
                    Console.WriteLine("Error code {0} while attempting to fetch {1}", response.StatusCode, url);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception {0} while attempting to fetch {1}", e, url);
            }

            return responseString;
        }

    }
}
