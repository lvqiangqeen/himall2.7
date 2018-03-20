using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Himall.CommonModel;
using Himall.Core;
using System.Text;
using MySql.Data.MySqlClient;

namespace Himall.Service
{
    /// <summary>
    /// 订单和销售统计
    /// </summary>
    public class OrderAndSaleStatisticsService : ServiceBase, IOrderAndSaleStatisticsService
    {
        //注意本类只能使用 主要使用order 相关表，除Order相关实体外，可以任意关联其他实体


        /// <summary>
        /// 获取会员基本统计信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OrderBasicStatistics GetLastThreeMonthOrderStatisticsByUser(long userId)
        {
            var time = DateTime.Now.AddMonths(-3);
            //最近三个月的订单，即只需要查询Mysql中所有的订单
            var orders = Context.OrderInfo.Where(item => item.UserId == userId && item.PayDate.HasValue && item.OrderDate >= time);
            var userOrderStatistics = GetLastThreeMonthOrderStatistics(orders);
            return userOrderStatistics;
        }



        #region 私有方法

        /// <summary>
        /// 根据已有条件的订单获取其统计信息
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        OrderBasicStatistics GetLastThreeMonthOrderStatistics(IQueryable<OrderInfo> orders)
        {
            var userOrderStatistics = new OrderBasicStatistics();
            userOrderStatistics.TradeAmount = orders.Sum(item => (decimal?)item.ActualPayAmount).GetValueOrDefault();
            userOrderStatistics.TradeCount = orders.Count();
            var latestOrder = orders.OrderByDescending(item => item.Id).FirstOrDefault();
            if (latestOrder != null)
                userOrderStatistics.SleepDays = (int)(DateTime.Now - latestOrder.OrderDate).TotalDays;
            return userOrderStatistics;
        }

        #endregion

        /// <summary>
        /// 获取店铺下门店销售额（按天排序）
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="branchShopId"></param>
        /// <returns></returns>
        public List<BranchShopDayFeat> GetDayAmountSale(DateTime start, DateTime end, long branchShopId)
        {
            var en = end.AddDays(1);
            var orders = Context.OrderInfo.Where(a => a.ShopBranchId == branchShopId && a.PayDate.HasValue && a.PayDate > start && a.PayDate.Value < en);
            orders = orders.Where(p => p.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay && p.OrderStatus != OrderInfo.OrderOperateStatus.Close);
            var sum = orders.ToList().GroupBy(a => a.PayDate.Value.Date);
            var result = sum.Select(a => new BranchShopDayFeat()
            {
                BranchShopId = branchShopId,
                SaleAmount = a.Sum(x => (decimal?)x.ActualPayAmount).GetValueOrDefault(),
                Day = a.Key
            }).ToList();
            var day = (end - start).Days;
            List<BranchShopDayFeat> list = new List<BranchShopDayFeat>();
            for (int i = 0; i <= day; i++)
            {
                BranchShopDayFeat m = new BranchShopDayFeat();
                m.BranchShopId = branchShopId;
                var time = start.Date.AddDays(i);
                m.Day = time;
                var ex = result.Where(a => a.Day.Date == time).FirstOrDefault();
                if (ex != null)
                {
                    m.SaleAmount = ex.SaleAmount;
                    m.Day = ex.Day.Date;
                }
                list.Add(m);
            }
            return list.OrderByDescending(a=>a.Day).ToList();
        }

        /// <summary>
        /// 获取门店在某天的销售排行 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="shopId"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public int GetRank(DateTime date, long shopId, decimal Amount)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select count(1)+1 as rank");
            sb.Append(" from ");
            sb.Append("(select sum(ActualPayAmount) as total from Himall_Orders where  !ISNULL(ShopBranchId) and ShopBranchId>0 and DATE(OrderDate) = @orderdate and ShopId=@shopId GROUP BY ShopBranchId) b");
            sb.Append(" where b.total>@total");
            MySqlParameter[] para = new MySqlParameter[] {
                new MySqlParameter("@orderdate", date),
                new MySqlParameter("@total",Amount),
                new MySqlParameter("@shopId",shopId)
            };
            var result = Context.Database.SqlQuery<int>(sb.ToString(),para).FirstOrDefault();
            return result;
        }


        public QueryPageModel<BranchShopFeat> GetBranchShopFeat(BranchShopFeatsQuery query)
        {
            if (query.StartDate > query.EndDate)
            {
                throw new HimallException("时间段异常：开始时间大于结束时间！");
            }
            var orders = Context.OrderInfo.Where(p => p.ShopId == query.ShopId && p.ShopBranchId.HasValue && p.ShopBranchId.Value!=0);
            orders = orders.Where(p => p.OrderStatus != OrderInfo.OrderOperateStatus.Close);
            if (query.StartDate.HasValue)
            {
                orders = orders.Where(p => p.PayDate >= query.StartDate);
            }
            if (query.EndDate.HasValue)
            {
                orders = orders.Where(p => p.PayDate <= query.EndDate);
            }
            var result = orders.GroupBy(a => a.ShopBranchId).ToList().Select(b => new BranchShopFeat()
            {
                BranchShopId = b.Key.Value,
                ShopId = query.ShopId,
                SaleAmount = b.Sum(x => (decimal?)x.ActualPayAmount).GetValueOrDefault(),
            }).OrderByDescending(x => x.SaleAmount).ToList();
            var index = 0;
            foreach (var t in result)
            {
                index++;
                t.Rank += index;
            }
            int total = 0;
            var datas = result.AsQueryable().GetPage(out total, p => p.OrderBy(o => o.Rank), query.PageNo, query.PageSize);
            QueryPageModel<BranchShopFeat> pageModel = new QueryPageModel<BranchShopFeat>()
            {
                Models = datas.ToList(),
                Total = total
            };
            return pageModel;
        }




        public SaleStatistics GetSaleStatisticsByShop(long shopId, DateTime? startTime, DateTime? endTime)
        {
            if (startTime > endTime)
            {
                throw new HimallException("时间段异常：开始时间大于结束时间！");
            }
            var saleStatistics = new SaleStatistics();
            saleStatistics.DealCount = 0;
            saleStatistics.SaleCount = 0;
            saleStatistics.SalesVolume = 0;

            //临时变量
            int _saleCount = 0;
            int _dealCount = 0;
            decimal _salesVolume = 0;
            decimal _refundRate = 0;
            int _payOrderCount = 0;
            int _refundOrderCount = 0;


            #region 当前数据

            //统计已付款的订单
            var orders = Context.OrderInfo.Where(p => p.ShopId == shopId);
            if (startTime.HasValue)
            {
                orders = orders.Where(p => p.PayDate >= startTime);
            }
            if (endTime.HasValue)
            {
                orders = orders.Where(p => p.PayDate <= endTime);
            }
            //已付款的订单（包括关闭的）用于计算退款率
            var payOrders = orders.Where(p => p.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay).ToList();
            var payOrderIds = payOrders.Select(e => e.Id);
            var dealOrders = payOrders.Where(p => p.OrderStatus != OrderInfo.OrderOperateStatus.Close);
            _payOrderCount += payOrders.Count();
            var orderIds = dealOrders.Select(p => p.Id).ToArray();
            var orderItems = Context.OrderItemInfo.Where(p => orderIds.Contains(p.OrderId));

            _saleCount = (int)orderItems.ToList().Sum(p => (long?)p.Quantity).GetValueOrDefault(0);
            _dealCount = dealOrders.Count();
            _salesVolume = dealOrders.Sum(p => (decimal?)p.ActualPayAmount).GetValueOrDefault(0);

            var orderRefunds = Context.OrderRefundInfo.Where(p => orderIds.Contains(p.OrderId) && p.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed).ToList();
            if (orderRefunds.Count() > 0)
            {
                _saleCount = _saleCount - (int)orderRefunds.Sum(p => (long?)p.ReturnQuantity).GetValueOrDefault(0);
            }

            saleStatistics.DealCount += _dealCount;
            saleStatistics.SaleCount += _saleCount;
            saleStatistics.SalesVolume += _salesVolume;
            saleStatistics.RefundRate += _refundRate;

            #endregion

            //退款率
            //在筛选时间内退款成功的订单
            var refunds = Context.OrderRefundInfo.Where(p => payOrderIds.Contains(p.OrderId) && p.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed);

            if (startTime.HasValue)
            {
                refunds = refunds.Where(p => p.ManagerConfirmDate >= startTime);
            }
            if (endTime.HasValue)
            {
                refunds = refunds.Where(p => p.ManagerConfirmDate <= endTime);
            }
            _refundOrderCount = refunds.Select(p => p.OrderId).Distinct().Count();
            if (_payOrderCount > 0)
            {
                _refundRate = Math.Round((decimal)_refundOrderCount / (decimal)_payOrderCount, 2);
            }
            saleStatistics.RefundRate = _refundRate;

            //计算参数
            if (saleStatistics.DealCount > 0)
            {
                saleStatistics.OrderPrice = saleStatistics.SalesVolume / saleStatistics.DealCount;
                saleStatistics.OrderAverage = (decimal)saleStatistics.SaleCount / saleStatistics.DealCount;
                if (saleStatistics.SaleCount > 0)
                {
                    saleStatistics.OrderItemPrice = saleStatistics.SalesVolume / saleStatistics.SaleCount;
                }
            }

            return saleStatistics;
        }


        #region 会员购买商品类别冗余统计


        /// <summary>
        /// 会员购买商品类别添加
        /// </summary>
        /// <param name="model"></param>
        public void AddMemberBuyCategory(MemberBuyCategoryInfo model)
        {
            Context.MemberBuyCategoryInfo.Add(model);
            Context.SaveChanges();
        }


        /// <summary>
        /// 会员购买商品类别查询
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MemberBuyCategoryInfo GetMemberBuyCategory(long categoryId, long userId)
        {
            return Context.MemberBuyCategoryInfo.Where(p => p.CategoryId == categoryId && p.UserId == userId).FirstOrDefault();
        }

        /// <summary>
        /// 会员购买商品类别删除
        /// </summary>
        /// <param name="id"></param>
        public void DeleteMemberBuyCategory(long id)
        {
            Context.MemberBuyCategoryInfo.Remove(item => item.Id == id);
            Context.SaveChanges();
        }

        /// <summary>
        /// 会员购买商品类别修改
        /// </summary>
        /// <param name="model"></param>
        public void UpdateMemberBuyCategory(MemberBuyCategoryInfo model)
        {
            var m = Context.MemberBuyCategoryInfo.Where(p => p.Id == model.Id).FirstOrDefault();
            m.OrdersCount = model.OrdersCount;
            Context.SaveChanges();
        }
        #endregion



        public SaleStatistics GetBranchShopSaleStatistics(long branchShopId, DateTime? startTime, DateTime? endTime)
        {
            if (startTime > endTime)
            {
                throw new HimallException("时间段异常：开始时间大于结束时间！");
            }
            var saleStatistics = new SaleStatistics();
            saleStatistics.DealCount = 0;
            saleStatistics.SaleCount = 0;
            saleStatistics.SalesVolume = 0;

            //临时变量
            int _saleCount = 0;
            int _dealCount = 0;
            decimal _salesVolume = 0;
            decimal _refundRate = 0;
            int _payOrderCount = 0;
            int _refundOrderCount = 0;


            #region 当前数据

            //统计已付款的订单
            var orders = Context.OrderInfo.Where(p => p.ShopBranchId == branchShopId);
            if (startTime.HasValue)
            {
                orders = orders.Where(p => p.PayDate >= startTime);
            }
            if (endTime.HasValue)
            {
                orders = orders.Where(p => p.PayDate <= endTime);
            }
            //已付款的订单（包括关闭的）用于计算退款率
            var payOrders = orders.Where(p => p.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay).ToList() ;
            var payOrderIds = payOrders.Select(e => e.Id);
            var dealOrders = payOrders.Where(p => p.OrderStatus != OrderInfo.OrderOperateStatus.Close);
            var orderIds = dealOrders.Select(p => p.Id).ToArray();
            _payOrderCount += payOrders.Count();
            var orderItems = Context.OrderItemInfo.Where(p => orderIds.Contains(p.OrderId));

            _saleCount = (int)orderItems.ToList().Sum(p => (long?)p.Quantity).GetValueOrDefault(0);
            _dealCount = dealOrders.Count();
            _salesVolume = dealOrders.Sum(p => (decimal?)p.ActualPayAmount).GetValueOrDefault(0);

            var orderRefunds = Context.OrderRefundInfo.Where(p => orderIds.Contains(p.OrderId) && p.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed).ToList();
            if (orderRefunds.Count() > 0)
            {
                _saleCount = _saleCount - (int)orderRefunds.Sum(p => (long?)p.ReturnQuantity).GetValueOrDefault(0);
            }

            saleStatistics.DealCount += _dealCount;
            saleStatistics.SaleCount += _saleCount;
            saleStatistics.SalesVolume += _salesVolume;
            saleStatistics.RefundRate += _refundRate;

            #endregion

            //退款率
            //在筛选时间内退款成功的订单
            var refunds = Context.OrderRefundInfo.Where(p => payOrderIds.Contains(p.OrderId)&&p.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed);

            if (startTime.HasValue)
            {
                refunds = refunds.Where(p => p.ManagerConfirmDate >= startTime);
            }
            if (endTime.HasValue)
            {
                refunds = refunds.Where(p => p.ManagerConfirmDate <= endTime);
            }
            _refundOrderCount = refunds.Select(p => p.OrderId).Distinct().Count();
            if (_payOrderCount > 0)
            {
                _refundRate = Math.Round((decimal)_refundOrderCount / (decimal)_payOrderCount, 2);
            }
            saleStatistics.RefundRate = _refundRate;

            //计算参数
            if (saleStatistics.DealCount > 0)
            {
                saleStatistics.OrderPrice = saleStatistics.SalesVolume / saleStatistics.DealCount;
                saleStatistics.OrderAverage = (decimal)saleStatistics.SaleCount / saleStatistics.DealCount;
                if (saleStatistics.SaleCount > 0)
                {
                    saleStatistics.OrderItemPrice = saleStatistics.SalesVolume / saleStatistics.SaleCount;
                }
            }

            return saleStatistics;
        }
    }
}
