using System.Web;
using System.Web.Configuration;
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

            string MetadataRootFolder = WebConfigurationManager.AppSettings["MetadataRootFolder"];
            if (!MetadataRootFolder.EndsWith("\\")) 
            {
                MetadataRootFolder += "\\";
            }
            AppInfo.path = MetadataRootFolder + "HorizonRestMetadataService.xml";
            AppInfo.MetadataRootFolder = MetadataRootFolder + "MetadataLocalFolder\\";
            AppInfo.FolderLocation = MetadataRootFolder + "Projects\\";
        }
    }
}
