using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Models;
using static Models.RestService;

namespace BusinessLogic
{
    public class Program
    {
        private Models.AppContext appContext;
        private WebResourceLoader webResourceLoader;

        public void DoTheJob(int processId)
        {
            try
            {
                MetadataService profile = GetMetadataService(AppInfo.Processes[processId].MetadataServiceId);
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

        //private ServiceLoadState AskForUsingLoadState(ServiceLoadState loadState)
        //{
        //    ServiceLoadState result = loadState;
        //    Console.WriteLine("Press y to continue load or any other key to start new load:");
        //    ConsoleKeyInfo keyInfo = Console.ReadKey();

        //    if (keyInfo.KeyChar != 'y')
        //    {
        //        result = null;
        //    }

        //    return result;
        //}
        
        private ServiceLoadState LoadRestServiceMetadata(ServiceLoadState loadState)
        {
            List<RestService> services = loadState.Services;
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

        private static MetadataService GetMetadataService(int id)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppInfo.path);
            XmlNode node = doc.SelectSingleNode("//MetadataServices/MetadataService[@ID='" + id + "']");
            if (node != null)
            {

                MetadataService metadataService = new MetadataService();
                metadataService.Id = Int32.Parse(node.Attributes["ID"].Value);
                metadataService.Name = node.Attributes["Name"].Value;
                metadataService.Url = node.Attributes["Url"].Value;
                metadataService.Username = node.Attributes["Username"].Value;
                metadataService.Password = node.Attributes["Password"].Value;
                return metadataService;
            }
            else
            {
                return null;
            }
        }
    }
}