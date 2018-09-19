using System.Web.Mvc;

namespace DiffRest.Controllers
{
    [RoutePrefix("Change")]
    public class ChangeController : Controller
    {
        // GET: Change
        public ActionResult Index()
        {
            return View();
        }
    }
}
