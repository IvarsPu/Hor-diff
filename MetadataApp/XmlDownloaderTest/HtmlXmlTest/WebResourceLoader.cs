namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    internal class WebResourceLoader
    {
        internal static List<string> GetAttachmentUrls(List<string> attachmentNames, List<string> attachmentPaths, string baseUrl)
        {
            List<string> attachmentUrls = new List<string>();
            for (int i = 0; i < attachmentNames.Count; i++)
            {
                string currentPath = attachmentPaths[i];
                string folderName = currentPath.Substring(currentPath.LastIndexOf("\\") + 1, currentPath.Length - currentPath.LastIndexOf("\\") - 6);
                attachmentUrls.Add(baseUrl + string.Format("{0}/42069420/attachments/{1}", folderName, attachmentNames[i]));
            }

            return attachmentUrls;
        }




        internal async Task<WebResponse> GetResponseFromSite(string urlPath)
        {
            WebRequest request = WebRequest.Create(urlPath);
            string login = ConfigurationManager.ConnectionStrings["Login"].ConnectionString;
            string password = ConfigurationManager.ConnectionStrings["Password"].ConnectionString;
            request.Credentials = new NetworkCredential(login, password);
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            WebResponse response = await request.GetResponseAsync();

            return response;
        }

        internal async Task FetchMultipleXmlAndSaveToDisk(List<string> urlPath, List<string> localPath, List<string> localFilename, int serviceCount)
        {
            string login = ConfigurationManager.ConnectionStrings["Login"].ConnectionString;
            string password = ConfigurationManager.ConnectionStrings["Password"].ConnectionString;
            var credentials = new NetworkCredential(login, password);
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
                httpClients[z].Timeout = TimeSpan.FromHours(24);
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
                    if (tryCount < 2 && (response == null || response.StatusCode != HttpStatusCode.NotFound || response.StatusCode != HttpStatusCode.InternalServerError))
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
            catch (TaskCanceledException ex)
            {
                if (ex.CancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Something canceled task whilst fetching {0}", url);
                }
                else
                {
                    Console.WriteLine("Timeout whilst fetching {0}", url);
                }

                CancellationTokenSource source = new CancellationTokenSource();
                source.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                responseData = await this.GetHttpResonse(client, url, ++tryCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0} while attempting to fetch {1}", ex.Message, url);
                CancellationTokenSource source = new CancellationTokenSource();
                source.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                responseData = await this.GetHttpResonse(client, url, ++tryCount);
            }

            return responseData;
        }
    }
}
