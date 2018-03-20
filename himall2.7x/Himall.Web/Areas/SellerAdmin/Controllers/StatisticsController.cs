using Himall.Application;
using Himall.CommonModel;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class StatisticsController : BaseSellerController
    {
        private IStatisticsService _iStatisticsService;
        public StatisticsController(IStatisticsService iStatisticsService)
        {
            _iStatisticsService = iStatisticsService;
        }
        protected string RenderPartialViewToString(Controller controller, string partialViewName)
        {
            IView view = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialViewName).View;
            using (StringWriter writer = new StringWriter())
            {
                ViewContext viewContext = new ViewContext(controller.ControllerContext, view, controller.ViewData, controller.TempData, writer);
                viewContext.View.Render(viewContext, writer);
                return writer.ToString();
            }
        }
        #region 店铺总流量
        // GET: Admin/Chart
        public ActionResult ShopFlow()
        {

            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            var data = _iStatisticsService.GetShopFlowChart(CurrentSellerManager.ShopId, DateTime.Now.Year, DateTime.Now.Month);


            return View(new ChartDataViewModel(data));
        }

        private List<SelectListItem> GetYearDrop(int start, int end)
        {
            List<SelectListItem> yearDrop = new List<SelectListItem>();
            for (int i = start; i < end; i++)
            {
                yearDrop.Add(new SelectListItem { Selected = (DateTime.Now.Year == i), Text = i.ToString(), Value = i.ToString() });
            }

            return yearDrop;

        }

        private List<SelectListItem> GetMonthDrop()
        {
            List<SelectListItem> monthDrop = new List<SelectListItem>();
            for (int i = 1; i < 13; i++)
            {
                monthDrop.Add(new SelectListItem { Selected = (DateTime.Now.Month == i), Text = i.ToString(), Value = i.ToString() });
            }

            return monthDrop;

        }

        private List<SelectListItem> GetWeekDrop(int year, int month)
        {
            List<SelectListItem> weekDrop = new List<SelectListItem>();
            var start = DateTimeHelper.GetStartDayOfWeeks(year, month, 1);
            for (int i = 1; i <= 4; i++)
            {
                weekDrop.Add(new SelectListItem
                {
                    Selected = i == 1,
                    Text = string.Format("{0} -- {1}",
                        start.ToString("yyyy-MM-dd"), start.AddDays(6).ToString("yyyy-MM-dd")),
                    Value = i.ToString()
                });
                start = start.AddDays(7);
            }
            return weekDrop;
        }

        [UnAuthorize]
        [HttpGet]
        public JsonResult GetShopFlowChartByMonth(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = _iStatisticsService.GetShopFlowChart(CurrentSellerManager.ShopId, year, month);
            return Json(new { successful = true, chart = data }, JsonRequestBehavior.AllowGet);
        }



        #endregion

        #region 商品流量排行
        [UnAuthorize]
        [HttpGet]
        public ActionResult ProductVisitRanking()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            ViewBag.WeekDrop = GetWeekDrop(DateTime.Now.Year, DateTime.Now.Month);
            return View();
        }
        [UnAuthorize]
        [HttpGet]
        public JsonResult ProductVisitRankingChart(string day = "", int year = 0, int month = 0, int weekIndex = 0)
        {
            Model.LineChartDataModel<int> model = new Model.LineChartDataModel<int>();
            DateTime start;

            if (!string.IsNullOrWhiteSpace(day))
            {
                if (!DateTime.TryParse(day, out start))
                {
                    start = DateTime.Now;
                }
                model = _iStatisticsService.GetProductVisitRankingChart(CurrentSellerManager.ShopId, start);
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                if (weekIndex == 0)
                    model = _iStatisticsService.GetProductVisitRankingChart(CurrentSellerManager.ShopId, year, month);
                else
                    model = _iStatisticsService.GetProductVisitRankingChart(CurrentSellerManager.ShopId, year, month, weekIndex, 15);
            }
            return Json(new { successful = true, chart = model }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region 商品销售排行

        [HttpGet]
        [UnAuthorize]
        public ActionResult ProductSaleRanking()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            ViewBag.WeekDrop = GetWeekDrop(DateTime.Now.Year, DateTime.Now.Month);
            return View();
        }


        [HttpGet]
        [UnAuthorize]
        public JsonResult GetSaleRankingChart(string day = "", int year = 0, int month = 0, int weekIndex = 0, int dimension = 1)
        {
            Model.LineChartDataModel<int> model = new Model.LineChartDataModel<int>();
            DateTime start;

            if (!string.IsNullOrWhiteSpace(day))
            {
                if (!DateTime.TryParse(day, out start))
                {
                    start = DateTime.Now;
                }
                model = _iStatisticsService.GetProductSaleRankingChart(CurrentSellerManager.ShopId, start, (SaleDimension)dimension);
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                if (weekIndex == 0)
                    model = _iStatisticsService
                        .GetProductSaleRankingChart(CurrentSellerManager.ShopId, year, month, (SaleDimension)dimension);
                else
                    model = _iStatisticsService
                        .GetProductSaleRankingChart(CurrentSellerManager.ShopId, year, month, weekIndex, (SaleDimension)dimension);
            }
            return Json(new { successful = true, chart = model }, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region 店铺总销量

        public ActionResult ShopSale()
        {

            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            var data = _iStatisticsService.GetShopSaleChart(CurrentSellerManager.ShopId, DateTime.Now.Year, DateTime.Now.Month);


            return View(new ChartDataViewModel(data));
        }


        [UnAuthorize]
        [HttpGet]
        public JsonResult GetShopSaleChartByMonth(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = _iStatisticsService.GetShopSaleChart(CurrentSellerManager.ShopId, year, month);
            return Json(new { successful = true, chart = data }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region 成交转化率


        public ActionResult DealConversionRate()
        {

            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            var data = _iStatisticsService.GetDealConversionRateChart(CurrentSellerManager.ShopId, DateTime.Now.Year, DateTime.Now.Month);


            return View(new ChartDataViewModel(data));
        }

        [HttpGet]
        [UnAuthorize]
        public JsonResult GetDealConversionRateChartByMonth(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = _iStatisticsService.GetDealConversionRateChart(CurrentSellerManager.ShopId, year, month);
            return Json(new { successful = true, chart = data }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        [UnAuthorize]
        public JsonResult GetWeekList(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = GetWeekDrop(year, month);
            return Json(new { successful = true, week = data }, JsonRequestBehavior.AllowGet);
        }

        #region 商品统计
        public ActionResult ProductSaleStatistic()
        {
            return View();
        }
        public JsonResult GetProductSaleStatisticList(int page, int rows, DateTime startDate, DateTime endDate, string Sort = "", bool IsAsc = false)
        {
            ProductStatisticQuery query = new ProductStatisticQuery
            {
                PageSize = rows,
                PageNo = page,
                StartDate = startDate,
                EndDate = endDate,
                ShopId = CurrentSellerManager.ShopId,
                Sort = Sort,
                IsAsc = IsAsc
            };
            var model = StatisticApplication.GetProductSales(query);
            DataGridModel<ProductStatisticModel> dataGrid = new DataGridModel<ProductStatisticModel>() { rows = model.Models, total = model.Total };
            return Json(dataGrid);
        }

        public ActionResult ProductSaleCategoryStatistic()
        {
            var startDate = DateTime.Now.Date.AddDays(-1);
            var endDate = DateTime.Now.Date.AddDays(-1);
            var productCateSales = StatisticApplication.GetProductCategorySales(startDate, endDate, CurrentSellerManager.ShopId);
            return View(productCateSales);
        }

        public JsonResult GetProductSaleCategoryStatistic(DateTime startDate, DateTime endDate)
        {
            var productCateSales = StatisticApplication.GetProductCategorySales(startDate, endDate, CurrentSellerManager.ShopId);
            return Json(new { success = true, model = productCateSales }, JsonRequestBehavior.AllowGet);
        }
        #endregion 商品统计

        #region 交易统计
        public ActionResult TradeStatistic()
        {
            var startDate = DateTime.Now.Date.AddDays(-1);
            var endDate = startDate;//默认为一天
            var platTradeStatistic = StatisticApplication.GetShopTradeStatistic(startDate, endDate, CurrentSellerManager.ShopId);
            return View(platTradeStatistic);
        }
        public JsonResult GetPlatTradeStatistic(DateTime startDate, DateTime endDate)
        {
            var platTradeStatistic = StatisticApplication.GetShopTradeStatistic(startDate, endDate, CurrentSellerManager.ShopId);
            return Json(new { success = true, model = platTradeStatistic }, JsonRequestBehavior.AllowGet);
        }
        #endregion 交易统计

        #region 导出
        public ActionResult ExportTradeStatistic(DateTime startDate, DateTime endDate)
        {
            var tradeStatistic = StatisticApplication.GetShopTradeStatisticOnDay(startDate, endDate, CurrentSellerManager.ShopId);
            ViewData.Model = tradeStatistic;
            string Title = startDate.ToString("yyyy-MM-dd") + "至" + endDate.ToString("yyyy-MM-dd") + "交易统计数据";
            ViewData.Add("Title", Title);
            string viewHtml = RenderPartialViewToString(this, "ExportTradeStatistic");

            return File(System.Text.UTF8Encoding.Default.GetBytes(viewHtml), "application/ms-excel", "交易数据导出.xls");
        }

        public ActionResult ExportProductStatistic(DateTime startDate, DateTime endDate)
        {
            ProductStatisticQuery query = new ProductStatisticQuery
            {
                PageSize = int.MaxValue,
                PageNo = 1,
                StartDate = startDate,
                EndDate = endDate,
                ShopId = CurrentSellerManager.ShopId
            };
            var model = StatisticApplication.GetProductSales(query);

            ViewData.Model = model.Models;
            string Title = startDate.ToString("yyyy-MM-dd") + "至" + endDate.ToString("yyyy-MM-dd") + "商品统计数据";
            ViewData.Add("Title", Title);
            string viewHtml = RenderPartialViewToString(this, "ExportProductStatistic");

            return File(System.Text.UTF8Encoding.Default.GetBytes(viewHtml), "application/ms-excel", "商品销售情况.xls");
        }
        #endregion 
    }
}