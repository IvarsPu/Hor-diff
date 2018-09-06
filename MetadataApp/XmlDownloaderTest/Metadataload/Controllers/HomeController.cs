using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Metadataload.Models;

namespace Metadataload.Controllers
{
    [RoutePrefix("Home")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Info(int processID)
        {
            ViewBag.Process = new MetadataController().GetProcessStatus(processID);

            return View();
        }

    }
}