using System.Web.Mvc;

namespace Metadataload.Controllers
{
    [RoutePrefix("Process")]
    public class ProcessController : Controller
    {
        public ActionResult Index(int id)
        {
            ViewBag.Id = id;
            return View();
        }

        public ActionResult Info(int processID = 0)
        {
            ViewBag.Process = new MetadataController().GetProcessStatus(processID);

            return View();
        }

    }
}