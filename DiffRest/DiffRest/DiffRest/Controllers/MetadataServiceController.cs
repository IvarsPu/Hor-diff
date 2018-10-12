using System.Web.Mvc;
using BusinessLogic;
using Models;

namespace DiffRest.Controllers
{
    [RoutePrefix("MetadataService")]
    public class MetadataServiceController : Controller
    {
        public ActionResult Index()
        {
            return View(new ListController().GetMetadataServices());
        }

        #region Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create([Bind(Include= "Name,Url,Username,Password")] MetadataService service)
        {
            if (new ServerConn().CreateServerConn(service))
            {
                return RedirectToAction("Index", "MetadataService", new { });
            }
            else
            {
                return View();
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(int id)
        {
            new ServerConn().DeleteServerConn(id);
            return RedirectToAction("Index", "MetadataService", new { });
        }
        #endregion

        #region Edit
        public ActionResult Edit(int id)
        {
            return View(new ServerConn().GetServerConn(id));
        }

        [HttpPost]
        public ActionResult Edit([Bind(Include = "Id,Name,Url,Username,Password")] MetadataService service)
        {
            if(new ServerConn().EditServerConn(service))
            {
                return RedirectToAction("Index", "MetadataService", new { });
            }
            else
            {
                return View();
            }
        }
        #endregion
    }
}
