
using System;
using System.Collections.Generic;
using System.Linq;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System.Data.Entity.Validation;
using Himall.Core.Helper;
using System.Text;
using Himall.CommonModel;
using MySql.Data.MySqlClient;

namespace Himall.Service
{
    public class StatisticsService : ServiceBase, IStatisticsService
    {

        #region 平台

        #region 会员折线图


        private void ChechData(int year, int month)
        {
            if (year < 1600 || year > 9999)
            {
                throw new Exception("非法的年份");
            }
            if (month < 0 || month > 12)
            {
                throw new Exception("非法的月份");
            }
        }

        public LineChartDataModel<int> GetMemberChart(int year, int month, int weekIndex)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[] { "星期一", "星期二", "星期三", "星期四", "星期五", "星期六", "星期天" },
                SeriesData = new List<ChartSeries<int>>()
            };
            var start = DateTimeHelper.GetStartDayOfWeeks(year, month, weekIndex);
            if (DateTime.MinValue.Equals(start))
                throw new ArgumentException("参数错误");

            var end = start.AddDays(6);
            var data = (from m in Context.UserMemberInfo
                        orderby m.CreateDate
                        where m.CreateDate >= start
                        && m.CreateDate <= end
                        group m by new { m.CreateDate.Year, m.CreateDate.Month, m.CreateDate.Day } into G
                        select new
                        {
                            G.Key,
                            Count = G.Count()
                        }).ToList();

            var chartItem = new ChartSeries<int> { Name = "新增会员", Data = new int[7] };
            for (int i = 0; i < 7; i++)
            {
                var date = start.AddDays(i);
                if (data.Any(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day))
                    chartItem.Data[i] = data.FirstOrDefault(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day).Count;
            }
            chart.SeriesData.Add(chartItem);

            return chart;
        }

        //TODO 新增会员排行//
        public LineChartDataModel<int> GetMemberChart(int year, int month)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>() { SeriesData = new List<ChartSeries<int>>() };
            ChechData(year, month);
            DateTime thisMonth = new DateTime(year, month, 1);
            DateTime thisMonthEnd = thisMonth.AddMonths(1);

            if (month == 1)
            {
                month = 12;
                year -= 1;
            }
            else
            {
                month--;
            }
            DateTime prevMonth = new DateTime(year, month, 1);
            DateTime prevMonthEnd = prevMonth.AddMonths(1);


            TimeSpan tt = thisMonth.AddMonths(1) - thisMonth;
            chart.XAxisData = GenerateStringByDays(31);

            #region MS SQL

            //var thisMonthData = (from m in context.UserMemberInfo
            //                    orderby m.CreateDate
            //                    where m.CreateDate >= thisMonth
            //                    && m.CreateDate < thisMonthEnd
            //                    group m by new { m.CreateDate.Year, m.CreateDate.Month, m.CreateDate.Day } into G
            //                    select new
            //                    {
            //                        G.Key,
            //                        Count = G.Count()
            //                    }).ToList();
            //var chartItem = new ChartSeries<int> { Name = string.Format("{0}月新增会员", thisMonth.Month), Data = new int[31] };
            //for (int i = 0; i < 31; i++)
            //{
            //    var date = thisMonth.AddDays(i);
            //    if (thisMonthData.Any(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day))
            //        chartItem.Data[i] = thisMonthData.FirstOrDefault(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day).Count;
            //}
            //chart.SeriesData.Add(chartItem);

            ////上个月的数据集 
            //var prevMonthData = (from m in context.UserMemberInfo
            //                    orderby m.CreateDate
            //                    where m.CreateDate >= prevMonth
            //                    && m.CreateDate < prevMonthEnd
            //                    group m by new { m.CreateDate.Year, m.CreateDate.Month, m.CreateDate.Day } into G
            //                    select new
            //                    {
            //                        G.Key,
            //                        Count = G.Count()
            //                    }).ToList();
            //chartItem = new ChartSeries<int> { Name = string.Format("{0}月新增会员", prevMonth.Month), Data = new int[31] };
            //for (int i = 0; i < 31; i++)
            //{
            //    var date = prevMonth.AddDays(i);
            //    if (prevMonthData.Any(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day))
            //        chartItem.Data[i] = prevMonthData.FirstOrDefault(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day).Count;
            //}
            //chart.SeriesData.Add(chartItem);

            #endregion 

            #region MySQL

            var thisMonthData = Context.UserMemberInfo.Where(m => m.CreateDate >= thisMonth && m.CreateDate < thisMonthEnd)
               .Select(m => new { m.CreateDate.Year, m.CreateDate.Month, m.CreateDate.Day })
               .GroupBy(m => new { m.Year, m.Month, m.Day })
               .Select(g => new { Count = g.Count(), g.Key.Year, g.Key.Month, g.Key.Day }).ToList();


            var chartItem = new ChartSeries<int> { Name = string.Format("{0}月新增会员", thisMonth.Month), Data = new int[31] };
            for (int i = 0; i < 31; i++)
            {
                var date = thisMonth.AddDays(i);
                if (thisMonthData.Any(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day))
                    chartItem.Data[i] = thisMonthData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day).Count;
            }
            chart.SeriesData.Add(chartItem);

            //上个月的数据集 
            var prevMonthData = Context.UserMemberInfo.Where(m => m.CreateDate >= prevMonth && m.CreateDate < prevMonthEnd)
               .Select(m => new { m.CreateDate.Year, m.CreateDate.Month, m.CreateDate.Day })
               .GroupBy(m => new { m.Year, m.Month, m.Day })
               .Select(g => new { Count = g.Count(), g.Key.Year, g.Key.Month, g.Key.Day }).ToList();

            chartItem = new ChartSeries<int> { Name = string.Format("{0}月新增会员", prevMonth.Month), Data = new int[31] };
            for (int i = 0; i < 31; i++)
            {
                var date = prevMonth.AddDays(i);
                if (prevMonthData.Any(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day))
                    chartItem.Data[i] = prevMonthData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day).Count;
            }
            chart.SeriesData.Add(chartItem);

            #endregion


            return chart;


        }

        private string[] GenerateStringByDays(int days)
        {
            string[] arr = new string[days];
            for (int i = 1; i <= days; i++)
            {
                arr[i - 1] = string.Format("{0}", i);
            }
            return arr;
        }

        public LineChartDataModel<int> GetMemberChart(DateTime day)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 区域分析地图

        /// <summary>
        /// 初始化下单客户数图表
        /// </summary>
        /// <param name="model"></param>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        private void InitialOrderMemberCount(MapChartDataModel model, int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);
            var data = (from t in
                           (from m in Context.UserMemberInfo
                            join o in Context.OrderInfo on m.Id equals o.UserId
                            orderby o.OrderDate ascending
                            where o.OrderDate >= start && o.OrderDate < end
                            select new
                            {
                                UserId = m.Id,
                                TopRegionId = m.TopRegionId
                            }).Distinct()

                        group t by new { t.TopRegionId } into G
                        select new
                        {

                            Name = G.Key.TopRegionId,
                            Count = G.Count()
                        }).ToList();
            if (data != null && data.Count() > 0)
            {
                model.RangeMin = data.Min(t => t.Count);
                model.RangeMax = data.Max(t => t.Count);
                if (model.RangeMax == model.RangeMin)
                {
                    model.RangeMin = 0;
                }

                model.Series = new MapChartSeries() { Name = "下单客户数", Data = new MapChartSeriesData[data.Count()] };
                int index = 0;
                foreach (var item in data.ToList())
                {
                    model.Series.Data[index] = new MapChartSeriesData();
                    model.Series.Data[index].name = item.Name.ToString();
                    model.Series.Data[index++].value = item.Count;
                }
            }
        }

        /// <summary>
        /// 初始化下单量图表
        /// </summary>
        /// <param name="model"></param>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        private void InitialOrderCount(MapChartDataModel model, int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);
            var data = (from t in
                           (from o in Context.OrderInfo
                            orderby o.OrderDate ascending
                            where o.OrderDate >= start && o.OrderDate < end
                            select new
                            {
                                TopRegionId = o.TopRegionId
                            })
                        group t by t.TopRegionId into G
                        select new
                        {
                            Name = G.Key,
                            Count = G.Count()
                        }).ToList();
            if (data != null && data.Count() > 0)
            {
                model.RangeMin = data.Min(t => t.Count);
                model.RangeMax = data.Max(t => t.Count);

                model.Series = new MapChartSeries() { Name = "下单量", Data = new MapChartSeriesData[data.Count()] };
                int index = 0;
                foreach (var item in data.ToList())
                {
                    model.Series.Data[index] = new MapChartSeriesData();
                    model.Series.Data[index].name = item.Name.ToString();
                    model.Series.Data[index++].value = item.Count;
                }
            }
        }

        /// <summary>
        /// 初始化下单金额图表
        /// </summary>
        /// <param name="model"></param>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        private void InitialOrderMoney(MapChartDataModel model, int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            var x = from i in Context.OrderInfo
                    select i.OrderTotalAmount;

            var data = (from t in
                            (from o in Context.OrderInfo
                             orderby o.OrderDate ascending
                             where o.OrderDate >= start && o.OrderDate < end
                             select new
                             {
                                 TopRegionId = o.TopRegionId,
                                 OrderTotalMoney = o.ProductTotalAmount + o.Freight + o.Tax
                             })
                        group t by t.TopRegionId into G
                        select new
                        {
                            Name = G.Key,
                            Money = G.Sum(g => g.OrderTotalMoney)
                        }).ToList();
            if (data != null && data.Count() > 0)
            {
                model.RangeMin = (int)data.Min(t => t.Money);
                model.RangeMax = (int)data.Max(t => t.Money);

                model.Series = new MapChartSeries() { Name = "下单金额", Data = new MapChartSeriesData[data.Count()] };
                int index = 0;
                foreach (var item in data.ToList())
                {
                    model.Series.Data[index] = new MapChartSeriesData();
                    model.Series.Data[index].name = item.Name.ToString();
                    model.Series.Data[index++].value = item.Money;
                }
            }
        }
        public MapChartDataModel GetAreaOrderChart(Model.OrderDimension dimension, int year, int month)
        {

            ChechData(year, month);

            MapChartDataModel model = new MapChartDataModel();
            switch (dimension)
            {
                case Model.OrderDimension.OrderCount: InitialOrderCount(model, year, month); break;
                case Model.OrderDimension.OrderMemberCount: InitialOrderMemberCount(model, year, month); break;
                case Model.OrderDimension.OrderMoney: InitialOrderMoney(model, year, month); break;
            }
            return model;

        }

        #endregion

        #region 店铺统计图表

        /// <summary>
        /// 初始化按照订单量维度获取前N店铺排行
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="rankSize"></param>
        private void InitialShopChartByOrderCount(DateTime start, DateTime end, int rankSize, LineChartDataModel<int> model)
        {
            int index = 0;
            var data = (from t in
                            (from o in Context.OrderInfo
                             join s in Context.ShopInfo on o.ShopId equals s.Id
                             where o.OrderDate >= start && o.OrderDate < end
                             select new
                             {
                                 OrderDate = o.OrderDate,
                                 ShopName = s.ShopName,

                             }).ToList()
                        orderby t.OrderDate
                        group t by new { t.ShopName } into G
                        select new
                        {
                            ShopName = G.FirstOrDefault().ShopName,
                            Count = G.Count()
                        }).OrderByDescending(x => x.Count).ToList();

            if (null != data && 0 < data.Count())
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "店铺订单量排行Top" + rankSize.ToString() };
                foreach (var item in data.Take(rankSize))
                {
                    series.Data[index] = item.Count;
                    model.ExpandProp[index++] = item.ShopName;
                }
                model.SeriesData.Add(series);
            }
            else
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "店铺订单量排行Top" + rankSize.ToString() };
                for (int i = 0; i < rankSize; i++)
                {

                    series.Data[index] = 0;
                    model.ExpandProp[index++] = "";
                }
                model.SeriesData.Add(series);
            }

        }

        /// <summary>
        /// 初始化按照销售额维度获取前N店铺排行
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="rankSize"></param>
        private void InitialShopChartBySales(DateTime start, DateTime end, int rankSize, LineChartDataModel<int> model)
        {
            int index = 0;
            var data = (from t in
                            (from sv in Context.ShopVistiInfo
                             join s in Context.ShopInfo on sv.ShopId equals s.Id
                             where sv.Date >= start && sv.Date < end
                             select new
                             {
                                 Date = sv.Date,
                                 ShopName = s.ShopName,
                                 OrderMoney = sv.SaleAmounts

                             }).ToList()
                        orderby t.Date
                        group t by new { t.ShopName } into G
                        select new
                        {
                            ShopName = G.FirstOrDefault().ShopName,
                            Money = G.Sum(g => g.OrderMoney)
                        }).OrderByDescending(x => x.Money).ToList();

            if (null != data && 0 < data.Count())
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "店铺销售额排行Top" + rankSize.ToString() };
                foreach (var item in data.Take(rankSize))
                {
                    series.Data[index] = (int)item.Money;
                    model.ExpandProp[index++] = item.ShopName;
                }
                model.SeriesData.Add(series);
            }
            else
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "店铺销售额排行Top" + rankSize.ToString() };
                for (int i = 0; i < rankSize; i++)
                {

                    series.Data[index] = 0;
                    model.ExpandProp[index++] = "";
                }
                model.SeriesData.Add(series);
            }
        }

        public LineChartDataModel<int> GetNewsShopChart(int year, int month)
        {

            LineChartDataModel<int> chart = new LineChartDataModel<int>() { SeriesData = new List<ChartSeries<int>>() };
            ChechData(year, month);
            DateTime thisMonth = new DateTime(year, month, 1);
            DateTime thisMonthEnd = thisMonth.AddMonths(1);

            if (month == 1)
            {
                month = 12;
                year -= 1;
            }
            else
            {
                month--;
            }
            DateTime prevMonth = new DateTime(year, month, 1);
            DateTime prevMonthEnd = prevMonth.AddMonths(1);


            TimeSpan tt = thisMonth.AddMonths(1) - thisMonth;
            chart.XAxisData = GenerateStringByDays(31);

            #region MS SQL
            //var thisMonthData = (from m in context.ShopInfo
            //                    orderby m.CreateDate
            //                    where m.CreateDate >= thisMonth
            //                    && m.CreateDate < thisMonthEnd
            //                    && m.Stage.Value == ShopInfo.ShopStage.Finish
            //                    group m by new { m.CreateDate.Year, m.CreateDate.Month, m.CreateDate.Day } into G
            //                    select new
            //                    {
            //                        G.Key,
            //                        Count = G.Count()
            //                    }).ToList();
            //var chartItem = new ChartSeries<int> { Name = string.Format("{0}月新增店铺", thisMonth.Month), Data = new int[31] };
            //for (int i = 0; i < 31; i++)
            //{
            //    var date = thisMonth.AddDays(i);
            //    if (thisMonthData.Any(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day))
            //        chartItem.Data[i] = thisMonthData.FirstOrDefault(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day).Count;
            //}
            //chart.SeriesData.Add(chartItem);

            ////上个月的数据集 
            //var prevMonthData = (from m in context.ShopInfo
            //                    orderby m.CreateDate
            //                    where m.CreateDate >= prevMonth
            //                    && m.CreateDate < prevMonthEnd
            //                    && m.Stage.Value == ShopInfo.ShopStage.Finish
            //                    group m by new { m.CreateDate.Year, m.CreateDate.Month, m.CreateDate.Day } into G
            //                    select new
            //                    {
            //                        G.Key,
            //                        Count = G.Count()
            //                    }).ToList();
            //chartItem = new ChartSeries<int> { Name = string.Format("{0}月新增店铺", prevMonth.Month), Data = new int[31] };
            //for (int i = 0; i < 31; i++)
            //{
            //    var date = prevMonth.AddDays(i);
            //    if (prevMonthData.Any(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day))
            //        chartItem.Data[i] = prevMonthData.FirstOrDefault(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day).Count;
            //}
            //chart.SeriesData.Add(chartItem);

            #endregion 

            #region MySQL

            var thisMonthData = Context.ShopInfo.Where(m => m.CreateDate >= thisMonth && m.CreateDate < thisMonthEnd && m.Stage.Value == ShopInfo.ShopStage.Finish)
                .Select(m => new { m.CreateDate.Year, m.CreateDate.Month, m.CreateDate.Day })
                .GroupBy(m => new { m.Year, m.Month, m.Day })
                .Select(g => new { Count = g.Count(), g.Key.Year, g.Key.Month, g.Key.Day }).ToList();

            var chartItem = new ChartSeries<int> { Name = string.Format("{0}月新增店铺", thisMonth.Month), Data = new int[31] };
            for (int i = 0; i < 31; i++)
            {
                var date = thisMonth.AddDays(i);
                if (thisMonthData.Any(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day))
                    chartItem.Data[i] = thisMonthData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day).Count;
            }
            chart.SeriesData.Add(chartItem);


            var prevMonthData = Context.ShopInfo.Where(m => m.CreateDate >= prevMonth && m.CreateDate < prevMonthEnd && m.Stage.Value == ShopInfo.ShopStage.Finish)
                .Select(m => new { m.CreateDate.Year, m.CreateDate.Month, m.CreateDate.Day })
                .GroupBy(m => new { m.Year, m.Month, m.Day })
                .Select(g => new { Count = g.Count(), g.Key.Year, g.Key.Month, g.Key.Day }).ToList();

            chartItem = new ChartSeries<int> { Name = string.Format("{0}月新增店铺", prevMonth.Month), Data = new int[31] };
            for (int i = 0; i < 31; i++)
            {
                var date = prevMonth.AddDays(i);
                if (prevMonthData.Any(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day))
                    chartItem.Data[i] = prevMonthData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day).Count;
            }
            chart.SeriesData.Add(chartItem);
            #endregion


            return chart;

        }


        public LineChartDataModel<int> GetShopRankingChart(int year, int month, ShopDimension dimension = ShopDimension.OrderCount, int rankSize = 15)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[rankSize],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[rankSize]
            };

            for (int i = 1; i <= rankSize; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            if (dimension == ShopDimension.OrderCount)
                InitialShopChartByOrderCount(start, end, 15, chart);
            else
                InitialShopChartBySales(start, end, 15, chart);
            return chart;
        }

        public LineChartDataModel<int> GetShopRankingChart(int year, int month, int weekIndex, ShopDimension dimension = ShopDimension.OrderCount, int rankSize = 15)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[rankSize],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[rankSize]
            };

            for (int i = 1; i <= rankSize; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            var start = DateTimeHelper.GetStartDayOfWeeks(year, month, weekIndex);
            if (DateTime.MinValue.Equals(start))
                throw new ArgumentException("参数错误");

            var end = start.AddDays(6);

            if (dimension == ShopDimension.OrderCount)
                InitialShopChartByOrderCount(start, end, 15, chart);
            else
                InitialShopChartBySales(start, end, 15, chart);

            return chart;
        }

        public LineChartDataModel<int> GetShopRankingChart(DateTime day, ShopDimension dimension = ShopDimension.OrderCount, int rankSize = 15)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[rankSize],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[rankSize]
            };

            for (int i = 1; i <= rankSize; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            var start = day;
            var end = start.AddHours(24);

            if (dimension == ShopDimension.OrderCount)
                InitialShopChartByOrderCount(start, end, 15, chart);
            else
                InitialShopChartBySales(start, end, 15, chart);

            return chart;
        }

        public LineChartDataModel<int> GetRecentMonthShopSaleRankChart()
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[15],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[15]
            };

            for (int i = 1; i <= 15; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }
            InitialShopChartByOrderCount(DateTime.Now.AddMonths(-1), DateTime.Now, 15, chart);
            return chart;
        }

        #endregion

        #region 销量排行

        /// <summary>
        /// 初始化按照销售量维度获取前N商品排行
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="rankSize"></param>
        private void InitialSaleChartBySaleCount(DateTime start, DateTime end, int rankSize, LineChartDataModel<int> model)
        {
            int index = 0;
            var data = (from t in
                            (from o in Context.OrderInfo
                             join oi in Context.OrderItemInfo on o.Id equals oi.OrderId
                             where o.OrderDate >= start && o.OrderDate < end && o.OrderStatus == OrderInfo.OrderOperateStatus.Finish

                             select new
                             {
                                 OrderDate = o.OrderDate,
                                 ProductId = oi.ProductId,
                                 ProductName = oi.ProductName,
                                 Quantity = oi.Quantity - oi.ReturnQuantity


                             }).ToList()
                        orderby t.OrderDate
                        group t by new { t.ProductId, t.ProductName } into G
                        select new
                        {
                            ProductName = G.FirstOrDefault().ProductName,
                            Count = G.Sum(t => t.Quantity)
                        }).OrderByDescending(x => x.Count).ToList();

            if (null != data && 0 < data.Count())
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "商品销售量排行Top" + rankSize.ToString() };
                foreach (var item in data.Take(rankSize))
                {
                    series.Data[index] = (int)item.Count;
                    model.ExpandProp[index++] = item.ProductName;
                }
                model.SeriesData.Add(series);
            }
            else
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "商品销售量排行Top" + rankSize.ToString() };
                for (int i = 0; i < rankSize; i++)
                {

                    series.Data[index] = 0;
                    model.ExpandProp[index++] = "";
                }
                model.SeriesData.Add(series);
            }

        }

        /// <summary>
        /// 近一个月平台销售排行
        /// </summary>
        /// <returns></returns>

        public LineChartDataModel<int> GetRecentMonthSaleRankChart()
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[15],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[15]
            };

            for (int i = 1; i <= 15; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            InitialSaleChartBySaleCount(DateTime.Now.AddMonths(-1), DateTime.Now, 15, chart);
            return chart;
        }


        /// <summary>
        /// 初始化按照销售额维度获取前N商品排行
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="rankSize"></param>
        private void InitialSaleChartBySales(DateTime start, DateTime end, int rankSize, LineChartDataModel<int> model)
        {
            int index = 0;
            //var data = (from t in
            //                (from pv in context.ProductVistiInfo
            //                 join p in context.ProductInfo on pv.ProductId equals p.Id
            //                 where pv.Date >= start && pv.Date < end
            //                 select new
            //                 {
            //                     Date = pv.Date,
            //                     ProductId = pv.ProductId,
            //                     ProductName = p.ProductName,
            //                     RealTotalPrice = pv.SaleAmounts


            //                 }).ToList()
            //            orderby t.Date
            //            group t by new { t.ProductId, t.ProductName } into G
            //            select new
            //            {
            //                ProductName = G.FirstOrDefault().ProductName,
            //                Money = G.Sum(money => money.RealTotalPrice)
            //            }).OrderByDescending(x => x.Money).ToList();

            var data = (from pm in
                            (from t in
                                 (from pv in Context.ProductVistiInfo
                                  join p in Context.ProductInfo on pv.ProductId equals p.Id
                                  where pv.Date >= start && pv.Date < end && p.IsDeleted == false
                                  select new
                                  {
                                      Date = pv.Date,
                                      ProductId = pv.ProductId,
                                      ProductName = p.ProductName,
                                      RealTotalPrice = pv.SaleAmounts
                                  }).ToList()
                             orderby t.Date
                             group t by new { t.ProductId, t.ProductName } into G
                             select new
                             {
                                 ProductId = G.FirstOrDefault().ProductId,
                                 ProductName = G.FirstOrDefault().ProductName,
                                 Money = G.Sum(t => t.RealTotalPrice)
                             })
                        join r in
                            (from rm in
                                 (from item in Context.OrderItemInfo
                                  join itemrefund in Context.OrderRefundInfo on item.Id equals itemrefund.OrderItemId
                                  where itemrefund.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed && itemrefund.ManagerConfirmDate >= start && itemrefund.ManagerConfirmDate < end
                                  select new
                                  {
                                      ProductId = item.ProductId,
                                      Amount = itemrefund.Amount
                                  }).ToList()
                             group rm by new { rm.ProductId } into R
                             select new
                             {
                                 ProductId = R.FirstOrDefault().ProductId,
                                 RefundMoney = R.Sum(money => money.Amount)
                             })
                        on pm.ProductId equals r.ProductId into ps
                        from pd in ps.DefaultIfEmpty()
                        select new
                        {
                            ProductName = pm.ProductName,
                            Money = (pm.Money - (pd == null ? 0 : pd.RefundMoney)) < 0 ? 0 : pm.Money - (pd == null ? 0 : pd.RefundMoney)
                        }).OrderByDescending(x => x.Money).ToList();

            if (null != data && 0 < data.Count())
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "商品销售额排行Top" + rankSize.ToString() };
                foreach (var item in data.Take(rankSize))
                {
                    series.Data[index] = (int)item.Money;
                    model.ExpandProp[index++] = item.ProductName;
                }
                model.SeriesData.Add(series);
            }
            else
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "商品销售额排行Top" + rankSize.ToString() };
                for (int i = 0; i < rankSize; i++)
                {

                    series.Data[index] = 0;
                    model.ExpandProp[index++] = "";
                }
                model.SeriesData.Add(series);
            }
        }


        public LineChartDataModel<int> GetSaleRankingChart(int year, int month, SaleDimension dimension = SaleDimension.SaleCount, int rankSize = 15)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[rankSize],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[rankSize]
            };

            for (int i = 1; i <= rankSize; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            if (dimension == SaleDimension.SaleCount)
                InitialSaleChartBySaleCount(start, end, 15, chart);
            else
                InitialSaleChartBySales(start, end, 15, chart);
            return chart;
        }

        public LineChartDataModel<int> GetSaleRankingChart(int year, int month, int weekIndex, SaleDimension dimension = SaleDimension.SaleCount, int rankSize = 15)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[rankSize],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[rankSize]
            };

            for (int i = 1; i <= rankSize; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            var start = DateTimeHelper.GetStartDayOfWeeks(year, month, weekIndex);
            if (DateTime.MinValue.Equals(start))
                throw new ArgumentException("参数错误");

            var end = start.AddDays(6);
            //应该是加上23：59：59
            end = end.Date.AddDays(1);
            if (dimension == SaleDimension.SaleCount)
                InitialSaleChartBySaleCount(start, end, 15, chart);
            else
                InitialSaleChartBySales(start, end, 15, chart);
            return chart;
        }

        public LineChartDataModel<int> GetSaleRankingChart(DateTime day, SaleDimension dimension = SaleDimension.SaleCount, int rankSize = 15)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[rankSize],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[rankSize]
            };

            for (int i = 1; i <= rankSize; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            var start = day;
            var end = start.AddHours(24);

            if (dimension == SaleDimension.SaleCount)
                InitialSaleChartBySaleCount(start, end, 15, chart);
            else
                InitialSaleChartBySales(start, end, 15, chart);
            return chart;
        }


        #endregion

        #endregion

        #region 商家

        public LineChartDataModel<int> GetShopFlowChart(long shopId, int year, int month)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>() { SeriesData = new List<ChartSeries<int>>() };
            ChechData(year, month);
            DateTime thisMonth = new DateTime(year, month, 1);
            DateTime thisMonthEnd = thisMonth.AddMonths(1);

            if (month == 1)
            {
                month = 12;
                year -= 1;
            }
            else
            {
                month--;
            }
            DateTime prevMonth = new DateTime(year, month, 1);
            DateTime prevMonthEnd = prevMonth.AddMonths(1);


            TimeSpan tt = thisMonth.AddMonths(1) - thisMonth;
            chart.XAxisData = GenerateStringByDays(31);

            #region MS SQL

            //var thisMonthData = (from m in context.ShopVistiInfo
            //                    orderby m.Date
            //                    where m.Date >= thisMonth && m.ShopId == shopId
            //                    && m.Date < thisMonthEnd
            //                    group m by m.Date into G
            //                    select new
            //                    {
            //                        G.Key,
            //                        Count = G.Sum(s => s.VistiCounts)
            //                    }).ToList();
            //var chartItem = new ChartSeries<int> { Name = string.Format("{0}月店铺总流量", thisMonth.Month), Data = new int[31] };
            //for (int i = 0; i < 31; i++)
            //{
            //    var date = thisMonth.AddDays(i);
            //    if (thisMonthData.Any(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day))
            //        chartItem.Data[i] = (int)thisMonthData.FirstOrDefault(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day).Count;
            //}
            //chart.SeriesData.Add(chartItem);

            ////上个月的数据集 
            //var prevMonthData = (from m in context.ShopVistiInfo
            //                    orderby m.Date
            //                    where m.Date >= prevMonth
            //                    && m.Date < prevMonthEnd && m.ShopId == shopId
            //                    group m by m.Date into G
            //                    select new
            //                    {
            //                        G.Key,
            //                        Count = G.Sum(s => s.VistiCounts)
            //                    }).ToList();
            //chartItem = new ChartSeries<int> { Name = string.Format("{0}月店铺总流量", prevMonth.Month), Data = new int[31] };
            //for (int i = 0; i < 31; i++)
            //{
            //    var date = prevMonth.AddDays(i);
            //    if (prevMonthData.Any(d =>  d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day))
            //        chartItem.Data[i] = (int)prevMonthData.FirstOrDefault(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day).Count;
            //}
            //chart.SeriesData.Add(chartItem);

            #endregion

            #region MySQL

            var thisMonthData = Context.ShopVistiInfo.Where(m => m.Date >= thisMonth && m.ShopId == shopId
                                 && m.Date < thisMonthEnd)
               .Select(m => new { m.VistiCounts, m.Date.Year, m.Date.Month, m.Date.Day })
               .GroupBy(m => new { m.Year, m.Month, m.Day })
               .Select(g => new { Count = g.Sum(s => s.VistiCounts), g.Key.Year, g.Key.Month, g.Key.Day }).ToList();

            var chartItem = new ChartSeries<int> { Name = string.Format("{0}月店铺总流量", thisMonth.Month), Data = new int[31] };
            for (int i = 0; i < 31; i++)
            {
                var date = thisMonth.AddDays(i);
                if (thisMonthData.Any(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day))
                    chartItem.Data[i] = (int)thisMonthData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day).Count;
            }
            chart.SeriesData.Add(chartItem);

            //上个月的数据集 
            var prevMonthData = Context.ShopVistiInfo.Where(m => m.Date >= prevMonth && m.ShopId == shopId
                                && m.Date < prevMonthEnd)
              .Select(m => new { m.VistiCounts, m.Date.Year, m.Date.Month, m.Date.Day })
              .GroupBy(m => new { m.Year, m.Month, m.Day })
              .Select(g => new { Count = g.Sum(s => s.VistiCounts), g.Key.Year, g.Key.Month, g.Key.Day }).ToList();

            chartItem = new ChartSeries<int> { Name = string.Format("{0}月店铺总流量", prevMonth.Month), Data = new int[31] };
            for (int i = 0; i < 31; i++)
            {
                var date = prevMonth.AddDays(i);
                if (prevMonthData.Any(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day))
                    chartItem.Data[i] = (int)prevMonthData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day).Count;
            }
            chart.SeriesData.Add(chartItem);

            #endregion


            return chart;


        }


        public LineChartDataModel<int> GetProductVisitRankingChart(long shopId, DateTime day, int rankSize = 15)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[rankSize],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[rankSize]
            };

            for (int i = 1; i <= rankSize; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            var start = day;
            var end = start.AddHours(24);

            InitialProductVisit(shopId, start, end, 15, chart);
            return chart;
        }

        public LineChartDataModel<int> GetProductVisitRankingChart(long shopId, int year, int month, int rankSize = 15)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[rankSize],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[rankSize]
            };

            for (int i = 1; i <= rankSize; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            InitialProductVisit(shopId, start, end, 15, chart);
            return chart;
        }

        public LineChartDataModel<int> GetProductVisitRankingChart(long shopId, int year, int month, int weekIndex, int rankSize = 15)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[rankSize],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[rankSize]
            };

            for (int i = 1; i <= rankSize; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            var start = DateTimeHelper.GetStartDayOfWeeks(year, month, weekIndex);
            if (DateTime.MinValue.Equals(start))
                throw new ArgumentException("参数错误");

            var end = start.AddDays(6);

            InitialProductVisit(shopId, start, end, 15, chart);
            return chart;
        }

        private void InitialProductVisit(long shopId, DateTime start, DateTime end, int rankSize, LineChartDataModel<int> model)
        {
            int index = 0;
            var data = (from t in
                            (from s in Context.ShopInfo
                             join p in Context.ProductInfo on s.Id equals p.ShopId
                             join pv in Context.ProductVistiInfo on p.Id equals pv.ProductId
                             where pv.Date >= start && pv.Date < end && s.Id == shopId && p.IsDeleted == false
                             select new
                             {
                                 Date = pv.Date,
                                 Count = pv.VistiCounts,
                                 ProductId = p.Id,
                                 ProductName = p.ProductName

                             }).ToList()
                        orderby t.Date
                        group t by new { t.Date, t.ProductName, t.ProductId } into G
                        select new
                        {
                            ProductName = G.FirstOrDefault().ProductName,
                            Visit = G.Sum(t => t.Count)
                        }).OrderByDescending(x => x.Visit).ToList();

            if (null != data && 0 < data.Count())
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "商品浏览量排行Top" + rankSize.ToString() };
                foreach (var item in data.Take(rankSize))
                {
                    series.Data[index] = (int)item.Visit;
                    model.ExpandProp[index++] = item.ProductName;
                }
                model.SeriesData.Add(series);
            }
            else
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "商品浏览量排行Top" + rankSize.ToString() };
                for (int i = 0; i < rankSize; i++)
                {

                    series.Data[index] = 0;
                    model.ExpandProp[index++] = "";
                }
                model.SeriesData.Add(series);
            }
        }


        public LineChartDataModel<int> GetProductSaleRankingChart(long shopId, DateTime day, SaleDimension dimension = SaleDimension.SaleCount, int rankSize = 15)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[rankSize],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[rankSize]
            };

            for (int i = 1; i <= rankSize; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            var start = day;
            var end = start.AddHours(24);

            if (dimension == SaleDimension.SaleCount)
                InitialProductSaleChartBySaleCount(shopId, start, end, 15, chart);
            else
                InitialProductSaleChartBySales(shopId, start, end, 15, chart);
            return chart;
        }

        public LineChartDataModel<int> GetProductSaleRankingChart(long shopId, int year, int month, SaleDimension dimension = SaleDimension.SaleCount, int rankSize = 15)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[rankSize],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[rankSize]
            };

            for (int i = 1; i <= rankSize; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            if (dimension == SaleDimension.SaleCount)
                InitialProductSaleChartBySaleCount(shopId, start, end, 15, chart);
            else
                InitialProductSaleChartBySales(shopId, start, end, 15, chart);
            return chart;
        }

        public LineChartDataModel<int> GetProductSaleRankingChart(long shopId, int year, int month, int weekIndex, SaleDimension dimension = SaleDimension.SaleCount, int rankSize = 15)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[rankSize],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[rankSize]
            };

            for (int i = 1; i <= rankSize; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
            }

            var start = DateTimeHelper.GetStartDayOfWeeks(year, month, weekIndex);
            if (DateTime.MinValue.Equals(start))
                throw new ArgumentException("参数错误");

            var end = start.AddDays(6);

            if (dimension == SaleDimension.SaleCount)
                InitialProductSaleChartBySaleCount(shopId, start, end, 15, chart);
            else
                InitialProductSaleChartBySales(shopId, start, end, 15, chart);
            return chart;
        }

        private void InitialProductSaleChartBySaleCount(long shopId, DateTime start, DateTime end, int rankSize, LineChartDataModel<int> model)
        {
            int index = 0;
            var data = (from t in
                            (from pv in Context.ProductVistiInfo
                             join p in Context.ProductInfo on pv.ProductId equals p.Id
                             join s in Context.ShopInfo on p.ShopId equals s.Id
                             where pv.Date >= start && pv.Date < end && s.Id == shopId && p.IsDeleted == false
                             select new
                             {
                                 Date = pv.Date,
                                 Count = pv.SaleCounts,
                                 ProductId = pv.ProductId,
                                 ProductName = p.ProductName,


                             }).ToList()
                        orderby t.Count
                        group t by new { t.ProductId } into G
                        select new
                        {
                            ProductName = G.FirstOrDefault().ProductName,
                            Count = G.Sum(t => t.Count)
                        }).OrderByDescending(x => x.Count).ToList();

            if (null != data && 0 < data.Count())
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "月销售量Top" + rankSize.ToString() + "      " + start.ToString("yyyy-MM-dd") + "至" + end.ToString("yyyy-MM-dd") };
                var items = data.Take(rankSize).ToList();
                foreach (var item in items)
                {
                    series.Data[index] = (int)item.Count;
                    model.ExpandProp[index++] = item.ProductName;
                }
                model.SeriesData.Add(series);
            }
            else
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "月销售量Top" + rankSize.ToString() + "      " + start.ToString("yyyy-MM-dd") + "至" + end.ToString("yyyy-MM-dd") };
                for (int i = 0; i < rankSize; i++)
                {

                    series.Data[index] = 0;
                    model.ExpandProp[index++] = "";
                }
                model.SeriesData.Add(series);
            }

        }



        private void InitialProductSaleChartBySales(long shopId, DateTime start, DateTime end, int rankSize, LineChartDataModel<int> model)
        {
            int index = 0;
            var data = (from t in
                            (from pv in Context.ProductVistiInfo
                             join p in Context.ProductInfo on pv.ProductId equals p.Id
                             join s in Context.ShopInfo on p.ShopId equals s.Id
                             where pv.Date >= start && pv.Date < end && s.Id == shopId && p.IsDeleted == false
                             select new
                             {
                                 Date = pv.Date,
                                 ProductId = pv.ProductId,
                                 ProductName = p.ProductName,
                                 RealTotalPrice = pv.SaleAmounts


                             }).ToList()
                        orderby t.Date
                        group t by new { t.ProductId, t.ProductName } into G
                        select new
                        {
                            ProductName = G.FirstOrDefault().ProductName,
                            Money = G.Sum(t => t.RealTotalPrice)
                        }).OrderByDescending(x => x.Money).ToList();

            if (null != data && 0 < data.Count())
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "商品销售额排行Top" + rankSize.ToString() };
                foreach (var item in data.Take(rankSize))
                {
                    series.Data[index] = (int)item.Money;
                    model.ExpandProp[index++] = item.ProductName;
                }
                model.SeriesData.Add(series);
            }
            else
            {
                ChartSeries<int> series = new ChartSeries<int>() { Data = new int[rankSize], Name = "商品销售额排行Top" + rankSize.ToString() };
                for (int i = 0; i < rankSize; i++)
                {

                    series.Data[index] = 0;
                    model.ExpandProp[index++] = "";
                }
                model.SeriesData.Add(series);
            }
        }

        #endregion

        public LineChartDataModel<int> GetShopSaleChart(long shopId, int year, int month)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>() { SeriesData = new List<ChartSeries<int>>() };
            ChechData(year, month);
            DateTime thisMonth = new DateTime(year, month, 1);
            DateTime thisMonthEnd = thisMonth.AddMonths(1);

            if (month == 1)
            {
                month = 12;
                year -= 1;
            }
            else
            {
                month--;
            }
            DateTime prevMonth = new DateTime(year, month, 1);
            DateTime prevMonthEnd = prevMonth.AddMonths(1);


            TimeSpan tt = thisMonth.AddMonths(1) - thisMonth;
            chart.XAxisData = GenerateStringByDays(31);

            #region MS SQL

            //var thisMonthData = (from sv in context.ShopVistiInfo
            //                    orderby sv.Date
            //                    where sv.Date >= thisMonth && sv.ShopId == shopId
            //                    && sv.Date < thisMonthEnd
            //                    group sv by new { sv.Date.Year, sv.Date.Month, sv.Date.Day } into G
            //                    select new
            //                    {
            //                        G.Key,
            //                        Count = G.Sum(s => s.SaleCounts)
            //                    }).ToList();
            //var chartItem = new ChartSeries<int> { Name = string.Format("{0}月店铺总销量", thisMonth.Month), Data = new int[31] };
            //for (int i = 0; i < 31; i++)
            //{
            //    var date = thisMonth.AddDays(i);
            //    if (thisMonthData.Any(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day))
            //        chartItem.Data[i] = (int)thisMonthData.FirstOrDefault(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day).Count;
            //}
            //chart.SeriesData.Add(chartItem);

            ////上个月的数据集 
            //var prevMonthData = (from sv in context.ShopVistiInfo
            //                     orderby sv.Date
            //                     where sv.Date >= prevMonth && sv.ShopId == shopId
            //                     && sv.Date < prevMonthEnd
            //                     group sv by new { sv.Date.Year, sv.Date.Month, sv.Date.Day } into G
            //                     select new
            //                     {
            //                         G.Key,
            //                         Count = G.Sum(s => s.SaleCounts)
            //                     }).ToList();
            //chartItem = new ChartSeries<int> { Name = string.Format("{0}月店铺总销量", prevMonth.Month), Data = new int[31] };
            //for (int i = 0; i < 31; i++)
            //{
            //    var date = prevMonth.AddDays(i);
            //    if (prevMonthData.Any(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day))
            //        chartItem.Data[i] = (int)prevMonthData.FirstOrDefault(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day).Count;
            //}
            //chart.SeriesData.Add(chartItem);

            #endregion 

            #region MySQL

            var thisMonthData = Context.ShopVistiInfo.Where(sv => sv.Date >= thisMonth && sv.ShopId == shopId && sv.Date < thisMonthEnd)
               .Select(m => new { m.SaleCounts, m.Date.Year, m.Date.Month, m.Date.Day })
               .GroupBy(m => new { m.Year, m.Month, m.Day })
               .Select(g => new { Count = g.Sum(s => s.SaleCounts), g.Key.Year, g.Key.Month, g.Key.Day }).ToList();


            var chartItem = new ChartSeries<int> { Name = string.Format("{0}月店铺总销量", thisMonth.Month), Data = new int[31] };
            for (int i = 0; i < 31; i++)
            {
                var date = thisMonth.AddDays(i);
                if (thisMonthData.Any(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day))
                    chartItem.Data[i] = (int)thisMonthData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day).Count;
            }
            chart.SeriesData.Add(chartItem);

            //上个月的数据集 
            var prevMonthData = Context.ShopVistiInfo.Where(sv => sv.Date >= prevMonth && sv.ShopId == shopId && sv.Date < prevMonthEnd)
               .Select(m => new { m.SaleCounts, m.Date.Year, m.Date.Month, m.Date.Day })
               .GroupBy(m => new { m.Year, m.Month, m.Day })
               .Select(g => new { Count = g.Sum(s => s.SaleCounts), g.Key.Year, g.Key.Month, g.Key.Day }).ToList();

            chartItem = new ChartSeries<int> { Name = string.Format("{0}月店铺总销量", prevMonth.Month), Data = new int[31] };
            for (int i = 0; i < 31; i++)
            {
                var date = prevMonth.AddDays(i);
                if (prevMonthData.Any(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day))
                    chartItem.Data[i] = (int)prevMonthData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day).Count;
            }
            chart.SeriesData.Add(chartItem);


            #endregion

            return chart;
        }


        public LineChartDataModel<int> GetRecentMonthSaleRankChart(long shopId)
        {
            LineChartDataModel<int> chart = new LineChartDataModel<int>()
            {
                XAxisData = new string[15],
                SeriesData = new List<ChartSeries<int>>(),
                ExpandProp = new string[15]
            };
            for (int i = 1; i <= 15; i++)
            {
                chart.XAxisData[i - 1] = i.ToString();
                chart.ExpandProp[i - 1] = "";
            }
            InitialProductSaleChartBySaleCount(shopId, DateTime.Now.AddMonths(-1), DateTime.Now, 15, chart);
            return chart;
        }


        public LineChartDataModel<float> GetDealConversionRateChart(long shopId, int year, int month)
        {
            LineChartDataModel<float> chart = new LineChartDataModel<float>() { SeriesData = new List<ChartSeries<float>>() };
            ChechData(year, month);
            DateTime thisMonth = new DateTime(year, month, 1);
            DateTime thisMonthEnd = thisMonth.AddMonths(1);

            if (month == 1)
            {
                month = 12;
                year -= 1;
            }
            else
            {
                month--;
            }
            DateTime prevMonth = new DateTime(year, month, 1);
            DateTime prevMonthEnd = prevMonth.AddMonths(1);


            TimeSpan tt = thisMonth.AddMonths(1) - thisMonth;
            chart.XAxisData = GenerateStringByDays(31);

            #region MS SQL

            //var thisMonthData = (from m in context.ShopVistiInfo
            //                    orderby m.Date
            //                    where m.Date >= thisMonth && m.ShopId == shopId
            //                    && m.Date < thisMonthEnd
            //                    group m by new { m.Date.Year, m.Date.Month, m.Date.Day } into G
            //                    select new
            //                    {
            //                        G.Key,
            //                        Count = G.Sum(s => s.VistiCounts == 0 ? 0 : (float)s.SaleCounts / s.VistiCounts)
            //                    }).ToList();
            //var chartItem = new ChartSeries<float> { Name = string.Format("{0}月成交转化率", thisMonth.Month), Data = new float[31] };
            //for (int i = 0; i < 31; i++)
            //{
            //    var date = thisMonth.AddDays(i);
            //    if (thisMonthData.Any(d => d.Key.Year==date.Year&&d.Key.Month==date.Month&&d.Key.Day==date.Day))
            //        chartItem.Data[i] = (float)Math.Round(thisMonthData.FirstOrDefault(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day).Count * 100, 2);
            //}
            //chart.SeriesData.Add(chartItem);

            ////上个月的数据集 
            //var prevMonthData = (from m in context.ShopVistiInfo
            //                    orderby m.Date
            //                    where m.Date >= prevMonth
            //                    && m.Date < prevMonthEnd && m.ShopId == shopId
            //                    group m by new { m.Date.Year, m.Date.Month, m.Date.Day } into G
            //                    select new
            //                    {
            //                        G.Key,
            //                        Count = G.Sum(s => s.VistiCounts == 0 ? 0 : (float)s.SaleCounts / s.VistiCounts)
            //                    }).ToList();
            //chartItem = new ChartSeries<float> { Name = string.Format("{0}月成交转化率", prevMonth.Month), Data = new float[31] };
            //for (int i = 0; i < 31; i++)
            //{
            //    var date = prevMonth.AddDays(i);
            //    if (prevMonthData.Any(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day))
            //        chartItem.Data[i] = (float)Math.Round(prevMonthData.FirstOrDefault(d => d.Key.Year == date.Year && d.Key.Month == date.Month && d.Key.Day == date.Day).Count * 100, 2);
            //}
            //chart.SeriesData.Add(chartItem);

            #endregion

            #region MySQL

            var thisMonthData = Context.ShopVistiInfo.Where(m => m.Date >= thisMonth && m.ShopId == shopId && m.Date < thisMonthEnd)
              .Select(m => new { m.VistiCounts, m.SaleCounts, m.Date.Year, m.Date.Month, m.Date.Day })
              .GroupBy(m => new { m.Year, m.Month, m.Day })
              .Select(g => new { Count = g.Sum(s => s.VistiCounts == 0 ? 0 : (float)s.SaleCounts / s.VistiCounts), g.Key.Year, g.Key.Month, g.Key.Day }).ToList();


            var chartItem = new ChartSeries<float> { Name = string.Format("{0}月成交转化率", thisMonth.Month), Data = new float[31] };
            for (int i = 0; i < 31; i++)
            {
                var date = thisMonth.AddDays(i);
                if (thisMonthData.Any(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day))
                    chartItem.Data[i] = (float)Math.Round(thisMonthData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day).Count * 100, 2);
            }
            chart.SeriesData.Add(chartItem);

            //上个月的数据集 
            var prevMonthData = Context.ShopVistiInfo.Where(m => m.Date >= prevMonth && m.Date < prevMonthEnd && m.ShopId == shopId)
              .Select(m => new { m.VistiCounts, m.SaleCounts, m.Date.Year, m.Date.Month, m.Date.Day })
              .GroupBy(m => new { m.Year, m.Month, m.Day })
              .Select(g => new { Count = g.Sum(s => s.VistiCounts == 0 ? 0 : (float)s.SaleCounts / s.VistiCounts), g.Key.Year, g.Key.Month, g.Key.Day }).ToList();

            chartItem = new ChartSeries<float> { Name = string.Format("{0}月成交转化率", prevMonth.Month), Data = new float[31] };
            for (int i = 0; i < 31; i++)
            {
                var date = prevMonth.AddDays(i);
                if (prevMonthData.Any(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day))
                    chartItem.Data[i] = (float)Math.Round(prevMonthData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day).Count * 100, 2);
            }
            chart.SeriesData.Add(chartItem);

            #endregion
            return chart;
        }

        #region 前台统计
        public void AddPlatVisitUser(DateTime dt, long num)
        {
            var platVisit = Context.PlatVisitsInfo.FirstOrDefault(e => e.Date == dt);
            if (platVisit == null)
            {
                platVisit = new PlatVisitsInfo();
                platVisit.Date = dt;
                platVisit.VisitCounts = 1;
                Context.PlatVisitsInfo.Add(platVisit);
            }
            else
            {
                platVisit.VisitCounts += num;
            }
            Context.SaveChanges();
        }

        public void AddShopVisitUser(DateTime dt, long shopId, long num)
        {
            var shopVisit = Context.ShopVistiInfo.FirstOrDefault(e => e.Date == dt && e.ShopId == shopId);
            if (shopVisit == null)
            {
                shopVisit = new ShopVistiInfo();
                shopVisit.Date = dt.Date;
                shopVisit.VistiCounts = 1;
                shopVisit.ShopId = shopId;
                Context.ShopVistiInfo.Add(shopVisit);
            }
            else
            {
                shopVisit.VistiCounts += num;
            }
            Context.SaveChanges();
        }


        public void AddProductVisitUser(DateTime dt, long pid, long shopId, long num)
        {
            var productVisit = Context.ProductVistiInfo.FirstOrDefault(e => e.Date == dt && e.ProductId == pid);
            if (productVisit == null)
            {
                productVisit = new ProductVistiInfo();
                productVisit.Date = dt;
                productVisit.ShopId = shopId;
                productVisit.ProductId = pid;
                productVisit.VisitUserCounts = num;
                Context.ProductVistiInfo.Add(productVisit);
            }
            else
            {
                productVisit.VisitUserCounts += num;
            }
            Context.SaveChanges();
        }

        public void AddProductVisit(DateTime dt, long pid, long shopId, long num)
        {
            var productVisit = Context.ProductVistiInfo.FirstOrDefault(e => e.Date == dt && e.ProductId == pid);
            if (productVisit == null)
            {
                productVisit = new ProductVistiInfo();
                productVisit.Date = dt;
                productVisit.ShopId = shopId;
                productVisit.ProductId = pid;
                productVisit.VistiCounts = num;
                Context.ProductVistiInfo.Add(productVisit);
            }
            else
            {
                productVisit.VistiCounts += num;
            }
            Context.SaveChanges();
        }
        #endregion 前台统计

        #region 后台统计表
        public QueryPageModel<ProductStatisticModel> GetProductVisits(ProductStatisticQuery query)
        {
            StringBuilder sql = new StringBuilder();
            StringBuilder strOrderBy = new StringBuilder();
            StringBuilder strWhere = new StringBuilder();

            var enddate = query.EndDate.Date.AddDays(1);
            if (query.ShopId.HasValue)
            {
                strWhere.AppendFormat(" and Date>='{0}' and Date<'{1}' and shopId={2}", query.StartDate.Date, enddate, query.ShopId.Value);
            }
            else
            {
                strWhere.AppendFormat(" and Date>='{0}' and Date<'{1}'", query.StartDate.Date, enddate);
            }

            if (!string.IsNullOrWhiteSpace(query.Sort))
            {
                string strOrderByAsc = query.IsAsc ? "asc" : "desc";
                switch (query.Sort.ToLower().Trim())
                {
                    case "visticounts":
                        strOrderBy.AppendFormat("visticounts {0}", strOrderByAsc);
                        break;
                    case "visitusercounts":
                        strOrderBy.AppendFormat("visitusercounts {0}", strOrderByAsc);
                        break;
                    case "payusercounts":
                        strOrderBy.AppendFormat("payusercounts {0}", strOrderByAsc);
                        break;
                    case "singlepercentconversion":
                        strOrderBy.AppendFormat("case when VisitUserCounts>0 then PayUserCounts / VisitUserCounts else 1 end {0}", strOrderByAsc);
                        break;
                    case "salecounts":
                        strOrderBy.AppendFormat("salecounts {0}", strOrderByAsc);
                        break;
                    case "saleamounts":
                        strOrderBy.AppendFormat("saleamounts {0}", strOrderByAsc);
                        break;
                }
            }
            else
            {
                strOrderBy.Append("ProductId desc");
            }

            sql.AppendFormat("select * from (SELECT ProductId AS `ProductId`,SUM(`SaleAmounts`) AS `SaleAmounts`,SUM(`SaleCounts`) AS `SaleCounts`,SUM(`PayUserCounts`) AS `PayUserCounts`,SUM(`VisitUserCounts`) AS `VisitUserCounts`,SUM(`VistiCounts`) AS `VistiCounts` FROM `Himall_ProductVistis` where 1=1 {0} GROUP BY `ProductId`)tb1 order by {1} ", strWhere.ToString(), strOrderBy.ToString());
            
            //分页
            sql.AppendFormat(" limit {0},{1}", (query.PageNo - 1) * query.PageSize, query.PageSize);
            var model = Context.Database.SqlQuery<ProductStatisticModel>(sql.ToString()).ToList();

            sql = new StringBuilder();
            sql.AppendFormat("select count(1) as total from(SELECT ProductId FROM `Himall_ProductVistis` where 1=1 {0} GROUP BY `ProductId`)tb", strWhere.ToString());
            int total = Context.Database.SqlQuery<int>(sql.ToString()).ToList()[0];

            #region 
            //var visitModels = Context.ProductVistiInfo.AsQueryable();
            //var enddate = query.EndDate.Date.AddDays(1);
            //if (!query.ShopId.HasValue)
            //{
            //    visitModels = visitModels.Where(e => e.Date >= query.StartDate.Date && e.Date < enddate);
            //}
            //else
            //{
            //    visitModels = visitModels.Where(e => e.Date >= query.StartDate.Date && e.Date < enddate && e.ShopId == query.ShopId.Value);
            //}
            //var statisticModels = visitModels.GroupBy(e => e.ProductId).Select(g => new ProductStatisticModel()
            //{
            //    ProductId = g.Key,
            //    SaleAmounts = g.Sum(a => a.SaleAmounts),
            //    SaleCounts = g.Sum(a => a.SaleCounts),
            //    PayUserCounts = g.Sum(a => a.PayUserCounts),
            //    VisitUserCounts = g.Sum(a => a.VisitUserCounts),
            //    VistiCounts = g.Sum(a => a.VistiCounts)
            //});

            //var orderBy = statisticModels.OrderByDescending(d => d.ProductId);
            //if (!string.IsNullOrWhiteSpace(query.Sort))
            //{
            //    switch (query.Sort.ToLower().Trim())
            //    {
            //        case "visticounts":
            //            if (query.IsAsc)
            //                orderBy = statisticModels.OrderBy(d => d.VistiCounts);
            //            else
            //                orderBy = statisticModels.OrderByDescending(d => d.VistiCounts);
            //            break;
            //        case "visitusercounts":
            //            if (query.IsAsc)
            //                orderBy = statisticModels.OrderBy(d => d.VisitUserCounts);
            //            else
            //                orderBy = statisticModels.OrderByDescending(d => d.VisitUserCounts);
            //            break;
            //        case "payusercounts":
            //            if (query.IsAsc)
            //                orderBy = statisticModels.OrderBy(d => d.PayUserCounts);
            //            else
            //                orderBy = statisticModels.OrderByDescending(d => d.PayUserCounts);
            //            break;
            //        case "singlepercentconversion":
            //            if (query.IsAsc)
            //                orderBy = statisticModels.OrderBy(d => d.PayUserCounts / (d.VisitUserCounts + 1));
            //            else
            //                orderBy = statisticModels.OrderByDescending(d => d.PayUserCounts / (d.VisitUserCounts + 1));
            //            break;
            //        case "salecounts":
            //            if (query.IsAsc)
            //                orderBy = statisticModels.OrderBy(d => d.SaleCounts);
            //            else
            //                orderBy = statisticModels.OrderByDescending(d => d.SaleCounts);
            //            break;
            //        case "saleamounts":
            //            if (query.IsAsc)
            //                orderBy = statisticModels.OrderBy(d => d.SaleAmounts);
            //            else
            //                orderBy = statisticModels.OrderByDescending(d => d.SaleAmounts);
            //            break;
            //    }
            //}
            //int total = 0;

            //var pageModels = statisticModels.GetPage(out total, e => orderBy, query.PageNo, query.PageSize).ToList();
            #endregion
            foreach (var item in model)
            {
                item.StartDate = query.StartDate;
                item.EndDate = query.EndDate;
            }
            return new QueryPageModel<ProductStatisticModel>
            {
                Models = model,
                Total = total
            };
        }

        public IEnumerable<ProductVistiInfo> GetProductAllVisits(ProductStatisticQuery query)
        {
            var productVisits = Context.ProductVistiInfo.AsQueryable();
            var endDate = query.EndDate.Date.AddDays(1);
            if (query.ShopId.HasValue)
            {
                productVisits = productVisits.Where(e => e.ShopId == query.ShopId.Value);
            }
            productVisits = productVisits.Where(e => e.Date >= query.StartDate.Date && e.Date < endDate);
            return productVisits.ToList();
        }
        public IEnumerable<ShopVistiInfo> GetShopVisits(DateTime startDt, DateTime endDt, long shopId = 0)
        {
            endDt = endDt.Date.AddDays(1);
            if (shopId == 0)
            {
                return Context.ShopVistiInfo.Where(e => e.Date >= startDt.Date && e.Date < endDt.Date).ToList();
            }
            else
            {
                return Context.ShopVistiInfo.Where(e => e.Date >= startDt.Date && e.Date < endDt.Date && e.ShopId == shopId).ToList();
            }
        }

        public IEnumerable<PlatVisitsInfo> GetPlatVisits(DateTime startDt, DateTime endDt)
        {
            endDt = endDt.Date.AddDays(1);
            return Context.PlatVisitsInfo.Where(e => e.Date >= startDt.Date && e.Date < endDt.Date).ToList();
        }


        #endregion 后台统计表 



    }
}
