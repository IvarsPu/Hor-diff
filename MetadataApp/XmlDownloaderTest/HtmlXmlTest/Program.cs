namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using static HtmlXmlTest.RestService;

    internal class Program
    {
        private string rootUrl;
        private string rootLocalPath;
        private string metadataPath;
        private WebResourceLoader webResourceLoader;

        public static void Main(string[] args)
        {
            Program prog = new Program();

            prog.DoTheJob(args);

            Console.WriteLine("Work complete!");
            Console.WriteLine("Press any key to close the window");
            Console.ReadKey();
        }

        public void DoTheJob(string[] args)
        {

            this.rootUrl = ConfigurationManager.ConnectionStrings["Server"].ConnectionString;
            this.rootLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\rest\\";

            // Set the initial log path in root until the version folder is not known
            Logger.LogPath = this.rootLocalPath;

            //ServiceLoadState serviceState = this.LoadRestServiceTestState();
            ServiceLoadState serviceState = this.LoadRestServiceLoadState();

            serviceState.Services.RemoveRange(100, serviceState.Services.Count - 100);
            serviceState.CalcStatistics();

            int remainingServiceCount = 0;
            Logger.LogPath = this.rootLocalPath;

            while (serviceState.PendingLoadServices > 0 && remainingServiceCount != serviceState.PendingLoadServices)
            {
                remainingServiceCount = serviceState.PendingLoadServices;
                serviceState = this.LoadRestServiceMetadata(serviceState);
                serviceState.Services = this.GetPendingServices(serviceState.Services);
                serviceState.CalcStatistics();
            }
        }

        public ServiceLoadState LoadRestServiceTestState()
        {
            ServiceLoadState loadState = new ServiceLoadState();
            List<RestService> services = new List<RestService>();
            loadState.Services = services;

            try
            {
                Logger.LogInfo("Getting REST service structure");
                WebResourceReader.LoadRestServices(this.rootUrl, ref this.rootLocalPath, ref this.metadataPath);

                this.webResourceLoader = new WebResourceLoader(this.rootUrl, this.rootLocalPath);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                Logger.LogError(ex.StackTrace);
            }

            RestService service = new RestService();
            service.Href = this.rootUrl;
            service.Name = "TsarVDocIzd";
            service.Filepath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\rest\\520.1\\Avansa_norēķini\\TsarVDocIzd";
            services.Add(service);

            loadState.CalcStatistics();

            return loadState;
        }

        public ServiceLoadState LoadRestServiceLoadState()
        {
            ServiceLoadState loadState = null;
            List<RestService> services = null;

            try
            {
                Logger.LogInfo("Getting REST service structure");
                services = WebResourceReader.LoadRestServices(this.rootUrl, ref this.rootLocalPath, ref this.metadataPath);
                loadState = new ServiceLoadState();
                loadState.Services = services;

                this.webResourceLoader = new WebResourceLoader(this.rootUrl, this.rootLocalPath);
                ServiceLoadState savedState = this.webResourceLoader.GetServiceLoadState();

                if (savedState != null)
                {
                    Logger.LogInfo("Have found previous service load state");
                    this.LogState(savedState);
                    savedState = this.AskForUsingLoadState(savedState);
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
                Logger.LogInfo(string.Format("Loading REST metadata for {0} services", services.Count));
                this.webResourceLoader.LoadServiceMetadata(services, this.metadataPath).Wait();

                Logger.LogInfo("Generating json tree data");
                JsonGenerator.generateJSONMetadata(this.rootLocalPath, this.metadataPath);

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
            Logger.LogInfo("Loaded With Errors: " + loadState.LoadedWithErrors);
            Logger.LogInfo("Failed: " + loadState.Failed);
            Logger.LogInfo("Pending Load: " + loadState.NotLoaded);
        }

        private ServiceLoadState AskForUsingLoadState(ServiceLoadState loadState)
        {
            ServiceLoadState result = loadState;
            Console.WriteLine("Press y to continue load or any other key to start new load:");
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            if (keyInfo.KeyChar == 'y')
            {
                result.Services = this.GetPendingServices(loadState.Services);
            }
            else
            {
                result = null;
            }

            return result;
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
    }
}
