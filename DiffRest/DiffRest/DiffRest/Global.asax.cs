using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DiffRest.Controllers;
using Models;

namespace DiffRest
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AppInfo.path = HttpContext.Current.Server.MapPath("~/test_place/HorizonRestMetadataService.xml");
            AppInfo.MetadataRootFolder = HttpContext.Current.Server.MapPath("~/test_place/MetadataLocalFolder/");
            AppInfo.FolderLocation = HttpContext.Current.Server.MapPath("~/test_place/Projects/");
        }
    }
}
