using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class ErrorController : BaseMobileTemplatesController
    {
        // GET: Mobile/Error
        public ActionResult Error()
        {
            return View();
        }
    }
}