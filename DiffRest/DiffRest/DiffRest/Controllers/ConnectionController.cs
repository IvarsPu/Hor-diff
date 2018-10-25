using System.Web.Mvc;
using BusinessLogic;
using Models;

namespace DiffRest.Controllers
{
    public class ConnectionController : Controller
    {
        #region Main page
        public ActionResult Index()
        {
            return View(new ListController().GetConnections());
        }
        #endregion

        #region Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create([Bind(Include = "Name,Url,Username,Password")] RestConnection service)
        {
            if (new Connection().CreateServerConn(service))
            {
                return RedirectToAction("Index", "Connection", new { });
            }
            else
            {
                return View();
            }
        }
        #endregion

        #region Delete
        [HttpPost]
        public void Delete(int id)
        {
            new Connection().DeleteServerConn(id);
        }
        #endregion

        #region Edit
        public ActionResult Edit(int id)
        {
            return View(new BusinessLogic.Connection().GetServerConn(id));
        }

        [HttpPost]
        public ActionResult Edit([Bind(Include = "Id,Name,Url,Username,Password")] RestConnection service)
        {
            if(new Connection().EditServerConn(service))
            {
                return RedirectToAction("Index", "Connection", new { });
            }
            else
            {
                return View();
            }
        }
        #endregion
    }
}
