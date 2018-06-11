namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    internal class WebResourceLoader
    {
        internal async Task<WebResponse> GetResponseFromSite(string urlPath)
        {
            WebRequest request = WebRequest.Create(urlPath);
            request.Credentials = new NetworkCredential("test", "testrest");
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            WebResponse response = await request.GetResponseAsync();

            return response;
        }

        internal async Task FetchMultipleXmlAndSaveToDisk(List<string> urlPath, List<string> localPath, List<string> localFilename, int serviceCount)
        {
            var credentials = new NetworkCredential("test", "testrest");
            var handler = new HttpClientHandler { Credentials = credentials };
            var client = new HttpClient(handler);
            XDocument currentDocument = new XDocument();
            List<HttpClient> httpClients = new List<HttpClient>();
            List<XmlIO> xmlIOs = new List<XmlIO>();
            List<Task<string>> taskList = new List<Task<string>>();
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

                Task<DownloadData> stringXmlDocTask = this.GetHttpResonse(httpClients[currentService], urlPath[currentElement]);
                Task<XmlData> xmlDocTask = xmlIOs[currentService].LoadAsync(stringXmlDocTask);
                taskList.Add(xmlIOs[currentService].SaveAsync(xmlDocTask, localPath[currentElement], localFilename[currentElement]));
            }

            await Task.WhenAll(taskList.ToArray());
            for (int i = 0; i < taskList.Count; i++)
            {
                if (taskList[i].Result != string.Empty)
                {
                    Console.WriteLine(taskList[i].Result);
                }
            }
        }

        private async Task<DownloadData> GetHttpResonse(HttpClient client, string url, int tryCount = 0)
        {
            DownloadData responseData = new DownloadData();
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    responseData.ResponseString = response.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine("Error code {0} on try {1} whilst attempting to fetch {2}", response.StatusCode, tryCount + 1, url);
                    if (tryCount < 2 && (response == null || response.StatusCode != HttpStatusCode.BadRequest))
                    {
                        CancellationTokenSource source = new CancellationTokenSource();
                        source.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                        responseData = await this.GetHttpResonse(client, url, ++tryCount);
                    }
                    else if (tryCount >= 2)
                    {
                        XDocument errorMsg = XDocument.Parse(await response.Content.ReadAsStringAsync());
                        foreach (XElement element in errorMsg.Descendants("message"))
                        {
                            responseData.Error = element.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0} while attempting to fetch {1}", ex, url);
            }

            return responseData;
        }
    }
}
