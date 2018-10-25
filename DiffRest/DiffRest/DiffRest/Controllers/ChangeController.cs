using System.Collections.Generic;
using Models;
using System.Web.Http;
using System.Net.Http;

namespace DiffRest.Controllers
{
    [RoutePrefix("Change")]
    public class ChangeController : System.Web.Mvc.Controller
    {
        [Route("Index")]
        public System.Web.Mvc.ActionResult Index()
        {
            return View();
        }
    }

    [RoutePrefix("Home")]
    public class HomeController : ApiController
    {
        [Route("GetVersions")]
        public IList<HorizonVersion> GetVersions()
        {
            return new BusinessLogic.ChangeController().GetHorizonVersions();
        }

        [Route("LoadFile")]
        [HttpGet]
        public HttpResponseMessage LoadFile(string first, string second)
        {
            return new BusinessLogic.ChangeController().LoadFile(first, second);
        }

        [Route("DiffColor")]
        [HttpGet]
        public string DiffColor(string firstFile, string secondFile)
        {
            return new BusinessLogic.ChangeController().DiffColor(firstFile, secondFile);
        }

        [Route("GetFile")]
        public string GetFile(string filePath)
        {
            return new BusinessLogic.ChangeController().GetFile(filePath);
        }
    }
}
