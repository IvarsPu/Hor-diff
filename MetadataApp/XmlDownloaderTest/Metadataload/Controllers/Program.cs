using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Metadataload.Models;
using static Metadataload.Models.RestService;

namespace Metadataload.Controllers
{
    internal class Program
    {
        private Models.AppContext appContext;
        private WebResourceLoader webResourceLoader;
        private int processId;

        public void DoTheJob(int processId)
        {
            this.processId = processId;
            string rootUrl = WebConfigurationManager.ConnectionStrings["Server"].ConnectionString;
            string rootLocalPath = WebConfigurationManager.ConnectionStrings["MetadataLocalFolder"].ConnectionString;
            
            this.appContext = new Models.AppContext(rootUrl, rootLocalPath);

            // Set the initial log path in root until the version folder is not known
            Logger.LogPath = this.appContext.RootLocalPath;
            
            // ServiceLoadState serviceState = this.LoadRestServiceTestState();
            ServiceLoadState serviceState = this.LoadRestServiceLoadState();
            
            //serviceState.Services.RemoveRange(10, serviceState.Services.Count - 10);
            //serviceState.CalcStatistics();
            int remainingServiceCount = 0;
            Logger.LogPath = this.appContext.ReleaseLocalPath;

            while (serviceState.PendingLoadServices > 0 && remainingServiceCount != serviceState.PendingLoadServices)
            {
                remainingServiceCount = serviceState.PendingLoadServices;
                serviceState = this.LoadRestServiceMetadata(serviceState);
                serviceState.Services = this.GetPendingServices(serviceState.Services);
                serviceState.CalcStatistics();
            }

            //throws exception
            //this.webResourceLoader.xmlMetadata.AddReleaseToVersionXmlFile();
        }

        public ServiceLoadState LoadRestServiceLoadState()
        {
            ServiceLoadState loadState = null;
            List<RestService> services = null;
            try
            {
                Logger.LogInfo("Getting REST service structure");

                XmlMetadata xmlMetadata = new XmlMetadata(this.appContext);
                this.webResourceLoader = new WebResourceLoader(this.appContext, xmlMetadata, processId);
                services = xmlMetadata.InitServiceMetadata(this.webResourceLoader);
                loadState = new ServiceLoadState();
                loadState.Services = services;
                
                ServiceLoadState savedState = this.webResourceLoader.GetServiceLoadState();

                if (savedState != null)
                {
                    Logger.LogInfo("Have found previous service load state");
                    this.LogState(savedState);
                    //savedState = this.AskForUsingLoadState(savedState);
                }

                if (savedState != null)
                {
                    loadState = savedState;
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

        public ServiceLoadState LoadRestServiceMetadata(ServiceLoadState loadState)
        {
            List<RestService> services = loadState.Services;
            try
            {
                HomeController.Processes[processId].Status.Total = loadState.PendingLoadServices;
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

        //private ServiceLoadState AskForUsingLoadState(ServiceLoadState loadState)
        //{
            //ServiceLoadState result = loadState;
            //Console.WriteLine("Press y to continue load or any other key to start new load:");
            //ConsoleKeyInfo keyInfo = Console.ReadKey();

            //if (keyInfo.KeyChar != 'y')
            //{
            //    result = null;
            //}

        //    return null;
        //}

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
    }
}