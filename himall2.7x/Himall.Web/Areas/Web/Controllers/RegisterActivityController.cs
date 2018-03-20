using Himall.Model;
using Himall.Web.Framework;
using System.Web.Mvc;
using System;
using Himall.Core;
using Himall.Core.Plugins.Message;
using System.Threading.Tasks;
using Himall.Web.Areas.Web.Models;
using Himall.Application;

namespace Himall.Web.Areas.Web.Controllers
{
    public class RegisterActivityController : BaseController
    {
      
        /// <summary>
        /// 注册有礼
        /// </summary>
        /// <returns></returns>
        public ActionResult Gift()
        {
            if (IsMobileTerminal)
            {
                Response.Redirect("/m-Wap/RegisterActivity/Gift");
            }
            var model = CouponApplication.GetCouponSendByRegister();
            return View(model);
        }
    }
}