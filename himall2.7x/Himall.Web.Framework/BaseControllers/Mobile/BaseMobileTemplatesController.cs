using System.Web.Mvc;

namespace Himall.Web.Framework
{
    /// <summary>
    /// 移动端控制器基类(带模板)
    /// </summary>
    public abstract class BaseMobileTemplatesController : BaseMobileController
    {
        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {

            var viewResult = filterContext.Result as ViewResult;
            if (viewResult != null)
            {
                var currentUserTemplate = "Default";
                if (PlatformType == Core.PlatformType.IOS || PlatformType == Core.PlatformType.Android)
                    currentUserTemplate = "APP";
                var template = string.IsNullOrEmpty(currentUserTemplate) ? "" : currentUserTemplate;
                var controller = filterContext.RequestContext.RouteData.Values["Controller"].ToString();
                var action = filterContext.RequestContext.RouteData.Values["Action"].ToString();
                if (string.IsNullOrWhiteSpace(viewResult.ViewName))
                {
                    viewResult.ViewName = string.Format(
                        "~/Areas/Mobile/Templates/{0}/Views/{1}/{2}.cshtml",
                        template,
                        controller,
                        action);
                    return;
                }
                else if(!viewResult.ViewName.EndsWith(".cshtml"))
                {
                    viewResult.ViewName = string.Format(
                         "~/Areas/Mobile/Templates/{0}/Views/{1}/{2}.cshtml",
                         template,
                         controller,
                         viewResult.ViewName);
                    return;
                }
            }
            base.OnResultExecuting(filterContext);
        }
    }
}
