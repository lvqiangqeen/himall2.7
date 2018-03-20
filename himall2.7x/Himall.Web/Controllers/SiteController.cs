using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Linq;
using Himall.Web.Framework;
using System.Web;
using System.Web.Routing;

namespace Himall.Web.Controllers
{
    public class SiteController : Controller
    {
        // GET: Site
        public ActionResult Close()
        {
            return View();
        }

        public ActionResult State()
        {
            ViewBag.IsDebug = GetSolutionDebugState();
            return View();
        }

        public ActionResult Version()
        {
            string path = Path.Combine( AppDomain.CurrentDomain.BaseDirectory , "bin" , "Himall.Web.dll" );
            Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
            //ViewBag.VersionConfig = ass.GetName().Version.ToString();
            ViewBag.FileVersion = FileVersionInfo.GetVersionInfo( path ).FileVersion;
            ViewBag.IsDebug = GetSolutionDebugState();
            return View();
        }
        public object GetAppDataUrl()
        {
            try
            {
                Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
                var appUrl = config.AppSettings.Settings["AppDateUrl"].Value;
                var appData = new { Success = "true", Url = appUrl };
                return Json(appData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var appData = new { Success = "false", ErrorMsg = ex.Message };
                return Json(appData, JsonRequestBehavior.AllowGet);
            }
        }

		/// <summary>
		/// 记录前端日志,前端日志通过post提交过来
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult Log(string msg)
		{
			Himall.Core.Log.Debug(msg);
			return new EmptyResult();
		}

        public ActionResult pay()
        {
            return View();
        }

		#region 私有方法
		private bool GetSolutionDebugState()
		{
#if !DEBUG
            return false;
#elif DEBUG
			return true;
#endif
			//CompilationSection compilation = ConfigurationManager.GetSection( "system.web/compilation" ) as CompilationSection;
			//return compilation.Debug;
		}

		private System.Web.Routing.RouteData GetRouteDataFromUrl(string url)
		{
			if (string.IsNullOrEmpty(url))
				return null;

			url = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, url.TrimStart('/'));
			var hr = new HttpRequest("", url, "");//这里的url就是要解析的url
			var stringWriter = new StringWriter();
			var hrs = new HttpResponse(stringWriter);
			var hc = new HttpContext(hr, hrs);
			var hcw = new HttpContextWrapper(hc);

			foreach (Route route in System.Web.Routing.RouteTable.Routes)
			{
				var routeData = route.GetRouteData(hcw);
				if (routeData != null)
					return routeData;
			}

			return null;
		}
		#endregion
    }
}