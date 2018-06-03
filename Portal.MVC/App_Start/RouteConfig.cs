using System.Web.Mvc;
using System.Web.Routing;

namespace Portal.MVC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Install", "Install", new
            {
                controller = "Install",
                action = "Install",

            }, new[] { "Portal.MVC.Controllers" });

            routes.MapRoute("WeiXin", "WeiXinAuth", new
            {
                controller = "ExternalAuthWeiXin",
                action = "Index",
                id = UrlParameter.Optional,

            }, new[] { "Portal.MVC.Controllers" });

            routes.MapRoute("Default", "{controller}/{action}/{id}", new
            {
                controller = "Home",
                action = "Index",
                id = UrlParameter.Optional,

            }, new[] { "Portal.MVC.Controllers" });
        }
    }
}