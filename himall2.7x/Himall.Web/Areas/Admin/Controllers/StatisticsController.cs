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
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class StatisticsController : BaseAdminController
    {
        IStatisticsService _iStatisticsService;
        IRegionService _iRegionService;
        public StatisticsController(IStatisticsService iStatisticsService, IRegionService iRegionService)
        {
            _iStatisticsService = iStatisticsService;
            _iRegionService = iRegionService;
        }
        #region 会员
        // GET: Admin/Chart
        [UnAuthorize]
        public ActionResult Member()
        {

            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            var data = _iStatisticsService.GetMemberChart(DateTime.Now.Year, DateTime.Now.Month);


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
        public JsonResult GetMemberChartByMonth(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = _iStatisticsService.GetMemberChart(year, month);
            return Json(new { successful = true, chart = data }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ExportMemberByMonth(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = _iStatisticsService.GetMemberChart(year, month);
            ViewData.Model = data;
            ViewData.Add("Title", year + "年" + month + "月");
            string viewHtml = RenderPartialViewToString(this, "ExportMemberByMonth");
            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", string.Format("月份_{0}_{1}.xls", data.SeriesData[0].Name, DateTime.Now.ToString("yyyy-MM-dd")));
        }


        [NonAction]
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

        #endregion

        #region 区域地图
        [UnAuthorize]
        [HttpGet]
        public JsonResult GetAreaMapBySearch(int dimension, int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = StatisticApplication.GetAreaOrderChart((OrderDimension)dimension, year, month);

            return Json(new { successful = true, chart = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportAreaMap(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var OrderMemberCountdata = _iStatisticsService.GetAreaOrderChart(OrderDimension.OrderCount
                , year, month);
            var OrderMoneydata = _iStatisticsService.GetAreaOrderChart(OrderDimension.OrderMoney
                , year, month);

            AreaMapExportModel result = new AreaMapExportModel();

            result.MaxMemberCount = OrderMemberCountdata.RangeMax;
            result.MinMemberCount = OrderMemberCountdata.RangeMin;
            result.MaxOrderMoney = OrderMoneydata.RangeMax;
            result.MinOrderMoney = OrderMoneydata.RangeMin;

            List<AreaMapExportSubModel> list = new List<AreaMapExportSubModel>();


            for (int i = 0; i < OrderMemberCountdata.Series.Data.Length; i++)
            {
                AreaMapExportSubModel subitem = new AreaMapExportSubModel();
                subitem.RegionID = int.Parse(OrderMemberCountdata.Series.Data[i].name);
                subitem.RegionName = _iRegionService.GetFullName(subitem.RegionID);
                subitem.MemberCount = (int)OrderMemberCountdata.Series.Data[i].value;
                subitem.OrderMoney = OrderMoneydata.Series.Data[i].value;
                list.Add(subitem);
            }

            result.Details = list;

            ViewData.Model = result;
            ViewData.Add("Title", year + "年" + month + "月");
            string viewHtml = RenderPartialViewToString(this, "ExportAreaMap");
            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", string.Format("区域统计_{0}.xls", DateTime.Now.ToString("yyyy-MM-dd")));
        }


        [UnAuthorize]
        public ActionResult OrderAreaMap()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            ViewBag.Year = DateTime.Now.Year;
            ViewBag.Month = DateTime.Now.Month;

            //
            return View();
        }

        #endregion

        #region 店铺
        [UnAuthorize]
        public ActionResult NewShop()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            var data = _iStatisticsService.GetNewsShopChart(DateTime.Now.Year, DateTime.Now.Month);
            return View(new ChartDataViewModel(data));
        }

        [UnAuthorize]
        [HttpGet]
        public JsonResult GetNewShopChartByMonth(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = _iStatisticsService.GetNewsShopChart(year, month);
            return Json(new { successful = true, chart = data }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        [HttpGet]
        public ActionResult ShopRanking()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            ViewBag.WeekDrop = GetWeekDrop(DateTime.Now.Year, DateTime.Now.Month);
            return View();
        }
        [UnAuthorize]
        public JsonResult GetWeekList(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = GetWeekDrop(year, month);
            return Json(new { successful = true, week = data }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        [HttpGet]
        public JsonResult GetShopRankingChart(string day = "", int year = 0, int month = 0, int weekIndex = 0, int dimension = 1)
        {
            Model.LineChartDataModel<int> model = new Model.LineChartDataModel<int>();
            DateTime start;

            if (!string.IsNullOrWhiteSpace(day))
            {
                if (!DateTime.TryParse(day, out start))
                {
                    start = DateTime.Now;
                }
                model = _iStatisticsService.GetShopRankingChart(start, (ShopDimension)dimension);
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                if (weekIndex == 0)
                    model = _iStatisticsService.GetShopRankingChart(year, month, (ShopDimension)dimension);
                else
                    model = _iStatisticsService.GetShopRankingChart(year, month, weekIndex, (ShopDimension)dimension);
            }
            return Json(new { successful = true, chart = model }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 销售排行

        [HttpGet]
        [UnAuthorize]
        public ActionResult ProductSaleRanking()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            ViewBag.WeekDrop = GetWeekDrop(DateTime.Now.Year, DateTime.Now.Month);
            return View();
        }

        [UnAuthorize]
        [HttpGet]
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
                model = _iStatisticsService.GetSaleRankingChart(start, (SaleDimension)dimension);
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                if (weekIndex == 0)
                    model = _iStatisticsService.GetSaleRankingChart(year, month, (SaleDimension)dimension);
                else
                    model = _iStatisticsService.GetSaleRankingChart(year, month, weekIndex, (SaleDimension)dimension);
            }
            return Json(new { successful = true, chart = model }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ExportSaleRanking(string day = "", int year = 0, int month = 0, int weekIndex = 0)
        {
            #region 获取查询结果
            List<SalesExportModel> result = new List<SalesExportModel>();
            Model.LineChartDataModel<int> SaleCountmodel = new Model.LineChartDataModel<int>();
            Model.LineChartDataModel<int> Salesmodel = new Model.LineChartDataModel<int>();
            string Title = "";
            DateTime start;

            if (!string.IsNullOrWhiteSpace(day))
            {
                if (!DateTime.TryParse(day, out start))
                {
                    start = DateTime.Now;
                }
                SaleCountmodel = _iStatisticsService.GetSaleRankingChart(start, SaleDimension.SaleCount);
                Salesmodel = _iStatisticsService.GetSaleRankingChart(start, SaleDimension.Sales);
                Title = "按天统计：" + day;
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                if (weekIndex == 0)
                {
                    SaleCountmodel = _iStatisticsService.GetSaleRankingChart(year, month, SaleDimension.SaleCount);
                    Salesmodel = _iStatisticsService.GetSaleRankingChart(year, month, SaleDimension.Sales);
                    Title = "按月统计：" + year + "年" + month + "月";
                }
                else
                {
                    SaleCountmodel = _iStatisticsService.GetSaleRankingChart(year, month, weekIndex, SaleDimension.SaleCount);
                    Salesmodel = _iStatisticsService.GetSaleRankingChart(year, month, weekIndex, SaleDimension.Sales);
                    Title = "按周统计：" + year + "年" + month + "月 第" + weekIndex + "周";
                }
            }

            for (int i = 0; i < SaleCountmodel.ExpandProp.Length; i++)
            {
                SalesExportModel model = new SalesExportModel();
                if (!string.IsNullOrEmpty(SaleCountmodel.ExpandProp[i]))
                {
                    model.ProductName = SaleCountmodel.ExpandProp[i];
                    model.SaleCount = SaleCountmodel.SeriesData[0].Data[i];
                    model.SaleAmount = Salesmodel.SeriesData[0].Data[i];
                    result.Add(model);
                }
            }
            #endregion

            #region 构建EXCEL
            ViewData.Model = result;
            //ViewBag.Title = Title;
            ViewData.Add("Title", Title);
            string viewHtml = RenderPartialViewToString(this, "ExportSaleRanking");

            return File(System.Text.UTF8Encoding.Default.GetBytes(viewHtml), "application/ms-excel", string.Format("销量分析_{0}.xls", DateTime.Now.ToString("yyyy-MM-dd")));
            #endregion
        }

        #endregion

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
            var endDate = startDate;//默认为一天
            var productCateSales = StatisticApplication.GetProductCategorySales(startDate, endDate);
            return View(productCateSales);
        }

        public JsonResult GetProductSaleCategoryStatistic(DateTime startDate, DateTime endDate)
        {
            var productCateSales = StatisticApplication.GetProductCategorySales(startDate, endDate);

            return Json(new { success = true, model = productCateSales }, JsonRequestBehavior.AllowGet);
        }
        #endregion 商品统计

        #region 交易统计
        public ActionResult TradeStatistic()
        {
            var startDate = DateTime.Now.Date.AddDays(-1);
            var endDate = startDate;//默认为一天
            var platTradeStatistic = StatisticApplication.GetPlatTradeStatistic(startDate, endDate);
            return View(platTradeStatistic);
        }
        public JsonResult GetPlatTradeStatistic(DateTime startDate, DateTime endDate)
        {
            var platTradeStatistic = StatisticApplication.GetPlatTradeStatistic(startDate, endDate);
            return Json(new { success = true, model = platTradeStatistic }, JsonRequestBehavior.AllowGet);
        }
        #endregion 交易统计

        #region 导出
        public ActionResult ExportTradeStatistic(DateTime startDate, DateTime endDate)
        {
            var platTradeStatistic = StatisticApplication.GetPlatTradeStatisticOnDay(startDate, endDate);
            ViewData.Model = platTradeStatistic;
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
                EndDate = endDate
            };
            var model = StatisticApplication.GetProductSales(query);

            ViewData.Model = model.Models;
            string Title = startDate.ToString("yyyy-MM-dd") + "至" + endDate.ToString("yyyy-MM-dd") + "商品统计数据";
            ViewData.Add("Title", Title);
            string viewHtml = RenderPartialViewToString(this, "ExportProductStatistic");
            return File(System.Text.UTF8Encoding.Default.GetBytes(viewHtml), "application/ms-excel", "商品销售情况.xls");

        }
        #endregion 导出
    }
}