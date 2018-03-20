using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Himall.CommonModel;
using Himall.Web.Models;
using Himall.Application;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class BillingController : BaseSellerController
    {

        // GET: SellerAdmin/Billing
        public ActionResult Index()
        {
            var shopID = CurrentSellerManager.ShopId;
            var model = BillingApplication.GetShopBillingIndex(shopID);
            ViewBag.IsMainAccount = CurrentSellerManager.IsMainAccount;
            return View(model);
        }

        [HttpPost]
        public ActionResult GetSevenDaysTradeChart()
        {
            var enddate = DateTime.Now.Date;
            var startdate = DateTime.Now.AddDays(-6);
            var data= BillingApplication.GetTradeChart(startdate, enddate, CurrentSellerManager.ShopId);
            return Json(new { successful = true, chart = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetThirdtyDaysTradeChart()
        {
            var enddate = DateTime.Now.Date;
            var startdate = DateTime.Now.AddDays(-29);
            var data = BillingApplication.GetTradeChart(startdate, enddate, CurrentSellerManager.ShopId);
            return Json(new { successful = true, chart = data }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetTradeChartMonthChart()
        {
            var enddate = DateTime.Now;
            var startdate = new DateTime(enddate.Year, enddate.Month, 1);
            var data = BillingApplication.GetTradeChartMonth(startdate, enddate,CurrentSellerManager.ShopId);
            return Json(new { successful = true, chart = data }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult PendingSettlementOrders()
        {
            var model = BillingApplication.GetShopSettlementCycle(CurrentSellerManager.ShopId);
            return View(model);
        }

		/// <summary>
		/// 已结算列表
		/// </summary>
		/// <returns></returns>
		public ActionResult SettlementList()
		{
			return View();
		}

		[HttpPost]
		public JsonResult SettlementList(DateTime? startDate, DateTime? endDate, int page, int rows)
		{
			if (startDate == null)
				startDate = endDate.HasValue ? endDate.Value.AddYears(-1).Date : DateTime.Now.AddYears(-1).Date;

			var query = new ShopAccountItemQuery();
			query.PageNo = page;
			query.PageSize = rows;
			query.TimeStart = startDate;
			query.TimeEnd = endDate;
			query.ShopAccountType = ShopAccountType.SettlementIncome;
			query.ShopId = CurrentSellerManager.ShopId;

			var data = BillingApplication.GetShopAccountItem(query);
			if (data.Models == null || data.Models.Count == 0)
				return Json(new { rows = new object[0], total = 0 }, true);

			var models = data.Models.Select(item => new
			{
				Id = item.Id,
				Amount = item.Amount,
				DetailId = item.DetailId,
				CreateTime = item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
				Cycle = string.Format("{0:yyyy-MM-dd HH:mm:ss}-{1:yyyy-MM-dd HH:mm:ss}", item.CreateTime.AddDays(-item.SettlementCycle), item.CreateTime)
			});

			return Json(new
			{
				rows = models,
				data.Total
			}, true);
		}

		public ActionResult ExportSettlementList(DateTime? startDate, DateTime? endDate)
		{
			if (startDate == null)
				startDate = endDate.HasValue ? endDate.Value.AddYears(-1) : DateTime.Now.AddYears(-1);

			var query = new ShopAccountItemQuery();
			query.TimeStart = startDate;
			query.TimeEnd = endDate;
			query.ShopAccountType = ShopAccountType.SettlementIncome;
			query.ShopId = CurrentSellerManager.ShopId;

			var data = BillingApplication.GetShopAccountItemNoPage(query);

			return ExcelView("已结算列表", data);
		}

        public ActionResult SettlementOrders(long? detailId)
        {
            var model = BillingApplication.GetShopSettlementStatistics(CurrentSellerManager.ShopId,detailId);
            return View(model);
        }

        public ActionResult ShopAccountItem()
        {
            return View();
        }

        /// <summary>
        /// 获取店铺帐户流水
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetShopAccountItemlist(DateTime? startDate, DateTime? endDate, ShopAccountType? type, int page, int rows)
        {
            ShopAccountItemQuery query = new ShopAccountItemQuery();
            query.PageNo = page;
            query.PageSize = rows;
            query.TimeStart = startDate;
            query.TimeEnd = endDate;
            query.ShopAccountType = type;
            query.ShopId = CurrentSellerManager.ShopId;
            var model = BillingApplication.GetShopAccountItem(query);

			var result = new
			{
				rows = model.Models.Select(p => new
				{
					p.Id,
					p.ShopId,
					p.ShopName,
					CreateTime=p.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
					p.AccountNo,
					p.AccountType,
					p.ShopAccountType,
					p.Balance,
					p.DetailId,
					p.DetailLink,
					p.IsIncome,
					p.Amount,
					p.Income,
					p.Expenditure,
					p.ReMark,
					p.AccoutID
				}),
				total = model.Total
			};

            return Json(result);
        }

        public ActionResult PendingSettlementDetail(long Id)
        {
            var model = BillingApplication.GetPendingOrderSettlementDetail(Id, CurrentSellerManager.ShopId);
            return View(model);
        }


        public ActionResult SettlementDetail(long Id)
        {
            var model = BillingApplication.GetOrderSettlementDetail(Id, CurrentSellerManager.ShopId);
            return View(model);
        }

        [HttpPost]
        public JsonResult PendingSettlementOrderList(DateTime? startDate, DateTime? endDate, long? orderId, int page, int rows)
        {
            var query = new PendingSettlementOrderQuery();
            query.OrderStart = startDate;
            query.OrderEnd = endDate;
            query.PageNo = page;
            query.PageSize = rows;
            query.OrderId = orderId;
            query.ShopId = CurrentSellerManager.ShopId;
            var model = BillingApplication.GetPendingSettlementOrders(query);
            var result = new { rows = model.Models, total = model.Total };
            return Json(result);
        }

        [HttpPost]
        public JsonResult SettlementOrderList(DateTime? startDate, DateTime? endDate, DateTime? BillingstartDate, DateTime? BillingendDate, long? orderId,long?detailId,int page, int rows)
        {
            var query = new SettlementOrderQuery();
            query.OrderStart = startDate;
            query.OrderEnd = endDate;
            query.SettleStart = BillingstartDate;
            query.SettleEnd = BillingendDate;
            query.WeekSettlementId = detailId;
            query.PageNo = page;
            query.PageSize = rows;
            query.OrderId = orderId;
            query.ShopId = CurrentSellerManager.ShopId;
            var model = BillingApplication.GetSettlementOrders(query);
            var result = new { rows = model.Models, total = model.Total };
            return Json(result);
        }

        public ActionResult MarketServiceRecordInfo(long Id)
        {
            var shopId = CurrentSellerManager.ShopId;
            var model = BillingApplication.GetMarketServiceRecord(Id, shopId);
            return View(model);
        }

		/// <summary>
		/// 导出未结算订单
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="orderId"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public ActionResult ExportUnSettlementOrder(DateTime? startDate, DateTime? endDate, long? orderId)
		{
			var query = new PendingSettlementOrderQuery();
			query.OrderStart = startDate;
			query.OrderEnd = endDate;
			query.OrderId = orderId;
			query.ShopId = CurrentSellerManager.ShopId;
			var models = BillingApplication.GetPendingSettlementOrdersNoPage(query);
			return ExcelView("待结算订单", models);
		}

		/// <summary>
		/// 导出已结算订单
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="orderId"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public ActionResult ExportSettlementOrder(DateTime? startDate, DateTime? endDate,DateTime? BillingstartDate, DateTime? BillingendDate, long? orderId,long? detailId)
		{
			var query = new SettlementOrderQuery();
			query.OrderStart = startDate;
			query.OrderEnd = endDate;
			query.SettleStart = BillingstartDate;
			query.SettleEnd = BillingendDate;
			query.OrderId = orderId;
			query.ShopId = CurrentSellerManager.ShopId;
			query.WeekSettlementId = detailId;
			var models = BillingApplication.GetSettlementOrdersNoPage(query);

			return ExcelView("已结算订单", models);
		}

		/// <summary>
		/// 获取店铺帐户流水
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public ActionResult ExportShopAccountItemlist(DateTime? startDate, DateTime? endDate, ShopAccountType? type)
		{
			ShopAccountItemQuery query = new ShopAccountItemQuery();
			query.TimeStart = startDate;
			query.TimeEnd = endDate;
			query.ShopAccountType = type;
			query.ShopId = CurrentSellerManager.ShopId;
			var models = BillingApplication.GetShopAccountItemNoPage(query);
			return ExcelView("收支明细", models);
		}
    }
}