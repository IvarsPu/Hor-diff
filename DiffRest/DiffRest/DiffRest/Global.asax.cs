﻿using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DiffRest.Controllers;

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

            ProfileController.path = HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["location"].ToString() + WebConfigurationManager.AppSettings["profileInfo"].ToString());
            HomeController.MetadataRootFolder = HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["location"].ToString() + WebConfigurationManager.AppSettings["MetadataLocalFolder"].ToString());
            HomeController.FolderLocation = HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["location"].ToString() + WebConfigurationManager.AppSettings["result"].ToString());
        }
    }
}
