using System;
using System.Collections.Generic;
using System.Linq;
using Himall.CommonModel;
using Himall.CommonModel.QueryModel;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
namespace Himall.Service
{
    /// <summary>
    /// 结算的数据层实现
    /// </summary>
    public class BillingService : ServiceBase, IBillingService
    {
        #region 字段
        private static object obj = new object();
        #endregion

        #region 方法
        /// <summary>
        /// 店铺帐户（在入驻成功后建立一个帐户）
        /// </summary>
        /// <param name="model"></param>
        public void AddShopAccount(ShopAccountInfo model)
        {
            if (!Context.ShopAccountInfo.Any(a => a.ShopId == model.ShopId))
            {
                Context.ShopAccountInfo.Add(model);
                Context.SaveChanges();
            }
        }

        /// <summary>
        /// 更新店铺资金信息（结算时，退款时，充值时）
        /// </summary>
        /// <param name="model"></param>
        public void UpdateShopAccount(ShopAccountInfo model)
        {
            Context.ShopAccountInfo.Attach(model);
            Context.Entry(model).State = System.Data.Entity.EntityState.Modified;
            Context.SaveChanges();
        }

        /// <summary>
        /// 根据店铺ID获取店铺帐户信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ShopAccountInfo GetShopAccount(long shopId)
        {
            var model = Context.ShopAccountInfo.FirstOrDefault(a => a.ShopId == shopId);
            //待结算金额总在变化改为实时查询吧20160709
            var PendingSettlementAmount = Context.PendingSettlementOrdersInfo.Where(a => a.ShopId == shopId).Sum(a => (decimal?)a.SettlementAmount).GetValueOrDefault();
            model.PendingSettlement = PendingSettlementAmount;
            return model;
        }

        /// <summary>
        /// 店铺流水
        /// </summary>
        /// <param name="item"></param>
        public void AddShopAccountItem(ShopAccountItemInfo model)
        {
            Context.ShopAccountItemInfo.Add(model);
            Context.SaveChanges();
        }

        /// <summary>
        /// 分页获取店铺流水
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<ShopAccountItemInfo> GetShopAccountItem(ShopAccountItemQuery query)
        {
            int total = 0;
            var itemQuery = ToWhere(query);
            itemQuery = itemQuery.GetPage(out total, d => d.OrderByDescending(o => o.Id), query.PageNo, query.PageSize);
            return new QueryPageModel<ShopAccountItemInfo>() { Models = itemQuery.ToList(), Total = total };
        }

        /// <summary>
        /// 获取店铺流水
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<ShopAccountItemInfo> GetShopAccountItemNoPage(ShopAccountItemQuery query)
        {
            var itemQuery = ToWhere(query);
            return itemQuery.ToList();
        }

        /// <summary>
        /// 分页获取平台流水
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<PlatAccountItemInfo> GetPlatAccountItem(PlatAccountItemQuery query)
        {
            int total = 0;
            var ItemQuery = ToWhere(query);
            ItemQuery = ItemQuery.GetPage(out total, d => d.OrderByDescending(o => o.Id), query.PageNo, query.PageSize);
            QueryPageModel<PlatAccountItemInfo> pageModel = new QueryPageModel<PlatAccountItemInfo>() { Models = ItemQuery.ToList(), Total = total };
            return pageModel;
        }

        /// <summary>
        /// 分页获取平台流水
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<PlatAccountItemInfo> GetPlatAccountItemNoPage(PlatAccountItemQuery query)
        {
            var itemQuery = ToWhere(query);
            return itemQuery.OrderByDescending(p => p.Id).ToList();
        }

        /// <summary>
        /// 店铺提现申请
        /// </summary>
        /// <param name="item"></param>
        public void AddShopWithDrawInfo(ShopWithDrawInfo info)
        {
            Context.ShopWithDrawInfo.Add(info);
            Context.SaveChanges();
        }

        /// <summary>
        /// 获取店铺提现详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ShopWithDrawInfo GetShopWithDrawInfo(long Id)
        {
            return Context.ShopWithDrawInfo.FirstOrDefault(a => a.Id == Id);
        }

        public LineChartDataModel<decimal> GetTradeChart(DateTime start, DateTime end, long? shopId)
        {
            LineChartDataModel<decimal> chart = new LineChartDataModel<decimal>() { SeriesData = new List<ChartSeries<decimal>>() };

            var data = Context.OrderInfo.Where(a => a.PayDate.HasValue && a.PayDate >= start && a.PayDate <= end);
            if (shopId.HasValue && shopId.Value > 0)
            {
                data = data.Where(a => a.ShopId == shopId);
            }
            var list = data.Select(a => new { Amount = (a.ProductTotalAmount + a.Freight + a.Tax - a.DiscountAmount), PayDate = a.PayDate }).ToList();

            var days = (end - start).Days; //相差的天数

            string[] arr = new string[days + 1];
            for (int i = 0; i <= days; i++)
            {
                arr[i] = start.Date.AddDays(i).ToString("MM/dd");
            }

            chart.XAxisData = arr;

            var arrEx = new string[days + 1];

            var chartItem = new ChartSeries<decimal> { Name = "交易额走势图", Data = new decimal[days + 1] };

            for (int i = 0; i <= days; i++)
            {
                var date = start.Date.AddDays(i).Date;

                var m = list.Where(a => a.PayDate.Value.Date == date).ToList();
                if (m.Count > 0)
                {
                    chartItem.Data[i] = m.Sum(a => a.Amount);
                    arrEx[i] = date + "的销售额为:" + chartItem.Data[i];
                }
                else
                {
                    chartItem.Data[i] = 0;
                    arrEx[i] = date + "的销售额为:" + chartItem.Data[i];
                }
            }
            chart.ExpandProp = arrEx;
            chart.SeriesData.Add(chartItem);

            return chart;
        }

        public LineChartDataModel<decimal> GetTradeChartMonth(DateTime start, DateTime end, long? shopId)
        {
            LineChartDataModel<decimal> chart = new LineChartDataModel<decimal>() { SeriesData = new List<ChartSeries<decimal>>() };

            var data = Context.OrderInfo.Where(a => a.PayDate.HasValue && a.PayDate >= start && a.PayDate < end);
            if (shopId.HasValue && shopId.Value > 0)
            {
                data = data.Where(a => a.ShopId == shopId);
            }
            var list = data.Select(a => new { Amount = (a.ProductTotalAmount + a.Freight + a.Tax - a.DiscountAmount), PayDate = a.PayDate }).ToList();

            int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);//本月天数
            string[] arr = new string[days];
            for (int i = 0; i <= days - 1; i++)
            {
                arr[i] = start.Date.AddDays(i).ToString("MM/dd");
            }

            chart.XAxisData = arr;

            var chartItem = new ChartSeries<decimal> { Name = "交易额走势图", Data = new decimal[days + 1] };
            var ExpandProp = new string[days + 1];

            for (int i = 0; i <= days; i++)
            {
                var date = start.Date.AddDays(i).Date;

                var m = list.Where(a => a.PayDate.Value.Date == date).ToList();
                if (m.Count > 0)
                {
                    chartItem.Data[i] = m.Sum(a => a.Amount);
                }
                else
                {
                    chartItem.Data[i] = 0;
                }
            }
            chart.SeriesData.Add(chartItem);

            return chart;
        }

        /// <summary>
        /// 更新提现数据
        /// </summary>
        /// <param name="model"></param>
        public void UpdateShopWithDraw(ShopWithDrawInfo model)
        {
            Context.ShopWithDrawInfo.Attach(model);
            Context.Entry(model).State = System.Data.Entity.EntityState.Modified;
            Context.SaveChanges();
        }

        /// <summary>
        /// 分页获取店铺提现记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<ShopWithDrawInfo> GetShopWithDraw(WithdrawQuery query)
        {
            int total = 0;
            var pendingQuery = ToWhere(query);
            pendingQuery = pendingQuery.GetPage(out total, d => d.OrderByDescending(o => o.Id), query.PageNo, query.PageSize);
            QueryPageModel<ShopWithDrawInfo> pageModel = new QueryPageModel<ShopWithDrawInfo>() { Models = pendingQuery.ToList(), Total = total };
            return pageModel;
        }

        /// <summary>
        /// 获取店铺提现记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<ShopWithDrawInfo> GetShopWithDrawNoPage(WithdrawQuery query)
        {
            var pendingQuery = ToWhere(query);
            return pendingQuery.ToList();
        }

        /// <summary>
        /// 添加待结算订单
        /// </summary>
        /// <param name="model"></param>
        public void AddPendingSettlementOrders(PendingSettlementOrdersInfo model)
        {
            Context.PendingSettlementOrdersInfo.Add(model);
            Context.SaveChanges();
        }

        /// <summary>
        /// 获取待结算订单详情
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public PendingSettlementOrdersInfo GetPendingSettlementOrderDetail(long orderId)
        {
            var model = Context.PendingSettlementOrdersInfo.Where(a => a.OrderId == orderId).FirstOrDefault();
            return model;
        }

        /// <summary>
        /// 已结算详情
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public AccountDetailInfo GetSettlementOrderDetail(long orderId)
        {
            var model = Context.AccountDetailInfo.Where(a => a.OrderId == orderId).FirstOrDefault();
            return model;

        }

        /// <summary>
        /// 获取上次结算的结束日期
        /// </summary>
        /// <returns></returns>
        public DateTime? GetLastSettlementTime()
        {
            DateTime? last = null;
            //已结算表查找最后一次结算记录
            var settlementOrder = Context.AccountInfo.OrderByDescending(a => a.Id).FirstOrDefault();
            if (settlementOrder != null)
            {
                last = settlementOrder.AccountDate.Date;
            }
            else
            {
                var order = Context.OrderInfo.Where(a => a.OrderStatus == OrderInfo.OrderOperateStatus.Finish).FirstOrDefault();
                if (order != null)
                {
                    last = order.FinishDate.Value.Date;
                }
            }
            return last;
        }

        /// <summary>
        /// 分页获取待结算订单
        /// </summary>
        /// <param name="query">待结算订单查询实体</param>
        /// <returns></returns>
        public QueryPageModel<PendingSettlementOrdersInfo> GetPendingSettlementOrders(PendingSettlementOrderQuery query)
        {
            int total = 0;
            var pendingQuery = ToWhere(query);
            pendingQuery = pendingQuery.GetPage(out total, d => d.OrderByDescending(o => o.Id), query.PageNo, query.PageSize);
            return new QueryPageModel<PendingSettlementOrdersInfo>() { Models = pendingQuery.ToList(), Total = total };
        }

        /// <summary>
        /// 统计待结算订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<StatisticsPendingSettlement> StatisticsPendingSettlementOrders(StatisticsPendingSettlementQuery query)
        {
            var data = this.Context.PendingSettlementOrdersInfo.AsQueryable();

            if (!string.IsNullOrEmpty(query.ShopName))
            {
                data = data.Where(p => p.ShopName.Contains(query.ShopName));
            }

            var result = new QueryPageModel<StatisticsPendingSettlement>();
            result.Total = data.GroupBy(p => p.ShopId).Count();
            var models = data.GroupBy(p => p.ShopId).Select(p => new StatisticsPendingSettlement
            {
                ShopId = p.Key,
                Amount = p.Sum(pp => pp.SettlementAmount)
            }).OrderByDescending(p => p.ShopId).Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize);
            result.Models = models.ToList();

            return result;
        }

        /// <summary>
        /// 统计待结算订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<StatisticsPendingSettlement> StatisticsPendingSettlementOrdersNoPage(StatisticsPendingSettlementQuery query)
        {
            var data = this.Context.PendingSettlementOrdersInfo.AsQueryable();

            if (!string.IsNullOrEmpty(query.ShopName))
            {
                data = data.Where(p => p.ShopName.Contains(query.ShopName));
            }

            var models = data.GroupBy(p => p.ShopId).Select(p => new StatisticsPendingSettlement
            {
                ShopId = p.Key,
                Amount = p.Sum(pp => pp.SettlementAmount)
            });
            return models.ToList();
        }

        /// <summary>
        /// 获取待结算订单
        /// </summary>
        /// <param name="query">待结算订单查询实体</param>
        /// <returns></returns>
        public List<PendingSettlementOrdersInfo> GetPendingSettlementOrdersNoPage(PendingSettlementOrderQuery query)
        {
            var pendingQuery = ToWhere(query);
            return pendingQuery.ToList();
        }

        /// <summary>
        /// 分页获取已结算订单
        /// </summary>
        /// <param name="query">结算订单查询实体</param>
        /// <returns></returns>
        public QueryPageModel<AccountDetailInfo> GetSettlementOrders(SettlementOrderQuery query)
        {
            int total = 0;
            var accountDetailQuery = ToWhere(query);
            accountDetailQuery = accountDetailQuery.GetPage(out total, d => d.OrderByDescending(o => o.Id), query.PageNo, query.PageSize);
            QueryPageModel<AccountDetailInfo> pageModel = new QueryPageModel<AccountDetailInfo>() { Models = accountDetailQuery.ToList(), Total = total };
            return pageModel;
        }

        /// <summary>
        /// 获取已结算订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<AccountDetailInfo> GetSettlementOrdersNoPage(SettlementOrderQuery query)
        {
            var accountDetailQuery = ToWhere(query);
            return accountDetailQuery.ToList();
        }

        public void Settle()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 处理余额私有方法
        /// </summary>
        /// <param name="shopId">店铺ID</param>
        /// <param name="money">金额</param>
        /// <param name="TradeType">类别</param>
        /// <param name="AccountNo">交易流水号</param>
        /// <param name="ChargeWay">备注</param>
        /// <param name="AccoutID">关联资金编号</param>
        public void UpdateAccount(long shopId, decimal money, Himall.CommonModel.ShopAccountType TradeType, string AccountNo, string ChargeWay, long detailID = 0)
        {
            lock (obj)
            {
                //处理余额
                var mShopAccountInfo = GetShopAccount(shopId);
                mShopAccountInfo.Balance += money;
                UpdateShopAccount(mShopAccountInfo);
                var isincome = true;
                if (TradeType == ShopAccountType.Refund || TradeType == ShopAccountType.MarketingServices || TradeType == ShopAccountType.WithDraw)
                {
                    isincome = false;
                }
                //处理充值记录
                ShopAccountItemInfo mShopAccountItemInfo = new ShopAccountItemInfo()
                {
                    AccountNo = AccountNo,
                    AccoutID = mShopAccountInfo.Id,
                    Amount = Math.Abs(money),
                    Balance = mShopAccountInfo.Balance,
                    CreateTime = DateTime.Now,
                    DetailId = detailID.ToString(),
                    IsIncome = isincome,
                    ReMark = ChargeWay,
                    ShopId = shopId,
                    ShopName = mShopAccountInfo.ShopName,
                    TradeType = TradeType
                };

                ///平台佣金退还
                if (TradeType == ShopAccountType.PlatCommissionRefund)
                {
                    var platAccount = GetPlatAccount();
                    platAccount.Balance -= money;
                    UpdatePlatAccount(platAccount);
                    PlatAccountItemInfo PlatAccountItemInfo = new PlatAccountItemInfo()
                    {
                        AccountNo = AccountNo,
                        AccoutID = platAccount.Id,
                        Amount = Math.Abs(money),
                        Balance = platAccount.Balance,
                        CreateTime = DateTime.Now,
                        DetailId = detailID.ToString(),
                        IsIncome = false,
                        ReMark = ChargeWay,
                        TradeType = PlatAccountType.PlatCommissionRefund
                    };
                    AddPlatAccountItem(PlatAccountItemInfo);
                }
                AddShopAccountItem(mShopAccountItemInfo);
            }
        }

        public void UpdatePlatAccount(PlatAccountInfo model)
        {
            Context.PlatAccountInfo.Attach(model);
            Context.Entry(model).State = System.Data.Entity.EntityState.Modified;
            Context.SaveChanges();
        }

        /// <summary>
        /// 获取平台佣金总额
        /// </summary>
        /// <param name="shopId">店铺ID选填</param>
        /// <returns></returns>
        public decimal GetPlatCommission(long? shopId = null, long? accountId = null)
        {
            var model = Context.AccountDetailInfo.AsQueryable();
            if (shopId.HasValue)
                model = model.Where(a => a.ShopId == shopId);
            if (accountId.HasValue)
                model = model.Where(p => p.AccountId == accountId.Value);
            decimal amount = 0;
            amount = model.Sum(a => (decimal?)a.CommissionAmount - a.RefundCommisAmount).GetValueOrDefault();
            return amount;
        }

        /// <summary>
        /// 获取分销佣金总额
        /// </summary>
        /// <param name="shopId">店铺ID选填</param>
        /// <returns></returns>

        public decimal GetDistributorCommission(long? shopId = null, long? accountId = null)
        {
            var model = Context.AccountDetailInfo.AsQueryable();
            if (shopId.HasValue)
                model = model.Where(a => a.ShopId == shopId);
            if (accountId.HasValue)
                model = model.Where(p => p.AccountId == accountId.Value);
            decimal amount = 0;
            amount = model.Sum(a => (decimal?)a.BrokerageAmount - a.ReturnBrokerageAmount).GetValueOrDefault();
            return amount;
        }

        /// <summary>
        /// 获取结算总金额
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public decimal GetSettlementAmount(long? shopId = null, long? accountId = null)
        {
            var model = Context.AccountDetailInfo.AsQueryable();
            if (shopId.HasValue)
                model = model.Where(a => a.ShopId == shopId);
            if (accountId.HasValue)
                model = model.Where(p => p.AccountId == accountId.Value);
            decimal amount = 0;
            amount = model.Sum(a => (decimal?)a.SettlementAmount).GetValueOrDefault();
            return amount;
        }

        /// <summary>
        /// 获取平台待结算佣金总和(平台待结算页面汇总)
        /// </summary>
        /// <returns></returns>
        public decimal GetPendingPlatCommission()
        {
            var pendingPlatCommission = Context.PendingSettlementOrdersInfo.Sum(a => (decimal?)a.PlatCommission - a.PlatCommissionReturn).GetValueOrDefault();

            return pendingPlatCommission;
        }

        /// <summary>
        /// 获取平台帐户信息
        /// </summary>
        /// <returns></returns>
        public PlatAccountInfo GetPlatAccount()
        {
            var model = Context.PlatAccountInfo.FirstOrDefault();
            //待结算金额总在变化改为实时查询吧20160709
            var PendingSettlementAmount = Context.PendingSettlementOrdersInfo.Sum(a => (decimal?)a.SettlementAmount).GetValueOrDefault();
            model.PendingSettlement = PendingSettlementAmount;
            return model;
        }

        /// <summary>
        /// 添加平台流水
        /// </summary>
        /// <param name="model"></param>
        public void AddPlatAccountItem(PlatAccountItemInfo model)
        {
            Context.PlatAccountItemInfo.Add(model);
            Context.SaveChanges();
        }
        #endregion

        #region 私有方法
        private IQueryable<AccountDetailInfo> ToWhere(SettlementOrderQuery query)
        {
            var accountDetailQuery = Context.AccountDetailInfo.AsQueryable();
            if (query.ShopId.HasValue)
            {
                accountDetailQuery = accountDetailQuery.Where(a => a.ShopId == query.ShopId.Value);
            }
            if (query.OrderId.HasValue)
            {
                accountDetailQuery = accountDetailQuery.Where(a => a.OrderId == query.OrderId.Value);
            }
            if (query.OrderStart.HasValue)
            {
                accountDetailQuery = accountDetailQuery.Where(a => a.OrderFinshDate >= query.OrderStart.Value);
            }
            if (query.OrderEnd.HasValue)
            {
                var end = query.OrderEnd.Value.Date.AddDays(1);
                accountDetailQuery = accountDetailQuery.Where(a => a.OrderFinshDate < end);
            }
            if (query.SettleStart.HasValue)
            {
                accountDetailQuery = accountDetailQuery.Where(a => a.Date >= query.SettleStart.Value);
            }
            if (query.SettleEnd.HasValue)
            {
                var end = query.SettleEnd.Value.Date.AddDays(1);
                accountDetailQuery = accountDetailQuery.Where(a => a.Date < end);
            }

            if (!string.IsNullOrEmpty(query.ShopName))
            {
                accountDetailQuery = accountDetailQuery.Where(a => a.ShopName.Contains(query.ShopName));
            }
            if (!string.IsNullOrEmpty(query.PaymentName))
            {
                accountDetailQuery = accountDetailQuery.Where(a => a.PaymentTypeName == query.PaymentName);
            }
            if (query.WeekSettlementId.HasValue && query.WeekSettlementId.Value != 0)
            {
                var detailId = query.WeekSettlementId.Value;
                accountDetailQuery = accountDetailQuery.Where(a => a.AccountId == detailId);
            }
            return accountDetailQuery;
        }

        private IQueryable<PendingSettlementOrdersInfo> ToWhere(PendingSettlementOrderQuery query)
        {

            var pendingQuery = Context.PendingSettlementOrdersInfo.AsQueryable();
            if (query.ShopId.HasValue)
            {
                pendingQuery = pendingQuery.Where(a => a.ShopId == query.ShopId.Value);
            }
            if (query.OrderId.HasValue)
            {
                pendingQuery = pendingQuery.Where(a => a.OrderId == query.OrderId.Value);
            }
            if (query.OrderStart.HasValue)
            {
                pendingQuery = pendingQuery.Where(a => a.OrderFinshTime >= query.OrderStart.Value);
            }
            if (query.OrderEnd.HasValue)
            {
                var end = query.OrderEnd.Value.Date.AddDays(1);
                pendingQuery = pendingQuery.Where(a => a.OrderFinshTime <end);
            }
            if (!string.IsNullOrEmpty(query.ShopName))
            {
                pendingQuery = pendingQuery.Where(a => a.ShopName.Contains(query.ShopName));
            }
            if (query.ShopId.HasValue)
                pendingQuery = pendingQuery.Where(p => p.ShopId == query.ShopId.Value);
            if (!string.IsNullOrEmpty(query.PaymentName))
            {
                pendingQuery = pendingQuery.Where(a => a.PaymentTypeName == query.PaymentName);
            }
            return pendingQuery;
        }

        private IQueryable<ShopAccountItemInfo> ToWhere(ShopAccountItemQuery query)
        {
            var itemQuery = Context.ShopAccountItemInfo.AsQueryable();
            if (query.ShopId.HasValue)
            {
                itemQuery = itemQuery.Where(a => a.ShopId == query.ShopId.Value);
            }

            if (!string.IsNullOrEmpty(query.ShopName))
                itemQuery = itemQuery.Where(p => p.ShopName.Contains(query.ShopName));

            if(query.IsIncome.HasValue)
            {
                itemQuery = itemQuery.Where(a=>a.IsIncome==query.IsIncome.Value);
            }

            if (query.ShopAccountType.HasValue)
            {
                itemQuery = itemQuery.Where(a => a.TradeType == query.ShopAccountType.Value);
            }
            if (query.TimeStart.HasValue)
            {
                itemQuery = itemQuery.Where(a => a.CreateTime >= query.TimeStart.Value);
            }
            if (query.TimeEnd.HasValue)
            {
                var end = query.TimeEnd.Value.Date.AddDays(1);
                itemQuery = itemQuery.Where(a => a.CreateTime < end);
            }
            return itemQuery;
        }

        private IQueryable<ShopWithDrawInfo> ToWhere(WithdrawQuery query)
        {
            var pendingQuery = Context.ShopWithDrawInfo.AsQueryable();
         
            if (query.ApplyStartTime.HasValue)
            {
                pendingQuery = pendingQuery.Where(a => a.ApplyTime >= query.ApplyStartTime.Value);
            }
            if (query.ApplyEndTime.HasValue)
            {
                var end = query.ApplyEndTime.Value.Date.AddDays(1);
                pendingQuery = pendingQuery.Where(a => a.ApplyTime < end);
            }
            if (query.AuditedStartTime.HasValue)
            {
                pendingQuery = pendingQuery.Where(a => a.DealTime >= query.AuditedStartTime.Value);
            }
            if (query.AuditedEndTime.HasValue)
            {
                var end = query.AuditedEndTime.Value.Date.AddDays(1);
                pendingQuery = pendingQuery.Where(a => a.DealTime < end);
            }
           
            if (query.ShopId.HasValue)
            {
                pendingQuery = pendingQuery.Where(a => a.ShopId == query.ShopId.Value);
            }
            if (!string.IsNullOrEmpty(query.ShopName))
            {
                pendingQuery = pendingQuery.Where(a => a.ShopName.Contains(query.ShopName));
            }
            if (query.Status.HasValue)
            {
                pendingQuery = pendingQuery.Where(a => a.Status == query.Status.Value);
            }
            if (query.Id.HasValue && query.Id.Value != 0)
            {
                pendingQuery = pendingQuery.Where(a => a.Id == query.Id.Value);
            }
            return pendingQuery;
        }

        private IQueryable<PlatAccountItemInfo> ToWhere(PlatAccountItemQuery query)
        {
            IQueryable<PlatAccountItemInfo> ItemQuery = Context.PlatAccountItemInfo.AsQueryable();
            if (query.PlatAccountType.HasValue)
            {
                ItemQuery = ItemQuery.Where(a => a.TradeType == query.PlatAccountType.Value);
            }
            if (query.TimeStart.HasValue)
            {
                ItemQuery = ItemQuery.Where(a => a.CreateTime >= query.TimeStart.Value);
            }
            if (query.TimeEnd.HasValue)
            {
                var end = query.TimeEnd.Value.Date.AddDays(1);
                ItemQuery = ItemQuery.Where(a => a.CreateTime < end);
            }
            return ItemQuery;
        }

        public decimal GetLastSettlementByShopId(long shopId)
        {
            //var lastId = Context.AccountInfo.OrderByDescending(a => a.Id).Select(a => a.Id).FirstOrDefault();
            //if (lastId == 0) { return 0; }
            //var acc = Context.AccountDetailInfo.Where(a => a.ShopId == shopId && a.AccountId == lastId).Sum(a => (decimal?)a.SettlementAmount).GetValueOrDefault();
            //return acc;
            var last = Context.ShopAccountItemInfo.Where(a => a.TradeType == ShopAccountType.SettlementIncome&&a.ShopId==shopId).OrderByDescending(a => a.Id).FirstOrDefault();
            if (last == null)
                return 0;
            else
                return last.Amount;
        }

        /// <summary>
        /// 获取某个店铺某个结算周期的结算总金额
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public decimal GetShopSettledAmountByAccountId(long shopId, long accountId)
        {
            var SettlementAmount = Context.AccountDetailInfo.Where(a => a.ShopId == shopId &&a.AccountId==accountId).Sum(a=>(decimal?)a.SettlementAmount).GetValueOrDefault();
            return SettlementAmount;
        }


        /// <summary>
        /// 获取上一次结算的基本信息
        /// </summary>
        /// <returns></returns>
        public AccountInfo GetLastAccountInfo()
        {
           var model=  Context.AccountInfo.OrderByDescending(a => a.Id).FirstOrDefault();

            if (model == null)
                model = new AccountInfo();
            return model;
        }

        /// <summary>
        /// 查询结算历史
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<AccountInfo> GetSettledHistory(ShopSettledHistoryQuery query)
        {
            int total = 0;
            var model = Context.AccountDetailInfo.Where(a => a.ShopId == query.ShopId).OrderBy(a=>a.Id).FirstOrDefault();
            var MinDate = query.MinSettleTime.Date;
            if (model == null) //不存在结算记录返回空
                return new QueryPageModel<AccountInfo>() { Models = new List<AccountInfo>(),Total=0 };
            else
            {
                MinDate = model.Date.Date;
            }
            var list =  Context.AccountInfo.Where(a => a.AccountDate>= MinDate) ;
            var result= list.GetPage(out total, d => d.OrderByDescending(o => o.Id), query.PageNo, query.PageSize);
            QueryPageModel<AccountInfo> pageModel = new QueryPageModel<AccountInfo>() { Models = result.ToList(), Total = total };
            return pageModel;
        }





        /// <summary>
        /// 获取某个店铺某个结算周期的结算总金额
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public decimal GetShopTodaySettledAmountByAccountId(long shopId, long accountId)
        {
            DateTime nowbegin = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
            DateTime nowend = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");
            if (accountId == 0)
            {
                return Context.AccountDetailInfo.Where(a => a.ShopId == shopId && a.Date >= nowbegin && a.Date <= nowend).Sum(a => (decimal?)a.SettlementAmount).GetValueOrDefault();
            }
            else
            {
                return Context.AccountDetailInfo.Where(a => a.ShopId == shopId && a.AccountId == accountId && a.Date >= nowbegin && a.Date <= nowend).Sum(a => (decimal?)a.SettlementAmount).GetValueOrDefault();
            }
        }
        /// <summary>
        /// 获取某个店铺某个结算周期的结算总金额
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public decimal GetShopTotalSettledAmountByAccountId(long shopId, long accountId)
        {
            if (accountId == 0)
            {
                return Context.AccountDetailInfo.Where(a => a.ShopId == shopId).Sum(a => (decimal?)a.SettlementAmount).GetValueOrDefault();
            }
            else
            {
                return Context.AccountDetailInfo.Where(a => a.ShopId == shopId && a.AccountId == accountId).Sum(a => (decimal?)a.SettlementAmount).GetValueOrDefault();
            }
        }
        #endregion
    }
}


