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
using Himall.Core;
using System.Web.Routing;


namespace Himall.Web.Framework
{
    /// <summary>
    /// 页面基类型
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class MobileWebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>
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

		public CommonModel.Model.WeiXinShareArgs WeiXinShareArgs
		{
			get
			{
				if (this.ViewContext.Controller is BaseMobileController)
					return ((BaseMobileController)this.ViewContext.Controller).WeiXinShareArgs;
				return null;
			}
		}

        public string CurrentAreaName
        {
            get
            {
                string result = "m";
                result = ViewBag.AreaName;
                return result;
            }
        }
    }
    /// <summary>
    /// 页面基类型
    /// </summary>
    public abstract class MobileWebViewPage : MobileWebViewPage<dynamic>
    {

    }
}
