using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using BusinessLogic;
using Models;

namespace DiffRest.Controllers
{
    [RoutePrefix("RESTMetadata")]
    public class RESTMetadataController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Route("StartMetadataLoad")]
        public void StartMetadataLoad(int metadataServiceId)
        {
            if (MetadataServiceController.GetMetadataService(metadataServiceId) != null)
            {
                int processId = 1;
                if (AppInfo.Processes.Count > 0)
                {
                    foreach (Process processValue in AppInfo.Processes.Values)
                    {
                        if (processValue.MetadataServiceId == metadataServiceId && !processValue.Done)
                        {
                            throw new Exception();
                        }
                    }
                    processId = AppInfo.Processes.Last().Key + 1;
                }

                Process process = new Process(processId, metadataServiceId, DateTime.Now);
                AppInfo.Processes.Add(processId, process);
                
                System.Threading.Tasks.Task.Run(() => new Program().DoTheJob(processId));
            }
            else
            {
                throw new Exception();
            }
        }

        //public bool AlreadyExists(int processId)
        //{
        //    MetadataService profile = MetadataServiceController.GetMetadataService(Process.Processes[processId].MetadataServiceId);
        //    Models.AppContext appContext = new Models.AppContext(profile.Url, profile.Username, profile.Password, HomeController.MetadataRootFolder);
        //    Logger.LogPath = appContext.RootLocalPath;
        //    XmlMetadata xmlMetadata = new XmlMetadata(appContext);
        //    WebResourceLoader webResourceLoader = new WebResourceLoader(appContext, xmlMetadata, processId);
        //    xmlMetadata.InitServiceMetadata(webResourceLoader);

        //    XmlDocument doc = new XmlDocument();
        //    doc.Load(HomeController.MetadataRootFolder + "Versions.xml");
        //    XmlNode node = doc.SelectSingleNode("//version[@name='" + appContext.Version + "']/release[@name='" + appContext.Release + "']");
        //    if (node != null)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        public ActionResult Info(int processId)
        {
            Process metadata = AppInfo.Processes.TryGetValue(processId, out Process value) ? value : null;
            return View(metadata);
        }
        
        [Route("StopProcess")]
        public void StopProcess(int processId)
        {
            AppInfo.Processes[processId].TokenSource.Cancel();
        }
    }
}