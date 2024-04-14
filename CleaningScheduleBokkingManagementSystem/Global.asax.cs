using Newtonsoft.Json;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace CleaningScheduleBokkingManagementSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Configure JSON serialization settings to ignore circular references
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }
    }
}
