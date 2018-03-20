using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;

using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class MarketingController : BaseAdminController
    {

        // GET: Admin/sale
        public ActionResult Management()
        {
            ViewBag.Rights = string.Join(",", CurrentManager.AdminPrivileges.Select(a => (int)a).OrderBy(a => a));
            return View();
        }
    }
}