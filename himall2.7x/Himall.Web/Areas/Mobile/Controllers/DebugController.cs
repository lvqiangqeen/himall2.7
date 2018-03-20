using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.Core.Helper;
using Himall.IServices;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class DebugController : BaseMobileTemplatesController
    {
       private IMemberService _iMemberService;
        public DebugController(IMemberService iMemberService)
        {
            _iMemberService = iMemberService;
        }
        // GET: Mobile/Debug
        public ActionResult Index()
        {
            return View();
        }

		[HttpPost]
		public JsonResult Logout()
		{
			var cacheKey = WebHelper.GetCookie(CookieKeysCollection.HIMALL_USER);
			if (!string.IsNullOrWhiteSpace(cacheKey))
			{
				
				//_iMemberService.DeleteMemberOpenId(userid, string.Empty);
				WebHelper.DeleteCookie(CookieKeysCollection.HIMALL_USER);

				WebHelper.DeleteCookie(CookieKeysCollection.SELLER_MANAGER);
				//记录主动退出符号
				WebHelper.SetCookie(CookieKeysCollection.HIMALL_ACTIVELOGOUT, "1", DateTime.MaxValue);
				return Json(new { success = true });
			}
			return Json(new { success = false });
		}
    }
}