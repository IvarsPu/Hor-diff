﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Xml.Linq;
using Models;
using static Models.RestService;

namespace BusinessLogic
{
    internal class WebResourceLoader
    {
        private Models.AppContext appContext;
        private int processId;
        private Logger Logger;

        public XmlMetadata xmlMetadata { get; set; }

        public WebResourceLoader(Models.AppContext appContext, XmlMetadata xmlMetadata, int processId)
        {
            this.appContext = appContext;
            this.xmlMetadata = xmlMetadata;
            this.processId = processId;
            this.Logger = (Logger) this.appContext.Logger;
        }

        internal async Task<WebResponse> GetResponseFromSite(string urlPath)
        {
            WebRequest request = WebRequest.Create(urlPath);
            request.Credentials = new NetworkCredential(appContext.Username, appContext.Password);
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            WebResponse response = await request.GetResponseAsync();
             
            return response;
        }

        internal async Task<List<RestService>> LoadServiceMetadata(List<RestService> services, int totalServiceCount)
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

            Process process = AppInfo.Processes[processId];
            try
            {
                process.Status.Text = "Running";
                int servicesCount = services.Count;
                int currentRestService = -1;

                foreach (RestService service in services)
                {
                    process.Token.ThrowIfCancellationRequested();

                    if (!noSchemaServices.Contains(service.Name) && service.LoadStatus != ServiceLoadStatus.Loaded)
                    {
                        currentRestService++;
                        currentService = currentRestService % serviceCount;

                        taskList.Add(this.LoadServiceMetadata(httpClients[currentService], xmlIOs[currentService], service, xmlFileList));

                        // Log status
                        if (currentRestService > 1 && currentRestService % appContext.ParallelThreads == 0)
                        {
                            await Task.WhenAll(taskList.ToArray());
                        }

                        // Store the state
                        if (currentRestService > 1 && currentRestService % 200 == 0)
                        {
                            await Task.WhenAll(taskList.ToArray());

                            // Update downloaded file metadata
                            xmlMetadata.AddFilesToXml(xmlFileList);

                            // Store load statuss and clean up
                            this.StoreLoadState(services);
                            taskList.Clear();
                        }
                    }
                    else
                    {
                        service.LoadStatus = ServiceLoadStatus.Loaded;
                       // process.Status.Loaded++;
                    }
                    process.Progress = Convert.ToInt32(process.Status.Loaded * 100 / totalServiceCount);
                }

                await Task.WhenAll(taskList.ToArray());
                xmlMetadata.AddFilesToXml(xmlFileList);
                this.StoreLoadState(services);

                process.EndTime = DateTime.Now;
                process.Status.Text = "Finished";
                process.Done = true;
            }
            catch (Exception ex)
            {
                Logger.LogError("LoadServiceMetadata failed with error: " + ex.Message);
                Logger.LogError(ex.StackTrace);
                process.Token.ThrowIfCancellationRequested();
            }

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
                    Logger.LogError(string.Format("Have not found load state: {0}", ex.Message));
                }
            }

            return loadState;
        }

        internal string GetLoadStateFilePath()
        {
            return this.appContext.ReleaseLocalPath + "LoadState.bin";
        }

        internal List<XmlFile> GetAttachmentLoadTasks(RestService service, XmlFile wadlXmlFile)
        {
            List<XmlFile> loadTasks = new List<XmlFile>();

            List<string> attachmentNames = new List<string>();
            List<XElement> attachments = new List<XElement>();
            string xmlNameSpace = "{http://wadl.dev.java.net/2009/02}";
            string localPath, url;

            attachments.AddRange(
                from el in wadlXmlFile.XDocument.Descendants()
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
                url = this.appContext.RootUrl + string.Format("{0}/42069420/attachments/{1}", service.Name, fileName);
                XmlFile fileLoad = new XmlFile(service.Name, url, localPath, fileName, true);
                loadTasks.Add(fileLoad);
            }

            return loadTasks;
        }

        internal List<XmlFile> GetQueryLoadTask(RestService service, XmlFile wadlXmFile)
        {
            List<XElement> queries = new List<XElement>();
            List<XmlFile> loadTasks = new List<XmlFile>();

            if(!appContext.LoadQuery)
            {
                return loadTasks;
            }

            queries.AddRange(
                from el in wadlXmFile.XDocument.Descendants()
                where el.Name.LocalName == "resource" && el.Attribute("path") != null && el.Attribute("path").Value == "query"
                select el);

            if (queries.Count > 0)
            {
                string url = this.appContext.RootUrl + string.Format("{0}/query", service.Name);
                XmlFile fileLoad = new XmlFile(service.Name, url, service.Filepath, "query.xml");
                loadTasks.Add(fileLoad);
            }

            return loadTasks;
        }

        internal List<XmlFile> GetTemplateLoadTask(RestService service, XmlFile catalogXmFile)
        {
            List<XElement> queries = new List<XElement>();
            List<XmlFile> loadTasks = new List<XmlFile>();
            List<string> pkTemplates = new List<string>();


            if (!String.IsNullOrEmpty(catalogXmFile.Error))
            {
                return loadTasks;
            }

            IEnumerable<XElement> templateElems = catalogXmFile.XDocument.Descendants("templates");

            if(templateElems.Count() > 0)
            {
                string templateUrl = templateElems.First().Descendants("href").First().Value.Replace("/rest/", "");

                string url = this.appContext.RootUrl + templateUrl;
                XmlFile fileLoad = new XmlFile(service.Name, url, service.Filepath, "template.xml");
                loadTasks.Add(fileLoad);
            }           
                       
            return loadTasks;
        }

        internal async Task<RestService> LoadServiceMetadata(HttpClient client, XmlIO parser, RestService service, List<XmlFile> xmlFileList)
        {
            string filename;
            List<XmlFile> serviceXmlFiles = new List<XmlFile>();

            service.LoadStatus = ServiceLoadStatus.NotLoaded;

            Status status = AppInfo.Processes[processId].Status;

            try
            {
                filename = service.Name + ".wadl";
                XmlFile xmlFile = await this.LoadXmlFile(client, parser, new XmlFile(service.Name, this.appContext.RootUrl + service.Name + "/" + filename, service.Filepath, filename));
                serviceXmlFiles.Add(xmlFile);
                bool wadlLoaded = xmlFile.Error == string.Empty;

                if (wadlLoaded)
                {
                    List<XmlFile> loadTasks = this.GetAttachmentLoadTasks(service, xmlFile);
                    loadTasks.AddRange(this.GetQueryLoadTask(service, xmlFile));

                    if (appContext.LoadTemplate)
                    {
                        XmlFile serviceCatalog = await this.LoadXmlFileWithoutSave(client, parser, new XmlFile(service.Name, this.appContext.RootUrl + service.Name, service.Filepath, filename));
                        loadTasks.AddRange(this.GetTemplateLoadTask(service, serviceCatalog));
                    }                    
                    await this.LoadXmlFiles(client, parser, loadTasks);

                    serviceXmlFiles.AddRange(loadTasks);
                }

                filename = service.Name + ".xsd";
                xmlFile = await this.LoadXmlFile(client, parser, new XmlFile(service.Name, this.appContext.RootUrl + service.Name + "/" + filename, service.Filepath, filename));
                serviceXmlFiles.Add(xmlFile);

                bool loadedWithErrors = false;
                foreach (XmlFile file in serviceXmlFiles)
                {
                    if (file.Error != string.Empty)
                    {
                        service.LoadStatus = ServiceLoadStatus.LoadedWithErrors;
                        loadedWithErrors = true;
                        break;
                    }
                }

                if (service.LoadStatus == ServiceLoadStatus.NotLoaded)
                {

                    if (!loadedWithErrors)
                    {
                        status.Loaded++; // Will retry
                        service.LoadStatus = ServiceLoadStatus.Loaded;
                    }
                    else
                    {
                        service.LoadStatus = ServiceLoadStatus.LoadedWithErrors;
                    }
                    
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
                status.Failed++;
            }

            return service;
        }

        private async Task<List<XmlFile>> LoadXmlFiles(HttpClient client, XmlIO parser, List<XmlFile> loadFiles)
        {
            foreach (XmlFile loadFile in loadFiles)
            {
                await this.LoadXmlFile(client, parser, loadFile);
            }

            return loadFiles;
        }

        private async Task<XmlFile> LoadXmlFile(HttpClient client, XmlIO parser, XmlFile xmlFile)
        {

            if (!xmlFile.Exists)
            {
                try
                {
                    await this.GetHttpResonse(client, xmlFile);
                    parser.ParseXml(xmlFile);
                    parser.SaveXml(xmlFile);
                }
                catch (Exception ex)
                {
                    xmlFile.Error = ex.Message;
                }
            }
            else
            {
                xmlFile.LoadLocalFile();
            }

            return xmlFile;
        }

        private async Task<XmlFile> LoadXmlFileWithoutSave(HttpClient client, XmlIO parser, XmlFile xmlFile)
        {


            try
            {
                await this.GetHttpResonse(client, xmlFile);
                parser.ParseXml(xmlFile);
            }
            catch (Exception ex)
            {
                xmlFile.Error = ex.Message;
            }

            return xmlFile;
        }

        private async Task<XmlFile> GetHttpResonse(HttpClient client, XmlFile xmlFile, int tryCount = 0)
        {
            try
            {
                var response = await client.GetAsync(xmlFile.Url);
                if (response != null)
                {
                    xmlFile.HttpResultCode = (int)response.StatusCode;
                }

                if (response.IsSuccessStatusCode)
                {
                    xmlFile.HttpResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    if (tryCount < 2)
                    {
                        if (tryCount == 0)
                        {
                            Logger.LogError(string.Format("HTTP error code {0} while attempting to fetch {1}", xmlFile.HttpResultCode, xmlFile.Url));
                        }

                        if (response.StatusCode != HttpStatusCode.BadRequest)
                        {
                            await Task.Delay(1000);
                            await this.GetHttpResonse(client, xmlFile, ++tryCount);
                        }
                    }
                    else if (tryCount >= 2 && response != null)
                    {
                        try
                        {
                            XDocument errorMsg = XDocument.Parse(await response.Content.ReadAsStringAsync());
                            foreach (XElement element in errorMsg.Descendants("message"))
                            {
                                if (xmlFile.Error.Trim() != element.Value)
                                {
                                    xmlFile.Error += " " + element.Value;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            xmlFile.Error = string.Format("Error {0} while attempting to fetch {1}", ex.Message, xmlFile.Url);
                            Logger.LogError(xmlFile.Error);
                        }
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                if (ex.CancellationToken.IsCancellationRequested)
                {
                    Logger.LogError(string.Format("Something canceled task while fetching {0}", xmlFile.Url));
                }
                else
                {
                    Logger.LogError(string.Format("Timeout while fetching {0}", xmlFile.Url));
                }

          //      await Task.Delay(1000);
         //       await this.GetHttpResonse(client, xmlFile, ++tryCount);
            }
            catch (Exception ex)
            {
                if (tryCount == 0)
                {
                    Logger.LogError(string.Format("Exception: {0} ont try {1} while attempting to fetch {2}", ex.Message, tryCount + 1, xmlFile.Url));
                    await Task.Delay(1000);
                    await this.GetHttpResonse(client, xmlFile, ++tryCount);
                }
            }

            if (xmlFile.Error != string.Empty)
            {
                throw new Exception(xmlFile.Error);
            }

            return xmlFile;
        }

        private List<HttpClient> GetHttpClients(int count)
        {
            List<HttpClient> httpClients = new List<HttpClient>();
            var credentials = new NetworkCredential(appContext.Username, appContext.Password);
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
