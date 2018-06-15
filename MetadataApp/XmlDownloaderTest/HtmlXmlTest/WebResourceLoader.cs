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

        internal async Task<List<XmlFile>> FetchMultipleXmlAndSaveToDisk(List<string> urlPath, List<string> localPath, List<string> localFilename, int serviceCount)
        {
            XDocument currentDocument = new XDocument();
            List<HttpClient> httpClients = this.GetHttpClients(serviceCount);
            List<XmlIO> xmlIOs = this.GetXmlIOs(serviceCount);
            List<Task<XmlData>> taskList = new List<Task<XmlData>>();
            List<XmlFile> allXmlData = new List<XmlFile>();
            int totalElementCount = urlPath.Count;
            int currentService;

            for (int currentElement = 0; currentElement < totalElementCount && currentElement < 100; currentElement++)// && currentElement < x for testing purposes
            {
                currentService = currentElement % serviceCount;

                Task<DownloadData> stringXmlDocTask = this.GetHttpResonse(httpClients[currentService], urlPath[currentElement]);
                Task<XmlData> xmlDocTask = xmlIOs[currentService].LoadAsync(stringXmlDocTask);
                taskList.Add(xmlIOs[currentService].SaveAsync(xmlDocTask, localPath[currentElement], localFilename[currentElement]));
                if (currentElement > 1 && currentElement % 200 == 0)
                {
                    await Task.WhenAll(taskList.ToArray());
                    Console.WriteLine(currentElement);
                }
            }

            for (int i = 0; i < taskList.Count; i++)
            {
                Console.WriteLine(taskList[i].Result.Error);

                string folderName = urlPath[i];
                Console.WriteLine(folderName);
                int index = folderName.IndexOf("/42069420/attachments/");
                if (index == -1)
                {
                    index = folderName.LastIndexOf("/");
                }

                folderName = folderName.Remove(index);
                folderName = folderName.Substring(folderName.LastIndexOf("/") + 1);

                Console.WriteLine(folderName);
                allXmlData.Add(new XmlFile(folderName, localFilename[i], false, taskList[i].Result.Error));
            }

            return allXmlData;
        }

        private async Task<DownloadData> GetHttpResonse(HttpClient client, string url, int tryCount = 0)
        {
            DownloadData responseData = new DownloadData();
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    responseData.ResponseTask = response.Content.ReadAsStringAsync();
                }
                else
                {
                    if (tryCount < 2 && (response == null || (response.StatusCode != HttpStatusCode.NotFound && response.StatusCode != HttpStatusCode.InternalServerError)))
                    {
                        Logger.Log(string.Format("Error code {0} on try {1} whilst attempting to fetch {2}", response.StatusCode, tryCount + 1, url));
                        await Task.Delay(1000);
                        responseData = await this.GetHttpResonse(client, url, ++tryCount);
                    }
                    else if (tryCount >= 2 || (response.StatusCode != HttpStatusCode.NotFound || response.StatusCode != HttpStatusCode.InternalServerError))
                    {
                        Logger.Log(string.Format("Error code {0} on try {1} whilst attempting to fetch {2}", response.StatusCode, tryCount + 1, url));
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
                    Logger.Log(string.Format("Something canceled task whilst fetching {0}", url));
                }
                else
                {
                    Logger.Log(string.Format("Timeout whilst fetching {0}", url));
                }

                await Task.Delay(1000);
                responseData = await this.GetHttpResonse(client, url, ++tryCount);
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("Exception: {0} ont try {1} while attempting to fetch {2}", ex.Message, tryCount + 1, url));

                await Task.Delay(1000);
                responseData = await this.GetHttpResonse(client, url, ++tryCount);
            }

            return responseData;
        }

        private List<HttpClient> GetHttpClients(int count)
        {
            List<HttpClient> httpClients = new List<HttpClient>();
            string login = ConfigurationManager.ConnectionStrings["Login"].ConnectionString;
            string password = ConfigurationManager.ConnectionStrings["Password"].ConnectionString;
            var credentials = new NetworkCredential(login, password);
            var handler = new HttpClientHandler { Credentials = credentials };
            for (int i = 0; i < count; i++)
            {
                httpClients.Add(new HttpClient(handler));
                httpClients[i].Timeout = TimeSpan.FromHours(24);
            }

            return httpClients;
        }

        private List<XmlIO> GetXmlIOs(int count)
        {
            List<XmlIO> xmlIOs = new List<XmlIO>();
            for (int i = 0; i < count; i++)
            {
                xmlIOs.Add(new XmlIO());
            }

            return xmlIOs;
        }
    }
}
