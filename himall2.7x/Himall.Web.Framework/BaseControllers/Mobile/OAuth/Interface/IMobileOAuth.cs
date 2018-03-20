
using System.Web.Mvc;
namespace Himall.Web.Framework
{
    /// <summary>
    /// 移动端信任登录
    /// </summary>
    interface IMobileOAuth
    {
        /// <summary>
        /// 获取移动端信任登录OpenId
        /// </summary>
        /// <returns></returns>
        MobileOAuthUserInfo GetUserInfo(ActionExecutingContext filterContext, out string redirectUrl);

        MobileOAuthUserInfo GetUserInfo(ActionExecutingContext filterContext, out string redirectUrl, Model.WXShopInfo settings);

        MobileOAuthUserInfo GetUserInfo_bequiet(ActionExecutingContext filterContext, out string redirectUrl, Model.WXShopInfo settings);
    }
}
