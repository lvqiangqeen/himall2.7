using Himall.Web.Framework;
using System.Web.Mvc;

namespace Himall.Web.Areas.Mobile.Controllers
{
    /// <summary>
    /// 跳转页面，当前控制器需要登录
    /// </summary>
    public class RedirectController :  BaseMobileMemberController
    {
        /// <summary>
        /// 用于需要登录的页面做跳转中继。
        /// 例如：A页面不需要登录，但某个操作需要登录，此时可以先跳转至本action，本action会完成登录流程后跳转回redirectUrl
        /// </summary>
        /// <param name="redirectUrl"></param>
        /// <returns></returns>
        public ActionResult Index(string redirectUrl)
        {
            return Redirect(redirectUrl);
        }
    }
}