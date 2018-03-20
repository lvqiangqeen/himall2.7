using Himall.Core;
using Himall.Web.Framework;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Himall.IServices;
using System.Threading;
using Himall.Web.Common;
using System;
using System.Web;
using System.Diagnostics;
using FluentValidation.Mvc;

namespace Himall.Web
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			RouteTable.Routes.IgnoreRoute("Areas/");
			//Job.Start();
            //  AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			GlobalConfiguration.Configure(WebApiConfig.Register);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			//RouteDebug.RouteDebugger.RewriteRoutesForTesting(RouteTable.Routes);
			AreaRegistrationOrder.RegisterAllAreasOrder();
			BundleConfig.RegisterBundles(BundleTable.Bundles);

            

            RegistAtStart.RegistStrategies();
            ObjectContainer.ApplicationStart(new AutoFacContainer());
			RegistAtStart.RegistPlugins();
			OnHimallStartMethodAttribute.Start();

			App_Code.Common.LimitOrderHelper.InitLimitOrder();
			//autoFac();
			//  ViewEngines.Engines.Insert(0, new TemplateVisualizationViewEngine());

			ModelValidatorProviders.Providers.Add(new FluentValidationModelValidatorProvider(new CustomValidatorFactory()));
			DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;
		}

		/// <summary>
		/// 自定义验证工厂
		/// </summary>
		public class CustomValidatorFactory : FluentValidation.ValidatorFactoryBase
		{
			public override FluentValidation.IValidator CreateInstance(Type validatorType)
			{
				var type = validatorType.GetGenericArguments()[0];
				var validatorAttribute = type.GetCustomAttribute<FluentValidation.Attributes.ValidatorAttribute>();
				if (validatorAttribute != null)
				{
					//创建验证实体
					var obj = System.Activator.CreateInstance(validatorAttribute.ValidatorType);
					return obj as FluentValidation.IValidator;
				}

				return null;
			}
		}

		protected void Application_End()
		{
			#region 访问首页，重启数据池
			string hosturl = SiteStaticInfo.CurDomainUrl;
#if DEBUG
			Himall.Core.Log.Info(System.DateTime.Now.ToString() + " -  " + hosturl);
#endif
			if (!string.IsNullOrWhiteSpace(hosturl))
			{
				System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(hosturl);
				System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
			}
			#endregion
		}
		/// <summary>
		/// 使用AutoFac实现依赖注入 Autofac.Integration.Mvc
		/// </summary>
		//private void autoFac()
		//{
		//    var builder = new ContainerBuilder();
		//    SetupResolveRules(builder);  //注入
		//    builder.RegisterControllers(Assembly.GetExecutingAssembly());  //注入所有Controller
		//    var container = builder.Build();
		//    DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
		//}




		//private void SetupResolveRules(ContainerBuilder builder)
		//{
		//    //UI项目只用引用service和repository的接口，不用引用实现的dll。
		//    //如需加载实现的程序集，将dll拷贝到bin目录下即可，不用引用dll
		//    var IServices = Assembly.Load("Himall.IServices");
		//    var Services = Assembly.Load("Himall.Service");
		//    builder.RegisterAssemblyTypes(Services,IServices).Where(t => t.Name.EndsWith("Service")).AsImplementedInterfaces().InstancePerRequest();
		//    //根据名称约定（服务层的接口和实现均以Service结尾），实现服务接口和服务实现的依赖
		//  //   builder.RegisterAssemblyTypes(Services).Where(t => t.Name.EndsWith("Service")).AsImplementedInterfaces().InstancePerRequest(); 
		//}
		//protected void Application_EndRequest()
		//{
		//    var statusCode = Context.Response.StatusCode;
		//    var routingData = Context.Request.RequestContext.RouteData;
		//    if (statusCode == 404 || routingData.DataTokens.Keys.Count==0)
		//    {
		//        Response.Clear();
		//        var area = routingData.DataTokens.Keys.Count==0?"":routingData.DataTokens["area"].ToString();
		//        if (area == "Admin")
		//        {
		//            Response.RedirectToRoute("Admin_default", new { controller = "BackError", action = "NotFound", IsReload = 1 });
		//        }
		//        else if(area=="SellerAdmin")
		//        {
		//            Response.RedirectToRoute("SellerAdmin_default", new { controller = "Error", action = "NotFound", id = UrlParameter.Optional });
		//        }else{
		//            Response.RedirectToRoute("Web_default", new { controller = "Error", action = "Error404", id = UrlParameter.Optional });
		//        }

		//    }
		//}
#if DEBUG
		protected DateTime dt;
		protected void Application_BeginRequest(Object sender, EventArgs E)
		{
			dt = DateTime.Now;
			//Core.Log.Debug(dt.ToString("yyyy-MM-dd hh:mm:ss fff") + ":[当前请求URL：" + HttpContext.Current.Request.Url + "；请求的参数为：" + HttpContext.Current.Request.QueryString + "；页面开始时间：" + dt.ToString("yyyy-MM-dd hh:mm:ss fff")+"]");

		}
		protected void Application_EndRequest(Object sender, EventArgs E)
		{
			DateTime dt2 = DateTime.Now;
			TimeSpan ts = dt2 - dt;
			if (ts.TotalMilliseconds >= 5000)//5秒以上的慢页面进行记录
				Core.Log.Debug(dt2.ToString("yyyy-MM-dd hh:mm:ss fff") + ":[当前请求URL：" + HttpContext.Current.Request.Url + "；请求的参数为：" + HttpContext.Current.Request.QueryString + "；页面加载的时间：" + ts.TotalMilliseconds.ToString() + " 毫秒]");
		}
#endif
	}

	/// <summary>
	/// 性能查询
	/// </summary>
	/*使用说明
		#if DEBUG
		[ActionPerformance]
		#endif
		public ActionResult Detail(string id = "", long partnerid=0)
	*/
	public class ActionPerformance : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			GetTimer(filterContext, "action").Stop();

			base.OnActionExecuted(filterContext);
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{

			GetTimer(filterContext, "action").Start();

			base.OnActionExecuting(filterContext);
		}



		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{

			var renderTimer = GetTimer(filterContext, "render");
			renderTimer.Stop();

			var actionTimer = GetTimer(filterContext, "action");
			var response = filterContext.HttpContext.Response;

			if (response.ContentType == "text/html")
			{
				if (actionTimer.ElapsedMilliseconds >= 5000 || renderTimer.ElapsedMilliseconds >= 5000)
				{
					string result = String.Format(
						 "{0}.{1}, Action执行时间: {2}毫秒, View执行时间: {3}毫秒.",
						 filterContext.RouteData.Values["controller"],
						 filterContext.RouteData.Values["action"],
						 actionTimer.ElapsedMilliseconds,
						 renderTimer.ElapsedMilliseconds
					 );
					Himall.Core.Log.Debug(result);
				}
			}

			base.OnResultExecuted(filterContext);
		}

		public override void OnResultExecuting(ResultExecutingContext filterContext)
		{
			GetTimer(filterContext, "render").Start();

			base.OnResultExecuting(filterContext);
		}

		private Stopwatch GetTimer(ControllerContext context, string name)
		{
			string key = "__timer__" + name;
			if (context.HttpContext.Items.Contains(key))
			{
				return (Stopwatch)context.HttpContext.Items[key];
			}

			var result = new Stopwatch();
			context.HttpContext.Items[key] = result;
			return result;
		}

	}
}
