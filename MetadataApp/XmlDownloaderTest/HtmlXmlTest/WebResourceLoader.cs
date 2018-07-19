namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using static HtmlXmlTest.RestService;

    internal class WebResourceLoader
    {
        private string rootUrl;
        private string rootLocalPath;

        public WebResourceLoader(string rootUrl, string rootLocalPath)
        {
            this.rootUrl = rootUrl;
            this.rootLocalPath = rootLocalPath;
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

        internal async Task<List<RestService>> LoadServiceMetadata(List<RestService> services, string metadataPath)
        {
            List<XmlFile> xmlFileList = new List<XmlFile>();
            int currentService, serviceCount = 10;
            List<HttpClient> httpClients = this.GetHttpClients(serviceCount);
            List<XmlIO> xmlIOs = this.GetXmlIOs(serviceCount);
            List<Task<RestService>> taskList = new List<Task<RestService>>();

            HashSet<string> noSchemaServices = new HashSet<string>
            {
                "global",
                "user",
                "system",
                "umlmodel",
                "TdmDimObjSL",
                "TdmDimObjBL"
            };

            int servicesCount = services.Count;

            for (int currentElement = 0; currentElement < servicesCount; currentElement++)
            {
                currentService = currentElement % serviceCount;
                RestService service = services[currentElement];

                if (!noSchemaServices.Contains(service.Name))
                {
                    taskList.Add(this.LoadServiceMetadata(httpClients[currentService], xmlIOs[currentService], service, xmlFileList));

                    // Log status
                    if (currentElement > 1 && currentElement % 10 == 0)
                    {
                        await Task.WhenAll(taskList.ToArray());
                        Logger.LogProgress(currentElement + " services loaded");
                    }

                    // Store the state
                    if (currentElement > 1 && currentElement % 200 == 0)
                    {
                        await Task.WhenAll(taskList.ToArray());

                        // Update downloaded file metadata
                        WebResourceReader.AddFilesToXml(metadataPath, xmlFileList);

                        // Store load statuss and clean up
                        this.StoreLoadState(services);
                        taskList.Clear();
                    }
                }
                else
                {
                    service.LoadStatus = ServiceLoadStatus.Loaded;
                }
            }

            await Task.WhenAll(taskList.ToArray());
            WebResourceReader.AddFilesToXml(metadataPath, xmlFileList);
            this.StoreLoadState(services);

            return services;
        }

        internal void StoreLoadState(List<RestService> services)
        {
            ServiceLoadState state = new ServiceLoadState();
            state.Services = services;
            state.CalcStatistics();

            string filePath = this.GetLoadStateFilePath();
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(
                                        filePath,
                                        FileMode.OpenOrCreate,
                                        FileAccess.Write,
                                        FileShare.None);
            formatter.Serialize(stream, state);
            stream.Close();
        }

        internal ServiceLoadState GetServiceLoadState()
        {
            ServiceLoadState loadState = null;
            string filePath = this.GetLoadStateFilePath();

            if (File.Exists(filePath))
            {
                try
                {
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream(
                                                filePath,
                                                FileMode.Open,
                                                FileAccess.Read,
                                                FileShare.Read);
                    loadState = (ServiceLoadState)formatter.Deserialize(stream);
                    stream.Close();
                    loadState.CalcStatistics();
                }
                catch (Exception ex)
                {
                    Logger.LogError(string.Format("Havent found load state: {0}", ex.Message));
                }
            }

            return loadState;
        }

        internal string GetLoadStateFilePath()
        {
            return this.rootLocalPath + "LoadState.bin";
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
                fileLoad.Attachment = true;
                loadTasks.Add(fileLoad);
            }

            return loadTasks;
        }

        internal List<FileLoadTask> GetQueryLoadTask(RestService service, XmlData wadlXmlData)
        {
            List<XElement> queries = new List<XElement>();
            List<FileLoadTask> loadTasks = new List<FileLoadTask>();

            queries.AddRange(
                from el in wadlXmlData.XDocument.Descendants()
                where el.Name.LocalName == "resource" && el.Attribute("path") != null && el.Attribute("path").Value == "query"
                select el);

            if (queries.Count > 0)
            {
                string url = this.rootUrl + string.Format("{0}/query", service.Name);
                FileLoadTask fileLoad = new FileLoadTask(url, service.Filepath, "query.xml");
                loadTasks.Add(fileLoad);
            }

            return loadTasks;
        }

        internal async Task<RestService> LoadServiceMetadata(HttpClient client, XmlIO parser, RestService service, List<XmlFile> xmlFileList)
        {
            string filename;
            XmlData xmlData;
            List<FileLoadTask> loadTasks;
            List<XmlFile> serviceXmlFiles = new List<XmlFile>();

            service.LoadStatus = ServiceLoadStatus.NotLoaded;

            try
            {
                filename = service.Name + ".wadl";
                xmlData = await this.LoadXmlFile(client, parser, this.rootUrl + service.Name + "/" + filename, service.Filepath, filename);
                serviceXmlFiles.Add(new XmlFile(service.Name, service.Filepath, filename, false, xmlData.Error));
                bool wadlLoaded = xmlData.Error == string.Empty;

                if (wadlLoaded)
                {
                    loadTasks = this.GetAttachmentLoadTasks(service, xmlData);
                    loadTasks.AddRange(this.GetQueryLoadTask(service, xmlData));
                    await this.LoadXmlFiles(client, parser, loadTasks);
                    foreach (FileLoadTask task in loadTasks)
                    {
                        serviceXmlFiles.Add(new XmlFile(service.Name, task.LocalFolder, task.Filename, task.Attachment, task.Error));
                    }
                }

                filename = service.Name + ".xsd";
                xmlData = await this.LoadXmlFile(client, parser, this.rootUrl + service.Name + "/" + filename, service.Filepath, filename);
                serviceXmlFiles.Add(new XmlFile(service.Name, service.Filepath, filename, false, xmlData.Error));

                foreach (XmlFile xmlFile in serviceXmlFiles)
                {
                    if (xmlFile.ErrorMSG != string.Empty)
                    {
                        service.LoadStatus = ServiceLoadStatus.LoadedWithErrors;
                        break;
                    }
                }

                if (service.LoadStatus == ServiceLoadStatus.NotLoaded)
                {
                    service.LoadStatus = ServiceLoadStatus.Loaded;
                }

                lock (xmlFileList)
                {
                    xmlFileList.AddRange(serviceXmlFiles);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Service {0} load failed. {1}", service.Name, ex.Message);
                Logger.LogError(msg);
                service.LoadStatus = ServiceLoadStatus.Failed;
            }

            return service;
        }

        private async Task<List<FileLoadTask>> LoadXmlFiles(HttpClient client, XmlIO parser, List<FileLoadTask> loadFiles)
        {
            foreach (FileLoadTask loadFile in loadFiles)
            {
                loadFile.FileXmlData = await this.LoadXmlFile(client, parser, loadFile.Url, loadFile.LocalFolder + "\\", loadFile.Filename);
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
                        if (tryCount == 0)
                        {
                            Logger.LogError(string.Format("Error code {0} on try {1} while attempting to fetch {2}", response.StatusCode, tryCount + 1, url));
                        }

                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            // Don't call again
                            responseData.Error = string.Format("Error code {0} while attempting to fetch {1}", response.StatusCode, url);
                        }
                        else
                        {
                            await Task.Delay(1000);
                            responseData = await this.GetHttpResonse(client, url, ++tryCount);
                        }
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
                            responseData.Error = string.Format("Error {0} while attempting to fetch {1}", ex.Message,  url);
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
                if (tryCount == 0)
                {
                    Logger.LogError(string.Format("Exception: {0} ont try {1} while attempting to fetch {2}", ex.Message, tryCount + 1, url));
                }

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
