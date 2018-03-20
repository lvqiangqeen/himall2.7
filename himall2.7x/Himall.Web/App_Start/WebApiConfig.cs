using System.Web.Http;
using Himall.Web.Framework;
using System.Web.Routing;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Routing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Dispatcher;

namespace Himall.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 配置和服务

            // Web API 路由
            config.MapHttpAttributeRoutes();
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            config.Routes.MapHttpRoute(
                name: "DefaultOpenApi",
                routeTemplate: "openapi/Hishop.Open.Api.I{controller}.{action}/{id}",
                defaults: new { controller = "OPATest", action = "Get", id = RouteParameter.Optional,defaultnamespace= "openapi" }
            );
#if DEBUG
            Himall.Core.Log.Debug("[OpenApi]RegRoute-DefaultOpenApi");
#endif

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { Controller = "Test", Action = "Get", id = RouteParameter.Optional, defaultnamespace = "api" }
            );
            config.Routes.MapHttpRoute(
                name: "SmallApi",
                routeTemplate: "SmallProgAPI/{controller}/{action}/{id}",
                defaults: new { Controller = "Test", Action = "Get", id = RouteParameter.Optional, defaultnamespace = "SmallProgAPI" }
            );
            config.Services.Replace(typeof(IHttpControllerSelector), new NamespaceHttpControllerSelector(config));


        }

    }
    
}
