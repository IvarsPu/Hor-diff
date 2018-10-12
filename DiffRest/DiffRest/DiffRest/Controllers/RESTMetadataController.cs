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
        
        public ActionResult StartMetadataLoad(int metadataServiceId)
        {
            if (new DownloadMetadata().DoTask(metadataServiceId))
            {//started
                return RedirectToAction("Index", "RESTMetadata");
            }
            else
            {//cant be started
                return RedirectToAction("Index", "RESTMetadata");
            }
        }

        public ActionResult Exists(int id)
        {
            try
            {
                if (new DownloadMetadata().AlreadyExists(id))
                {
                    return View(id);
                }
                else
                {
                    return RedirectToAction("StartMetadataLoad", "RESTMetadata", new { metadataServiceId = id });
                }
            }
            catch
            {
                return RedirectToAction("Index", "RESTMetadata");
            }
        }

        public ActionResult Info(int processId)
        {
            return View(AppInfo.Processes.TryGetValue(processId, out Process value) ? value : null);
        }
        
        [Route("StopProcess")]
        public void StopProcess(int processId)
        {
            AppInfo.Processes[processId].TokenSource.Cancel();
        }
    }
}