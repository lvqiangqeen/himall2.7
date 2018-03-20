using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Web.Controllers
{
    public class ErrorController : BaseController
    {
        // GET: Web/Common
        public ActionResult Error404()
        {
            return View();
        }

        public ActionResult DefaultError()
        {
            return View();
        }
    }
}