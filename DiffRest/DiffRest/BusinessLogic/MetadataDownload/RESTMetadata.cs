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
        private Logger Logger;

        public bool AlreadyExists(int id)
        {
            RestConnection profile = new Connection().GetServerConn(id);
            Models.AppContext appContext = new Models.AppContext(profile, AppInfo.MetadataRootFolder, new Logger(AppInfo.MetadataRootFolder));
            XmlMetadata xmlMetadata = new XmlMetadata(appContext);
            WebResourceLoader webResourceLoader = new WebResourceLoader(appContext, xmlMetadata, 0);
            System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Run(() => xmlMetadata.InitServiceMetadata(webResourceLoader));
            
            XmlDocument doc = new XmlDocument();
            doc.Load(AppInfo.MetadataRootFolder + "Versions.xml");
            t.Wait();
            XmlNode node = doc.SelectSingleNode("//version[@name='" + appContext.Version + "']/release[@name='" + appContext.Release + "']");
            return node != null;
        }

        public bool DoTask(int metadataServiceId)
        {
            if (new Connection().GetServerConn(metadataServiceId) != null)
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
                RestConnection profile = new Connection().GetServerConn(AppInfo.Processes[processId].MetadataServiceId);
                this.Logger = new Logger(AppInfo.MetadataRootFolder);
                this.appContext = new Models.AppContext(profile, AppInfo.MetadataRootFolder, Logger);


                // ServiceLoadState serviceState = this.LoadRestServiceTestState();
                ServiceLoadState serviceState = this.LoadRestServiceLoadState(processId);

                int totalServiceCount = serviceState.Services.Count;
                //serviceState.Services.RemoveRange(10, serviceState.Services.Count - 10);
                //serviceState.CalcStatistics();
                int remainingServiceCount = 0;
                ((Logger)this.appContext.Logger).LogPath = this.appContext.ReleaseLocalPath;

                AppInfo.Processes[processId].Status.Total = serviceState.PendingLoadServices;
                int loadCount = 0;
                do
                {
                    remainingServiceCount = serviceState.PendingLoadServices;
                    serviceState = this.LoadRestServiceMetadata(serviceState, totalServiceCount);
                    this.webResourceLoader.xmlMetadata.AddReleaseToVersionXmlFile(serviceState);

                    serviceState.CalcStatistics();
                    loadCount++; 
                } while (serviceState.PendingLoadServices > 0 && loadCount < 2);

                Process process = AppInfo.Processes[processId];
                process.Progress = Convert.ToInt32((process.Status.Loaded + serviceState.LoadedWithErrors) * 100 / totalServiceCount);
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
            List<RestService> services = null;
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
        
        private ServiceLoadState LoadRestServiceMetadata(ServiceLoadState loadState, int totalServiceCount)
        {
            List<RestService> services = loadState.Services;

            Logger.LogInfo(string.Format("Loading REST metadata for {0} services", loadState.PendingLoadServices));
            this.webResourceLoader.LoadServiceMetadata(services, totalServiceCount).Wait();

            this.LogState(loadState);

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

        private List<RestService> GetPendingServices(List<RestService> services)
        {
            List<RestService> result = new List<RestService>();

            foreach (RestService service in services)
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