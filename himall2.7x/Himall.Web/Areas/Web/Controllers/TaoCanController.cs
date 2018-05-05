using Himall.Model;
using Himall.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Web.Controllers
{
	public class TaoCanController : Controller
	{
		// GET: Web/TaoCan
		public ActionResult Index()
		{
			taocan_tcitemService taocanService = new taocan_tcitemService();
			List<taocan_tcmenu> taocanMenuList = taocanService.GetTaoCanMenus();
			ViewBag.taocanMenuList = taocanMenuList;
			return View();
		}


		public ActionResult TaoCanInfo(int id)
		{
			taocan_tcitemService ser = new taocan_tcitemService();
			taocao_tcinfo info = ser.GetTaoCanInfo(id);
			List<taocan_tcmenu> mainMenus = ser.GetTaoCanMenus().Where(me => me.Id <= 6 && me.parent_id == 0).ToList();
			ViewBag.TcInfo = info;
			ViewBag.MainMenus = mainMenus;
			return View();
		}
	}
}