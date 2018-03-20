using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Himall.Web
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.Add("PayRoute", new PayRoute("common/site/pay", new { controller = "site", action = "pay" }));

			routes.MapRoute(
				name: "Default",
				url: "common/{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
				constraints: null
			);
		}

		public class PayRoute : Route
		{
			public PayRoute(string url, object defaults)
				: base(url, new RouteValueDictionary(defaults), new MvcRouteHandler())
			{
			}

			public override RouteData GetRouteData(System.Web.HttpContextBase httpContext)
			{
				var baseRouteData = base.GetRouteData(httpContext);
				if (baseRouteData == null)
					return null;

				var queryString = httpContext.Request.QueryString;
				foreach (string key in queryString.Keys)
				{
					if ("area".Equals(key, System.StringComparison.OrdinalIgnoreCase))
					{
						var area = queryString[key];
						baseRouteData.DataTokens["namespaces"] = GetAreaNamespaces(area);
						baseRouteData.DataTokens["area"] = area;
					}
					else
						baseRouteData.Values[key] = queryString[key];
				}

				return baseRouteData;
			}

			private object GetAreaNamespaces(string area)
			{
				if (string.IsNullOrEmpty(area))
					return null;

				foreach (var route in RouteTable.Routes)
				{
					if (route is Route)
					{
						var temp = (Route)route;
						if (temp.DataTokens != null && temp.DataTokens.ContainsKey("area") && area.Equals(temp.DataTokens["area"] as string, System.StringComparison.OrdinalIgnoreCase))
							return temp.DataTokens["namespaces"];
					}
				}

				return null;
			}
		}
	}
}
