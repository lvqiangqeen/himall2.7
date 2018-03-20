using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class AdminPublicController : BaseAdminController
    {
        public ActionResult Top()
        {
            var t = ConfigurationManager.AppSettings["IsInstalled"];
            if (!(null == t || bool.Parse(t)))
            {
                return RedirectToAction("Agreement", "Installer", new { area = "Web" });
            }
            return View();
        }

        [ChildActionOnly]
        public ActionResult Bottom()
        {
            ViewBag.Rights  = string.Join(",", CurrentManager.AdminPrivileges.Select(a => (int)a).OrderBy(a => a));
            return View();
        }

    }
}