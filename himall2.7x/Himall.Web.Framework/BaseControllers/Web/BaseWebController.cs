using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Himall.Application;

namespace Himall.Web.Framework
{
	public abstract class BaseWebController : BaseController
	{
		ISellerManager sellerManager = null;

		public long UserId
		{
			get
			{
				if (CurrentUser != null)
					return CurrentUser.Id;
				return 0;
			}
		}

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.IsChildAction)
            {
                return;
            }
            StatisticApplication.StatisticPlatVisitUserCount();
            base.OnActionExecuted(filterContext);
        }

		/// <summary>
		/// 当前管理员
		/// </summary>
		public ISellerManager CurrentSellerManager
		{
			get
			{
				if (sellerManager != null)
				{
					return sellerManager;
				}
				else
				{
					long userId = UserCookieEncryptHelper.Decrypt(WebHelper.GetCookie(CookieKeysCollection.SELLER_MANAGER), CookieKeysCollection.USERROLE_SELLERADMIN);
					if (userId != 0)
					{
						sellerManager = ServiceHelper.Create<IManagerService>().GetSellerManager(userId);
					}
				}
				return sellerManager;
			}
		}
	}
}
