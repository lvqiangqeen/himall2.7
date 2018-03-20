using Himall.Core.Helper;
using Himall.IServices;
using Himall.Web.Framework;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Himall.Web.Framework
{
    /// <summary>
    /// 商城前台基础控制器类
    /// </summary>
    public abstract class BaseMemberController : BaseWebController
    {
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            //filterContext.Controller.ViewBag.Keyword = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().Keyword;
            //不能应用在子方法上
            if (filterContext.IsChildAction)
                return;
            if (CurrentUser == null || CurrentUser.Disabled)
            {
                if (Core.Helper.WebHelper.IsAjax())
                {
                    Result result = new Result();
                    result.msg = "登录超时,请重新登录！";
                    result.success = false;
					filterContext.Result = Json(result, JsonRequestBehavior.AllowGet);
                    return;
                }
                else
                {
                   
                        HttpRequestBase bases = (HttpRequestBase)filterContext.HttpContext.Request;
                        string url = bases.RawUrl.ToString();
                        string returnurl = System.Web.HttpUtility.HtmlEncode(url);
                        var result = RedirectToAction("", "Login", new { area = "Web", returnUrl = returnurl });
                        if (CurrentSellerManager != null && CurrentSellerManager.UserName.IndexOf(":")>0 && !IsMobileTerminal)
                            result = RedirectToAction("index", "Home", new { area = "SellerAdmin" });
                        //TODO:DZY[150731] 手机端登录由手机端自行处理
                        /*
                         * 代码块应包含在花括号内
                         */
                        if (!IsMobileTerminal)
                            filterContext.Result = result;
                        return;
                        //跳转到登录页
                }
            }
        }


        /// <summary>
        /// 获得路由中的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        protected string GetRouteString(string key, string defaultValue)
        {
            object value = RouteData.Values[key];
            if (value != null)
                return value.ToString();
            else
                return defaultValue;
        }

        /// <summary>
        /// 获得路由中的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        protected string GetRouteString(string key)
        {
            return GetRouteString(key, "");
        }

        /// <summary>
        /// 获得路由中的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        protected int GetRouteInt(string key, int defaultValue)
        {
            return TypeHelper.ObjectToInt(RouteData.Values[key], defaultValue);
        }

        /// <summary>
        /// 获得路由中的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        protected int GetRouteInt(string key)
        {
            return GetRouteInt(key, 0);
        }


    }
}
