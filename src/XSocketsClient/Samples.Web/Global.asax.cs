using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using XSockets.Plugin.Framework;
using XSockets.Server;

namespace Samples.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private IXSocketsRelayContainer _container;
        protected void Application_Start()
        {
            // Start the MVC->XSockets bridge
            _container = Composable.GetExport<IXSocketsRelayContainer>();
            _container.Start();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
