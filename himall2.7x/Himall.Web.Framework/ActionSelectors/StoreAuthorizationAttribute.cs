using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Himall.Web.Framework
{
    /// <summary>
    /// 门店授权权限验证
    /// </summary>
    public class StoreAuthorizationAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 验证权限（action执行前会先执行这里）
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool isOpenStore = Application.SiteSettingApplication.GetSiteSettings() != null && Application.SiteSettingApplication.GetSiteSettings().IsOpenStore;
            if (!isOpenStore)
            {
                //跳转到错误页
                var result = new ViewResult()
                {
                    ViewName = "NoAccess"
                };
                result.TempData.Add("Message", "门店未授权，你没有权限访问此页面");
                result.TempData.Add("Title", "门店未授权，你没有权限访问此页面！");
                filterContext.Result = result;
            }
        }
    }
}
