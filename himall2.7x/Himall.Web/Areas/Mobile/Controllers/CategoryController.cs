using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.IServices;
using Himall.Model;
using Himall.Application;

namespace Himall.Web.Areas.Mobile.Controllers
{
	public class CategoryController : BaseMobileTemplatesController
	{
		// GET: Mobile/Category
		public ActionResult Index()
		{
			var model = CategoryApplication.GetSubCategories();
			return View(model);
		}
	}
}