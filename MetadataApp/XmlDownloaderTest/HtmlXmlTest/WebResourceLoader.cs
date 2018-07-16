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
    using System.Linq;

    internal class WebResourceLoader
    {
        private string rootUrl;

        public WebResourceLoader(string rootUrl)
        {
            this.rootUrl = rootUrl;
        }

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

                try {
                    Task<DownloadData> stringXmlDocTask = this.GetHttpResonse(httpClients[currentService], urlPath[currentElement]);
             //       Task<XmlData> xmlDocTask = xmlIOs[currentService].ParseXmlAsync(stringXmlDocTask);
               //     taskList.Add(xmlIOs[currentService].SaveXmlAsync(xmlDocTask, localPath[currentElement], localFilename[currentElement]));
                } catch (Exception ex) {

                }

                if (currentElement > 1 && currentElement % serviceCount == 0) {
                    await Task.WhenAll(taskList.ToArray());
                    Logger.LogProgress((int) (((float) currentElement) / ((float) 100) * 100) + "% Done");
                }
            }

            for (int i = 0; i < taskList.Count; i++)
            {
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

        internal async Task<List<RestService>> LoadServiceMetadata(List<RestService> services, string metadataPath)
        {
            List<XmlFile> xmlFileList = new List<XmlFile>();
            int currentService, serviceCount = 10;
            List<HttpClient> httpClients = this.GetHttpClients(serviceCount);
            List<XmlIO> xmlIOs = this.GetXmlIOs(serviceCount);
            List<Task<RestService>> taskList = new List<Task<RestService>>();

            HashSet<string> noSchemaServices = new HashSet<string>();
            noSchemaServices.Add("global");
            noSchemaServices.Add("user");
            noSchemaServices.Add("system");
            noSchemaServices.Add("umlmodel");
            noSchemaServices.Add("TdmDimObjSL");
            noSchemaServices.Add("TdmDimObjBL");

            int servicesCount = services.Count;

            for (int currentElement = 0; currentElement < servicesCount; currentElement++)
            {
                currentService = currentElement % serviceCount;
                RestService service = services[currentElement];

                if (!noSchemaServices.Contains(service.Name))
                {

                    taskList.Add(LoadServiceMetadata(httpClients[currentService], xmlIOs[currentService], service, xmlFileList));

                    //Log status
                    if (currentElement > 1 && currentElement % 10 == 0)
                    {
                        await Task.WhenAll(taskList.ToArray());
                        Logger.LogProgress((int)(((float)currentElement) / ((float)servicesCount) * 100) + "% Done");
                    }

                    //Store the state
                    if (currentElement > 1 && currentElement % 200 == 0)
                    {
                        await Task.WhenAll(taskList.ToArray());
                        taskList.Clear();
                    }
                }
            }

            await Task.WhenAll(taskList.ToArray());

            Logger.LogInfo("Update downloaded file metadata");
            WebResourceReader.AddFilesToXml(metadataPath, xmlFileList);

            return services;
        }

        internal List<FileLoadTask> GetAttachmentLoadTasks(RestService service, XmlData wadlXmlData)
        {
            List<string> attachmentNames = new List<string>();
            List<XElement> attachments = new List<XElement>();
            string xmlNameSpace = "{http://wadl.dev.java.net/2009/02}";
            string localPath, url;

            List<FileLoadTask> loadTasks = new List<FileLoadTask>();

            attachments.AddRange(
                from el in wadlXmlData.XDocument.Descendants()
                where el.Name.LocalName == "resource" && el.Attribute("path") != null && el.Attribute("path").Value == "attachments"
                select el);


            foreach (XElement attachment in attachments)
            {
                attachmentNames.AddRange(
                    from el in attachment.Descendants(xmlNameSpace + "resource")
                    where el.Attribute("path") != null && el.Attribute("path").Value != @"{pk}" && el.Attribute("path").Value != @"{filename}"
                    select el.Attribute("path").Value);
            }

            localPath = service.Filepath.Substring(0, service.Filepath.LastIndexOf("\\") + 1) + "attachments\\";
            XmlIO.CreateFolder(localPath);
            foreach (string fileName in attachmentNames)
            {
                url = this.rootUrl + string.Format("{0}/42069420/attachments/{1}", service.Name, fileName);
                FileLoadTask fileLoad = new FileLoadTask(url, localPath, fileName);
                loadTasks.Add(fileLoad);
            }

            return loadTasks;
        }

        internal async Task<RestService> LoadServiceMetadata(HttpClient client, XmlIO parser, RestService service, List<XmlFile> xmlFileList)
        {
            string filename;
            XmlData xmlData;
            List<FileLoadTask> loadTasks;

            try
            {
                filename = service.Name + ".wadl";
                xmlData = await this.LoadXmlFile(client, parser, this.rootUrl + service.Name + "/" + filename, service.Filepath, filename);
                xmlFileList.Add(new XmlFile(service.Name, filename, false, xmlData.Error));
                bool wadlLoaded = xmlData.Error == string.Empty;

                if (wadlLoaded) //then find and load attachments
                {
                    loadTasks = this.GetAttachmentLoadTasks(service, xmlData);
                    await this.LoadXmlFiles(client, parser, loadTasks);
                    foreach (FileLoadTask task in loadTasks)
                    {
                        xmlFileList.Add(new XmlFile(service.Name, filename, true, task.Error));
                    }
                }

                filename = service.Name + ".xsd";
                xmlData = await this.LoadXmlFile(client, parser, this.rootUrl + service.Name + "/" + filename, service.Filepath, filename);
                xmlFileList.Add(new XmlFile(service.Name, filename, false, xmlData.Error));
            }
            catch (Exception ex)
            {
                string msg = string.Format("Service {0} load failed. {1}", service.Name, ex.Message);
                Logger.LogError(msg);
                service.Error = msg;
            }

            return service;
        }

        private async Task<List<FileLoadTask>> LoadXmlFiles(HttpClient client, XmlIO parser, List<FileLoadTask> loadFiles)
        {

            foreach (FileLoadTask loadFile in loadFiles) {
                loadFile.FileXmlData = await this.LoadXmlFile(client, parser, loadFile.Url, loadFile.LocalFolder, loadFile.Filename);
                loadFile.Error = loadFile.FileXmlData.Error;
            }
 
            return loadFiles;
        }

        private async Task<XmlData> LoadXmlFile(HttpClient client, XmlIO parser, string url, string localFolder, string filename)
        {
            XmlData xmlDocTask;
            try
            {
                DownloadData stringXmlDocTask = await this.GetHttpResonse(client, url);
                xmlDocTask = parser.ParseXml(stringXmlDocTask);
                parser.SaveXml(xmlDocTask, localFolder, filename);
            }
            catch (Exception ex)
            {
                xmlDocTask = new XmlData();
                xmlDocTask.Error = ex.Message;
            }

            return xmlDocTask;
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
                        Logger.LogError(string.Format("Error code {0} on try {1} while attempting to fetch {2}", response.StatusCode, tryCount + 1, url));
                        await Task.Delay(1000);
                        responseData = await this.GetHttpResonse(client, url, ++tryCount);
                    }
                    else if (tryCount >= 2 || (response.StatusCode != HttpStatusCode.NotFound || response.StatusCode != HttpStatusCode.InternalServerError))
                    {

                        try
                        {
                            XDocument errorMsg = XDocument.Parse(await response.Content.ReadAsStringAsync());
                            foreach (XElement element in errorMsg.Descendants("message"))
                            {
                                responseData.Error += " " + element.Value;
                            }
                        }
                        catch (Exception ex)
                        {
                            responseData.Error = string.Format("Error code {0} while attempting to fetch {1}", response.StatusCode,  url);
                        }

                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                if (ex.CancellationToken.IsCancellationRequested)
                {
                    Logger.LogError(string.Format("Something canceled task while fetching {0}", url));
                }
                else
                {
                    Logger.LogError(string.Format("Timeout while fetching {0}", url));
                }

                await Task.Delay(1000);
                responseData = await this.GetHttpResonse(client, url, ++tryCount);
            }
            catch (Exception ex)
            {
                Logger.LogError(string.Format("Exception: {0} ont try {1} while attempting to fetch {2}", ex.Message, tryCount + 1, url));

                await Task.Delay(1000);
                responseData = await this.GetHttpResonse(client, url, ++tryCount);
            }

            if (responseData.Error != string.Empty)
            {
                throw new Exception(responseData.Error);
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
