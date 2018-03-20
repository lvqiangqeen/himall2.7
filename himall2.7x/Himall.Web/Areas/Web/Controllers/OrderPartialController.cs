using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Web.Controllers
{
    public class OrderPartialController : BaseMemberController
    {
        public ActionResult TopBar()
        {
            InitTopBar();
            return PartialView("~/Areas/Web/Views/Shared/OrderTopBar.cshtml");
        }


        void InitTopBar()
        {
            //会员信息
            ViewBag.Member = CurrentUser;
        }
    }
}