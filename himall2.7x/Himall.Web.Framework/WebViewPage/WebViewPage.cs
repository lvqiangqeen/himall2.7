using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;


namespace Himall.Web.Framework
{
    /// <summary>
    /// 页面基类型
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>
	{
		public SiteSettingsInfo SiteSetting
		{
			get
			{
				if (this.ViewContext.Controller is BaseController)
					return ((BaseController)this.ViewContext.Controller).CurrentSiteSetting;
				return Application.SiteSettingApplication.GetSiteSettings();
			}
		}

        /// <summary>
        /// 当前用户信息
        /// </summary>
        public UserMemberInfo CurrentUser
        {
            get
			{
				if (this.ViewContext.Controller is BaseController)
					return ((BaseController)this.ViewContext.Controller).CurrentUser;
				return BaseController.GetUser(Request);
            }
        }
        public string Generator
        {
            get
            {
                return "2.7";
            }
        }
    }
    /// <summary>
    /// 页面基类型
    /// </summary>
    public abstract class WebViewPage : WebViewPage<dynamic>
    {

    }
}
