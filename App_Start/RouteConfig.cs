using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BeeTube
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();
           
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            //comment delete
            // Your other routes...

            routes.MapRoute(
                name: "DeleteComment",
                url: "Creator/DeleteComment/{videoId}/{id}",
                defaults: new { controller = "Creator", action = "DeleteComment" }
            );

        }
    }
}
