using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ManagersGradeController : BaseAdminController
    {
        // GET: Admin/ManagersGrade
        public ActionResult Index()
        {
            return View();
        }
    }
}