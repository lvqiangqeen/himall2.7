using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Quartz;
using Himall.Model;

namespace Himall.Service.Job
{
    /// <summary>
    /// 交易统计任务
    /// </summary>
    public class StatisticOrderJob:IJob
    {
        /// <summary>
        /// Job实现
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            Entity.Entities entity = new Entity.Entities();
            var statisticDate = GetStatisticDate();//格式为日期，不记时分秒
            try
            {
                while (statisticDate < DateTime.Now.Date)
                {//按天统计
                    Log.Info(statisticDate.ToString());
                    StatisticCreateOrder(statisticDate, statisticDate.AddDays(1));
                    statisticDate = statisticDate.AddDays(1);
                }
            }
            catch(Exception ex)
            {
                Core.Log.Error("交易统计异常：" + ex.Message);
            }
            
        }

        /// <summary>
        /// 取上一次统计时间
        /// </summary>
        /// <returns></returns>
        DateTime GetStatisticDate()
        {
            DateTime startDate = DateTime.Now.Date;
            Entity.Entities entity = new Entity.Entities();
            var lastRecord = entity.ShopVistiInfo.Where(item => item.StatisticFlag == false).FirstOrDefault();
            if (lastRecord != null)
            {
                startDate = lastRecord.Date;
            }
            else
            {
                //是否第一次统计
                var visitInfo = entity.ShopVistiInfo.FirstOrDefault();
                if (visitInfo==null)
                {//取第一个订单时间
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
            return startDate;
        }

        void StatisticCreateOrder(DateTime statisticStartDate,DateTime statisticEndDate)
        {
            Entity.Entities entity = new Entity.Entities();
            //时间段内所有订单（下单数据统计）
            var orders = entity.OrderInfo.Where(e => e.OrderDate >= statisticStartDate && e.OrderDate < statisticEndDate).ToList();
            var orderGroups = orders.GroupBy(e => e.ShopId);
            //已存在的店铺统计
            var shopids = orderGroups.Select(e => e.Key).Distinct();
            var shopVisitInfos = entity.ShopVistiInfo.Where(e => shopids.Contains(e.ShopId) && e.Date == statisticStartDate).ToList();
            List<ShopVistiInfo> shopVisitRange = new List<ShopVistiInfo>();
            foreach(var g in orderGroups)
            {
                ShopVistiInfo shopVisit = shopVisitInfos.FirstOrDefault(e => e.ShopId == g.Key);
                if (shopVisit == null)
                {
                    shopVisit = new ShopVistiInfo();
                    shopVisitRange.Add(shopVisit);
                }
                shopVisit.Date = statisticStartDate;
                shopVisit.ShopId = g.Key;
                shopVisit.OrderCount = g.Count();
                shopVisit.OrderAmount = g.Sum(e => e.TotalAmount);
                shopVisit.OrderUserCount = g.Select(e => e.UserId).Distinct().Count();
                shopVisit.OrderProductCount = g.Sum(e => e.OrderProductQuantity);
                shopVisit.StatisticFlag = true;
            }

            //将没有订单记录的统计信息，统一修改为已统计
            var noOrdersStatistic = shopVisitInfos.Where(e => !shopids.Any(p => p == e.ShopId));
            foreach (var v in noOrdersStatistic)
            {
                v.StatisticFlag = true;
            }
            entity.ShopVistiInfo.AddRange(shopVisitRange);
            entity.SaveChanges();

            //时间段内已支付订单(付款数据统计)
            var payOrders = entity.OrderInfo.Where(e => e.PayDate.HasValue && e.PayDate.Value >= statisticStartDate && e.PayDate.Value < statisticEndDate).ToList();
            var payOrderGroups = payOrders.GroupBy(e => e.ShopId);
            //已存在的店铺统计
            shopids = payOrderGroups.Select(e => e.Key).Distinct();
            shopVisitInfos = entity.ShopVistiInfo.Where(e => shopids.Contains(e.ShopId) && e.Date == statisticStartDate).ToList();
            shopVisitRange = new List<ShopVistiInfo>();
            foreach (var g in payOrderGroups)
            {
                ShopVistiInfo shopVisit = shopVisitInfos.FirstOrDefault(e => e.ShopId == g.Key);
                if (shopVisit == null)
                {
                    shopVisit = new ShopVistiInfo();
                    shopVisitRange.Add(shopVisit);
                }
                shopVisit.Date = statisticStartDate;
                shopVisit.ShopId = g.Key;
                shopVisit.OrderPayCount = g.Count();
                shopVisit.OrderPayUserCount = g.Select(e => e.UserId).Distinct().Count();
                shopVisit.SaleAmounts = g.Sum(e => e.TotalAmount);
                shopVisit.SaleCounts = g.Sum(e => e.OrderProductQuantity);
                shopVisit.StatisticFlag = true;
            }
            //将没有订单记录的统计信息，统一修改为已统计
            var noPayOrdersStatistic = shopVisitInfos.Where(e => !shopids.Any(p => p == e.ShopId));
            foreach (var v in noPayOrdersStatistic)
            {
                v.StatisticFlag = true;
            }
            entity.ShopVistiInfo.AddRange(shopVisitRange);

            //平台统计
            PlatVisitsInfo platVisit = entity.PlatVisitsInfo.FirstOrDefault(e => e.Date == statisticStartDate);
            if (platVisit == null)
            {
                platVisit = new PlatVisitsInfo();
                //添加
                entity.PlatVisitsInfo.Add(platVisit);
            }
            platVisit.Date = statisticStartDate;
            platVisit.OrderCount = orders.Count();
            platVisit.OrderAmount = orders.Sum(e => e.TotalAmount);
            platVisit.OrderUserCount = orders.Select(e => e.UserId).Distinct().Count();
            platVisit.OrderProductCount = orders.Sum(e => e.OrderProductQuantity);
            //已支付订单
            platVisit.OrderPayCount = payOrders.Count();
            platVisit.OrderPayUserCount = payOrders.Select(e => e.UserId).Distinct().Count();
            platVisit.SaleAmounts = payOrders.Sum(e => e.TotalAmount);
            platVisit.SaleCounts = payOrders.Sum(e => e.OrderProductQuantity);
            platVisit.StatisticFlag = true;
            
            entity.SaveChanges();
        }

    }
}
