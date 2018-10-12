using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Models;
using static Models.RestService;

namespace BusinessLogic
{
    public class DownloadMetadata
    {
        private Models.AppContext appContext;
        private WebResourceLoader webResourceLoader;

        public bool AlreadyExists(int id)
        {
            MetadataService profile = new ServerConn().GetServerConn(id);
            Models.AppContext appContext = new Models.AppContext(profile.Url, profile.Username, profile.Password, AppInfo.MetadataRootFolder);
            Logger.LogPath = appContext.RootLocalPath;
            XmlMetadata xmlMetadata = new XmlMetadata(appContext);
            WebResourceLoader webResourceLoader = new WebResourceLoader(appContext, xmlMetadata, 0);
            System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Run(() => xmlMetadata.InitServiceMetadata(webResourceLoader));
            
            XmlDocument doc = new XmlDocument();
            doc.Load(AppInfo.MetadataRootFolder + "Versions.xml");
            t.Wait();
            XmlNode node = doc.SelectSingleNode("//version[@name='" + appContext.Version + "']/release[@name='" + appContext.Release + "']");
            if (node == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DoTask(int metadataServiceId)
        {
            if (new ServerConn().GetServerConn(metadataServiceId) != null)
            {
                int processId = 1;
                if (AppInfo.Processes.Count > 0)
                {
                    foreach (Process processValue in AppInfo.Processes.Values)
                    {
                        if (processValue.MetadataServiceId == metadataServiceId && !processValue.Done)
                        {
                            return false;
                        }
                    }
                    processId = AppInfo.Processes.Last().Key + 1;
                }

                Process process = new Process(processId, metadataServiceId, DateTime.Now);
                AppInfo.Processes.Add(processId, process);

                System.Threading.Tasks.Task.Run(() => new DownloadMetadata().DoTheJob(processId));
                return true;
            }
            return false;
        }

        public List<Process> GetRESTMetadataList(int noOfProcesses)
        {
            List<Process> processList = new List<Process>();
            foreach (KeyValuePair<int, Process> pair in AppInfo.Processes.OrderByDescending(x => x.Key).Take(noOfProcesses))
            {
                processList.Add(pair.Value);
            }
            return processList;
        }

        #region Load service
        private void DoTheJob(int processId)
        {
            try
            {
                MetadataService profile = new ServerConn().GetServerConn(AppInfo.Processes[processId].MetadataServiceId);
                this.appContext = new Models.AppContext(profile.Url, profile.Username, profile.Password, AppInfo.MetadataRootFolder);

                // Set the initial log path in root until the version folder is not known
                Logger.LogPath = this.appContext.RootLocalPath;

                // ServiceLoadState serviceState = this.LoadRestServiceTestState();
                ServiceLoadState serviceState = this.LoadRestServiceLoadState(processId);

                //serviceState.Services.RemoveRange(10, serviceState.Services.Count - 10);
                //serviceState.CalcStatistics();
                int remainingServiceCount = 0;
                Logger.LogPath = this.appContext.ReleaseLocalPath;

                AppInfo.Processes[processId].Status.Total = serviceState.PendingLoadServices;

                while (serviceState.PendingLoadServices > 0 && remainingServiceCount != serviceState.PendingLoadServices)
                {
                    remainingServiceCount = serviceState.PendingLoadServices;
                    serviceState = this.LoadRestServiceMetadata(serviceState);
                    serviceState.Services = this.GetPendingServices(serviceState.Services);
                    serviceState.CalcStatistics();
                }

                this.webResourceLoader.xmlMetadata.AddReleaseToVersionXmlFile();
            }
            catch
            {
                Process process = AppInfo.Processes[processId];
                process.EndTime = DateTime.Now;
                process.Status.Text = "Stopped";
                process.Done = true;
            }
        }
        
        private ServiceLoadState LoadRestServiceLoadState(int processId)
        {
            ServiceLoadState loadState = null;
            List<Models.RestService> services = null;
            try
            {
                Logger.LogInfo("Getting REST service structure");

                XmlMetadata xmlMetadata = new XmlMetadata(this.appContext);
                this.webResourceLoader = new WebResourceLoader(this.appContext, xmlMetadata, processId);
                services = xmlMetadata.InitServiceMetadata(this.webResourceLoader);
                AppInfo.Processes[processId].Version = appContext.Version;
                AppInfo.Processes[processId].Release = appContext.Release;
                loadState = new ServiceLoadState();
                loadState.Services = services;
                
                ServiceLoadState savedState = this.webResourceLoader.GetServiceLoadState();

                if (savedState != null)
                {
                    Logger.LogInfo("Have found previous service load state");
                    this.LogState(savedState);
                    loadState = savedState;// this.AskForUsingLoadState(savedState);
                }

                loadState.CalcStatistics();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                Logger.LogError(ex.StackTrace);
            }

            return loadState;
        }
        
        private ServiceLoadState LoadRestServiceMetadata(ServiceLoadState loadState)
        {
            List<Models.RestService> services = loadState.Services;
            try
            {
                Logger.LogInfo(string.Format("Loading REST metadata for {0} services", loadState.PendingLoadServices));
                this.webResourceLoader.LoadServiceMetadata(services).Wait();

                //   Logger.LogInfo("Generating json tree data");
                //   JsonGenerator.generateJSONMetadata(this.rootLocalPath, this.metadataPath);

                this.LogState(loadState);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                Logger.LogError(ex.StackTrace);
            }

            return loadState;
        }

        private void LogState(ServiceLoadState loadState)
        {
            loadState.CalcStatistics();

            Logger.LogInfo("Loaded: " + loadState.Loaded);
            Logger.LogInfo("Loaded with errors: " + loadState.LoadedWithErrors);
            Logger.LogInfo("Failed: " + loadState.Failed);
            Logger.LogInfo("Waiting for load: " + loadState.NotLoaded);
        }

        private List<Models.RestService> GetPendingServices(List<Models.RestService> services)
        {
            List<Models.RestService> result = new List<Models.RestService>();

            foreach (Models.RestService service in services)
            {
                if (service.LoadStatus != ServiceLoadStatus.Loaded)
                {
                    result.Add(service);
                }
            }

            return result;
        }
        #endregion
    }
}