using Himall.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinServiceBase;

namespace WinOrderProductStatisticsService
{
    /// <summary>
    /// 交易统计任务
    /// </summary>
    public class StatisticOrderProduct:ISyncData
    {
        static string _conString = string.Empty;
        static string GetConString()
        {
            string conString= ConfigurationManager.ConnectionStrings["Entities"].ConnectionString;
            return conString;
        }
        /// <summary>
        /// 实现
        /// </summary>
        /// <param name="context"></param>
        public void Execute()
        {
            var startDate = GetStatisticDate();//格式为日期，不记时分秒
            try
            {
                while (startDate < DateTime.Now.Date)
                {//按天统计
                    Log.Info(startDate.ToString());
                    StatisticPayProduct(startDate, startDate.AddDays(1));
                    startDate = startDate.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                Log.Error("交易统计异常：" + ex.Message);
            }
        }

        /// <summary>
        /// 取上一次统计时间
        /// </summary>
        /// <returns></returns>
        DateTime GetStatisticDate()
        {
            DateTime startDate = DateTime.Now.Date;
            try
            {
                Himall.Entity.Entities entity = new Himall.Entity.Entities();

                var firstRecord = entity.ProductVistiInfo.Where(e => e.StatisticFlag == false).FirstOrDefault();
                if (firstRecord != null)
                {
                    startDate = firstRecord.Date;
                }
                else
                {
                    //是否第一次统计
                    var visitInfo = entity.ProductVistiInfo.FirstOrDefault();
                    if (visitInfo == null)
                    {
                        var firstOrder = entity.OrderInfo.FirstOrDefault();
                        if (firstOrder != null)
                            startDate = firstOrder.OrderDate.Date;
                        else
                        {
                            startDate = DateTime.Now.Date;
                        }
                    }
                    else
                    {
                        startDate = visitInfo.Date;
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error("有问题:" + ex.Message + ":::" + ex.StackTrace);
            }
            return startDate;
        }

        void StatisticPayProduct(DateTime statisticStartDate,DateTime statisticEndDate)
        {
            Himall.Entity.Entities entity = new Himall.Entity.Entities();

            //时间段内已支付订单
            var payOrders = from o in entity.OrderInfo
                        join i in entity.OrderItemInfo on o.Id equals i.OrderId
                        where o.PayDate.HasValue && o.PayDate.Value >= statisticStartDate && o.PayDate.Value < statisticEndDate
                        select new
                        {
                            OrderId = o.Id,
                            OrderDate = o.OrderDate,
                            ShopId = o.ShopId,
                            ProductId = i.ProductId,
                            ProductName = i.ProductName,
                            TotalAmount = i.RealTotalPrice,//实际上是金额
                            OrderProductQuantity = i.Quantity,
                            UserId = o.UserId
                        };

            var payOrderGroups = payOrders.GroupBy(e => e.ProductId);

            List<ProductVistiInfo> productVisitRange = new List<ProductVistiInfo>();
            var pids = payOrderGroups.Select(e => e.Key).Distinct();
            var oldProductVisits = entity.ProductVistiInfo.Where(e => e.Date == statisticStartDate && pids.Contains(e.ProductId)).ToList();

            foreach(var g in payOrderGroups)
            {
                ProductVistiInfo productVisit = oldProductVisits.FirstOrDefault(e => e.ProductId == g.Key);
                if (productVisit==null)
                {
                    productVisit = new ProductVistiInfo();
                    productVisitRange.Add(productVisit);
                    //销售量、销售金额在订单逻辑里有实时处理，如果没有记录则进行统计
                    productVisit.SaleCounts = g.Sum(e => e.OrderProductQuantity);
                    productVisit.SaleAmounts = g.Sum(e => e.TotalAmount);
                }
                productVisit.Date = statisticStartDate;
                productVisit.ProductId = g.Key;
                productVisit.PayUserCounts = g.Select(e => e.UserId).Distinct().Count();
                productVisit.StatisticFlag = true;
                var item = g.FirstOrDefault();
                if (item != null)
                {
                    productVisit.ShopId = item.ShopId;
                }
            }
            //将没有付款记录的统计信息，统一修改为已统计
            var noPayOrdersStatistic = oldProductVisits.Where(e => !pids.Any(p => p == e.ProductId));
            foreach (var v in noPayOrdersStatistic)
            {
                v.StatisticFlag = true;
            }
            entity.ProductVistiInfo.AddRange(productVisitRange);
            entity.SaveChanges();
        }

    }
}
