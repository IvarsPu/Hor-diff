using System.Web.Mvc;
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
        
        public ActionResult StartMetadataLoad(int metadataServiceId = 0)
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

        public ActionResult Exists(int id=0)
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

        public ActionResult Info(int processId = 0)
        {
            return View(AppInfo.Processes.TryGetValue(processId, out Process value) ? value : null);
        }
        
        public ActionResult StopProcess(int processId = 0)
        {
            try
            {
                AppInfo.Processes[processId].TokenSource.Cancel();
                return RedirectToAction("Index", "RESTMetadata");
            }
            catch
            {
                return RedirectToAction("Info", "RESTMetadata", new { processId });
            }
        }
    }
}