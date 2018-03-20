using Quartz;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;
using Himall.Core;
using Himall.IServices;
using System.Transactions;
using System.Data.Entity.SqlServer;
using Himall.Model;
using Himall.CommonModel;

namespace Himall.Service.Job
{
    public class AccountJob : IJob
    {
        /// <summary>
        /// 计算总佣金
        /// </summary>
        /// <param name="orderItems"></param>
        /// <returns></returns>
        private decimal CalculationTotalCommission(IList<Himall.Model.OrderItemInfo> orderItems)
        {
            decimal commission = decimal.Zero;
            commission = orderItems.Sum(c => (c.RealTotalPrice - c.CouponDiscount) * c.CommisRate);//RealTotalPrice(应付)*比率
            return commission;
        }

        /// <summary>
        /// 计算退货佣金
        /// </summary>
        /// <param name="orderItems"></param>
        /// <returns></returns>
        private decimal CalculationTotalRefundCommission(IList<Himall.Model.OrderItemInfo> orderItems)
        {
            decimal commission = decimal.Zero;
            commission = orderItems.Sum(c => c.RefundPrice * c.CommisRate);
            return commission;
        }


        /// <summary>
        /// 计算分销总佣金
        /// </summary>
        /// <param name="orderItems"></param>
        /// <returns></returns>
        private decimal CalculationTotalBrokerage(IList<Himall.Model.OrderItemInfo> orderItems)
        {
            decimal refundBrokerage = decimal.Zero;

            var items = orderItems.Where(a => a.DistributionRate.HasValue).ToList();

            if (items != null && items.Count > 0)
            {
                refundBrokerage = items.Sum(a => (a.RealTotalPrice - a.CouponDiscount) * a.DistributionRate.Value / 100);
            }
            return refundBrokerage;
        }


        /// <summary>
        /// 计算退货分销佣金
        /// </summary>
        /// <param name="orderItems"></param>
        /// <returns></returns>
        private decimal CalculationTotalRefundBrokerage(IList<Himall.Model.OrderRefundInfo> orderItems)
        {
            decimal refundBrokerage = decimal.Zero;
            refundBrokerage = orderItems.Sum(a => a.ReturnBrokerage);
            return refundBrokerage;
        }
        #region 原来结算
        //public void Execute(IJobExecutionContext context)
        //{
        //    try
        //    {
        //        Entity.Entities entity = new Entities();
        //        var checkDate = DateTime.MinValue;
        //        Log.Debug("AccountJob : start");
        //        var settings = new SiteSettingService().GetSiteSettings();
        //        if (settings.WeekSettlement < 1)
        //        {
        //            Log.Error("结算周期设置不正确！ " );
        //            return;
        //        }
        //        var account = entity.AccountInfo.OrderByDescending(a => a.Id).FirstOrDefault();
        //        //最后一个结算数据
        //        // var account = new AccountService().GetAccounts(new IServices.QueryModel.AccountQuery { PageNo = 1, PageSize = int.MaxValue }).Models.OrderByDescending(c => c.Id).FirstOrDefault();
        //        if (account == null)
        //        {
        //            //第一笔订单或者采购协议的日期
        //            checkDate = GetCheckDate(entity, checkDate).Date;
        //        }
        //        else
        //        {
        //            checkDate = account.EndDate.Date.AddDays(1);//上一次结束日期（2015-11-23 23:59:59），作为开始时间
        //        }
        //        if (checkDate.Equals(DateTime.MinValue))
        //            return;

        //        DateTime startDate = checkDate.Date;
        //        DateTime endDate = startDate.AddDays(settings.WeekSettlement).AddMilliseconds(-1);
        //        while (endDate < DateTime.Now.Date)
        //        {
        //            //结算日期
        //            CalculationMoney(startDate, endDate);
        //            account = entity.AccountInfo.Where(c => c.StartDate >= startDate).OrderByDescending(c => c.EndDate).FirstOrDefault();
        //            if (account != null)
        //            {
        //                checkDate = account.EndDate.Date.AddDays(1);//上一次结束日期（2015-11-23 23:59:59），作为开始时间
        //            }
        //            else
        //            {
        //                checkDate = startDate.AddDays(settings.WeekSettlement);
        //            }
        //            startDate = checkDate.Date;
        //            endDate = startDate.AddDays(settings.WeekSettlement).AddMilliseconds(-1);
        //        }

        //    }
        //    catch(Exception ex)
        //    {
        //        Log.Debug("AccountJob : " + ex.Message);
        //    }

        //}
        #endregion
        public void Execute(IJobExecutionContext context)
        {
            #region 结算

            //Entity.Entities entity = new Entities();
            ////using (TransactionScope transaction = new TransactionScope())
            ////{
            //try
            //{
            //    var checkDate = DateTime.MinValue;
            //    Log.Debug("AccountJob : start");
            //    var settings = new SiteSettingService().GetSiteSettings();
            //    if (settings.WeekSettlement < 1)
            //    {
            //        Log.Error("结算周期设置不正确！ ");
            //        return;
            //    }
            //    var account = entity.AccountInfo.OrderByDescending(a => a.Id).FirstOrDefault();
            //    if (account == null)
            //    {
            //        //第一笔结算数据
            //        checkDate = GetDate(entity, checkDate).Date;
            //    }
            //    else
            //    {
            //        checkDate = account.EndDate.Date;//上一次结束日期（2015-11-23 00:00:00），作为开始时间
            //    }
            //    if (checkDate.Equals(DateTime.MinValue))
            //        return;

            //    DateTime startDate = checkDate.Date;
            //    DateTime endDate = startDate.AddDays(settings.WeekSettlement);
            //    Log.Debug("AccountJob:endDate" + endDate + "DateTime:" + DateTime.Now.Date + "result:" + (endDate < DateTime.Now.Date));
            //    while (endDate < DateTime.Now)
            //    {
            //        //结算日期内的待结算订单 不计算开始时间，防止漏单
            //        var pendingSetllementData = entity.PendingSettlementOrdersInfo.Where(c => c.OrderFinshTime < endDate).OrderByDescending(c => c.OrderFinshTime).ToList();
            //        Log.Debug("Count:" + pendingSetllementData.Count());
            //        var accountInfo = new AccountInfo();
            //        accountInfo.ShopId = 0;
            //        accountInfo.ShopName = "系统定时任务结算";
            //        accountInfo.AccountDate = DateTime.Now;
            //        accountInfo.StartDate = startDate;
            //        accountInfo.EndDate = endDate;
            //        accountInfo.Status = AccountInfo.AccountStatus.Accounted;
            //        accountInfo.ProductActualPaidAmount = pendingSetllementData.Sum(a => a.ProductsAmount);
            //        accountInfo.FreightAmount = pendingSetllementData.Sum(b => b.FreightAmount);
            //        accountInfo.CommissionAmount = pendingSetllementData.Sum(c => c.PlatCommission);
            //        accountInfo.RefundCommissionAmount = pendingSetllementData.Sum(d => d.PlatCommissionReturn);
            //        accountInfo.RefundAmount = pendingSetllementData.Sum(e => e.RefundAmount);
            //        accountInfo.AdvancePaymentAmount = 0;
            //        accountInfo.Brokerage = pendingSetllementData.Sum(f => f.DistributorCommission);
            //        accountInfo.ReturnBrokerage = pendingSetllementData.Sum(g => g.DistributorCommissionReturn);
            //        accountInfo.PeriodSettlement = pendingSetllementData.Sum(h => h.SettlementAmount);

            //        accountInfo.OpenCommission = pendingSetllementData.Sum(i => i.OpenCommission);
            //        accountInfo.JoinCommission = pendingSetllementData.Sum(j => j.JoinCommission);
            //        //结算主表汇总数据

            //        foreach (var item in pendingSetllementData)
            //        {
            //            var accountDetail = new AccountDetailInfo
            //            {
            //                //AccountId = result.Id,
            //                ShopId = item.ShopId,
            //                ShopName = item.ShopName,
            //                OrderType = Model.AccountDetailInfo.EnumOrderType.FinishedOrder,
            //                Date = DateTime.Now,
            //                OrderFinshDate = item.OrderFinshTime,
            //                OrderId = item.OrderId,
            //                ProductActualPaidAmount = item.ProductsAmount,
            //                FreightAmount = item.FreightAmount,
            //                CommissionAmount = item.PlatCommission,
            //                RefundCommisAmount = item.PlatCommissionReturn,
            //                OrderRefundsDates = "",
            //                RefundTotalAmount = item.RefundAmount,
            //                OrderAmount = item.OrderAmount,
            //                BrokerageAmount = item.DistributorCommission,
            //                ReturnBrokerageAmount = item.DistributorCommissionReturn,
            //                SettlementAmount = item.SettlementAmount,
            //                PaymentTypeName = item.PaymentTypeName,
            //                OpenCommission = item.OpenCommission,
            //                JoinCommission = item.JoinCommission
            //            };
            //            accountInfo.Himall_AccountDetails.Add(accountDetail);
            //            //var detail = entity.AccountDetailInfo.Add(accountDetail);

            //        }
            //        entity.AccountInfo.Add(accountInfo);

            //        entity.SaveChanges();
            //        Random r = new Random();

            //        var plat = entity.PlatAccountInfo.FirstOrDefault();//平台账户
            //        //写平台资金明细表
            //        var platAccountItem = new PlatAccountItemInfo();
            //        platAccountItem.AccoutID = plat.Id;
            //        platAccountItem.CreateTime = DateTime.Now;
            //        platAccountItem.AccountNo = string.Format("{0:yyyyMMddHHmmssfff}{1}", DateTime.Now, r.Next(1000, 9999));
            //        platAccountItem.Amount = accountInfo.CommissionAmount - accountInfo.RefundCommissionAmount;//平台佣金-平台佣金退还
            //        platAccountItem.Balance = plat.Balance + platAccountItem.Amount;//账户余额+平台佣金-平台佣金退还
            //        platAccountItem.TradeType = PlatAccountType.SettlementIncome;
            //        platAccountItem.IsIncome = true;
            //        platAccountItem.ReMark = DateTime.Now + "平台结算" + accountInfo.Id;
            //        platAccountItem.DetailId = accountInfo.Id.ToString();
            //        entity.PlatAccountItemInfo.Add(platAccountItem);

            //        if (plat != null)
            //        {
            //            //平台账户总金额(加这次平台的佣金)
            //            plat.Balance += platAccountItem.Amount;//平台佣金-平台佣金退还
            //            //平台待结算金额
            //            plat.PendingSettlement -= accountInfo.PeriodSettlement;//本次结算的总金额。//platAccountItem.Amount;//平台佣金-平台佣金退还
            //            //平台已结算金额
            //            plat.Settled += accountInfo.PeriodSettlement;//本次结算的总金额。//platAccountItem.Amount;//平台佣金-平台佣金退还
            //        }


            //        var shoppendingSetllement = pendingSetllementData.GroupBy(a => a.ShopId).ToList();


            //        foreach (var item in shoppendingSetllement)
            //        {

            //            //商户资金明细表
            //            var shopAccount = entity.ShopAccountInfo.Where(a => a.ShopId == item.Key).FirstOrDefault();

            //            var shopAccountItemInfo = new ShopAccountItemInfo();
            //            shopAccountItemInfo.AccoutID = shopAccount.Id;
            //            shopAccountItemInfo.AccountNo = string.Format("{0:yyyyMMddHHmmssfff}{1}", DateTime.Now, r.Next(1000, 9999));
            //            shopAccountItemInfo.ShopId = shopAccount.ShopId;
            //            shopAccountItemInfo.ShopName = shopAccount.ShopName;
            //            shopAccountItemInfo.CreateTime = DateTime.Now;
            //            shopAccountItemInfo.Amount = item.Sum(a => a.SettlementAmount);//结算金额
            //            shopAccountItemInfo.Balance = shopAccount.Balance + shopAccountItemInfo.Amount; ;//账户余额+结算金额
            //            shopAccountItemInfo.TradeType = ShopAccountType.SettlementIncome;
            //            shopAccountItemInfo.IsIncome = true;
            //            shopAccountItemInfo.ReMark = "店铺结算明细" + accountInfo.Id; ;
            //            shopAccountItemInfo.DetailId = accountInfo.Id.ToString();
            //            shopAccountItemInfo.SettlementCycle = settings.WeekSettlement;
            //            entity.ShopAccountItemInfo.Add(shopAccountItemInfo);

            //            if (shopAccount != null)
            //            {
            //                shopAccount.Balance += shopAccountItemInfo.Amount;//结算金额
            //                shopAccount.PendingSettlement -= shopAccountItemInfo.Amount;
            //                shopAccount.Settled += shopAccountItemInfo.Amount;//平台佣金-平台佣金退还
            //            }

            //        }
            //        entity.PendingSettlementOrdersInfo.RemoveRange(pendingSetllementData);//结算完了删除已结算数据 
            //        entity.SaveChanges();
            //        //transaction.Complete();

            //        account = entity.AccountInfo.Where(c => c.StartDate >= startDate).OrderByDescending(c => c.EndDate).FirstOrDefault();
            //        if (account != null)
            //        {
            //            checkDate = account.EndDate.Date;//上一次结束日期（2015-11-23 00:00:00），作为开始时间
            //        }
            //        else
            //        {
            //            checkDate = startDate.AddDays(settings.WeekSettlement);
            //        }
            //        startDate = checkDate.Date;
            //        endDate = startDate.AddDays(settings.WeekSettlement);
            //    }

            //}
            //catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            //{
            //    Log.Error("AccountJob : " + ex.Message);
            //}
            //catch (Exception ex)
            //{
            //    Log.Error("AccountJob : " + ex.Message);
            //}
            #endregion
        }
    

        private void CalculationMoney(DateTime startDate, DateTime endDate)
        {
            Entity.Entities entity = new Entities();
            //退货单的详细
            var hasRefundOrdersDetails = (
                from p in entity.OrderInfo
                join o in entity.OrderRefundInfo on p.Id equals o.OrderId
                join x in entity.OrderItemInfo on o.OrderId equals x.OrderId
                where p.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.Finish
                && o.ManagerConfirmDate >= startDate
                && o.ManagerConfirmDate < endDate
                && o.ManagerConfirmStatus == Himall.Model.OrderRefundInfo.OrderRefundConfirmStatus.Confirmed
                select new
                {
                    Order = p,
                    OrderRefund = o,
                    OrderItem = x
                }
                ).Distinct().ToList();
            //符合标准的所有的订单
            var ordersDetails = (
                from p in entity.OrderInfo
                join o in entity.OrderItemInfo on p.Id equals o.OrderId
                where p.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.Finish
                && p.FinishDate >= startDate
                && p.FinishDate < endDate
                select new
                {
                    Order = p,
                    OrderItem = o
                }
                ).ToList();

            //分组店铺统计
            List<long> shopIds = new List<long>();

            shopIds.AddRange(hasRefundOrdersDetails.Select(c => c.Order.ShopId));
            shopIds.AddRange(ordersDetails.Select(c => c.Order.ShopId));
            //shopIds.AddRange(finishedPurchaseAgreement.Select(c => c.ShopId));

            shopIds = shopIds.Distinct().ToList();

            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {

                    //插入Himall_Accounts 统计表
                    foreach (var shopId in shopIds)
                    {
                        var orders = ordersDetails.Where(c => c.Order.ShopId == shopId).Select(c => c.Order).Distinct().ToList();

                        decimal productActualPaidAmount = orders.Sum(c => c.ProductTotalAmount) - orders.Sum(c => c.DiscountAmount);
                        //退单的运费不要加
                        decimal freightAmount = orders.Sum(c => c.Freight);
                        //佣金
                        decimal commissionAmount = CalculationTotalCommission(ordersDetails.Where(c => c.Order.ShopId == shopId).Select(c => c.OrderItem).Distinct().ToList());


                        //分销佣金
                        decimal Brokerage = CalculationTotalBrokerage(ordersDetails.Where(c => c.Order.ShopId == shopId).Select(c => c.OrderItem).Distinct().ToList());

                        decimal ReturnBrokerage = CalculationTotalRefundBrokerage(hasRefundOrdersDetails.Where(c => c.OrderRefund.ShopId == shopId).Select(c => c.OrderRefund).Distinct().ToList());

                        decimal refundCommissionAmount = CalculationTotalRefundCommission(hasRefundOrdersDetails.Where(c => c.OrderRefund.ShopId == shopId).Select(c => c.OrderItem).Distinct().ToList());
                        decimal refundAmount = hasRefundOrdersDetails.Where(c => c.OrderRefund.ShopId == shopId).Select(c => c.OrderRefund).Distinct().Sum(c => c.Amount);
                        //decimal advancePaymentAmount = finishedPurchaseAgreement.Where(c => c.ShopId == shopId).Sum(c => c.AdvancePayment.Value);
                        //decimal accMetaAmount = acc.Where(e => e.ShopId == shopId).FirstOrDefault().Price;//服务费用
                        //本期应结=商品实付总额+运费—佣金—退款金额+退还佣金-服务费用-分销佣金+退还分销佣金
                        decimal periodSettlement = productActualPaidAmount + freightAmount - commissionAmount - refundAmount + refundCommissionAmount - Brokerage + ReturnBrokerage;
                        var accountInfo = new Model.AccountInfo();
                        accountInfo.ShopId = shopId;
                        accountInfo.ShopName = entity.ShopInfo.Where(c => c.Id == shopId).FirstOrDefault().ShopName;
                        accountInfo.AccountDate = DateTime.Now;
                        accountInfo.StartDate = startDate;
                        accountInfo.EndDate = endDate.AddSeconds(-1);
                        accountInfo.Status = Model.AccountInfo.AccountStatus.UnAccount;
                        accountInfo.ProductActualPaidAmount = productActualPaidAmount;
                        accountInfo.FreightAmount = freightAmount;
                        accountInfo.CommissionAmount = commissionAmount;
                        accountInfo.RefundCommissionAmount = refundCommissionAmount;
                        accountInfo.RefundAmount = refundAmount;
                        //accountInfo.AdvancePaymentAmount = advancePaymentAmount;
                        accountInfo.PeriodSettlement = periodSettlement;
                        accountInfo.Brokerage = Brokerage;
                        accountInfo.ReturnBrokerage = ReturnBrokerage;
                        accountInfo.Remark = string.Empty;
                        entity.AccountInfo.Add(accountInfo);

                        //插入Himall_AccountDetails 订单的结算详情表

                        //含有退款的订单
                        foreach (var order in hasRefundOrdersDetails.Where(c => c.Order.ShopId == shopId).Select(c => c.Order).Distinct().ToList())
                        {
                            var accountDetail = new Model.AccountDetailInfo();
                            accountDetail.Himall_Accounts = accountInfo;
                            accountDetail.ShopId = order.ShopId;
                            accountDetail.Date = order.FinishDate.Value;
                            accountDetail.OrderType = Model.AccountDetailInfo.EnumOrderType.ReturnOrder;
                            accountDetail.OrderId = order.Id;
                            accountDetail.ProductActualPaidAmount = order.ProductTotalAmount - order.DiscountAmount;
                            accountDetail.FreightAmount = order.Freight;
                            accountDetail.CommissionAmount = CalculationTotalCommission(hasRefundOrdersDetails.Where(c => c.OrderRefund.OrderId == order.Id).Select(c => c.OrderItem).Distinct().ToList());
                            accountDetail.RefundCommisAmount = CalculationTotalRefundCommission(hasRefundOrdersDetails.Where(c => c.OrderRefund.OrderId == order.Id).Select(c => c.OrderItem).Distinct().ToList());

                            //YZY分销退款佣金
                            accountDetail.BrokerageAmount = CalculationTotalBrokerage(hasRefundOrdersDetails.Where(c => c.OrderRefund.OrderId == order.Id).Select(c => c.OrderItem).Distinct().ToList());
                            accountDetail.ReturnBrokerageAmount = CalculationTotalRefundBrokerage(hasRefundOrdersDetails.Where(c => c.OrderRefund.OrderId == order.Id).Select(c => c.OrderRefund).Distinct().ToList());


                            accountDetail.RefundTotalAmount = hasRefundOrdersDetails.Where(c => c.OrderRefund.OrderId == order.Id).Select(c => c.OrderRefund).Distinct().Sum(c => c.Amount);
                            accountDetail.OrderDate = order.OrderDate;
                            accountDetail.OrderRefundsDates = string.Join(";", hasRefundOrdersDetails.Where(c => c.OrderRefund.OrderId == order.Id).Select(c => c.OrderRefund.ManagerConfirmDate).Distinct());
                            entity.AccountDetailInfo.Add(accountDetail);
                        }

                        foreach (var order in orders)
                        {
                            var accountDetail = new Model.AccountDetailInfo();
                            accountDetail.Himall_Accounts = accountInfo;
                            accountDetail.ShopId = order.ShopId;
                            accountDetail.Date = order.FinishDate.Value;
                            accountDetail.OrderType = Model.AccountDetailInfo.EnumOrderType.FinishedOrder;
                            accountDetail.OrderId = order.Id;
                            accountDetail.ProductActualPaidAmount = order.ProductTotalAmount - order.DiscountAmount;
                            accountDetail.FreightAmount = order.Freight;
                            accountDetail.CommissionAmount = CalculationTotalCommission(ordersDetails.Where(c => c.Order.Id == order.Id).Select(c => c.OrderItem).Distinct().ToList());
                            accountDetail.RefundCommisAmount = decimal.Zero;
                            accountDetail.RefundTotalAmount = decimal.Zero;
                            accountDetail.BrokerageAmount = decimal.Zero;
                            accountDetail.ReturnBrokerageAmount = decimal.Zero;
                            accountDetail.OrderDate = order.OrderDate;

                            accountDetail.OrderRefundsDates = string.Empty;
                            entity.AccountDetailInfo.Add(accountDetail);
                        }

                    }

                    int rows = entity.SaveChanges();
                    transaction.Complete();

                }
                catch (Exception ee)
                {
                    Log.Error("CalculationMoney ：startDate=" + startDate.ToString() + " endDate=" + endDate.ToString() + "/r/n" + ee.Message);
                }
            }
            #region 营销费用*************************
            try
            {
                /*
                var acc = (from a in entity.AccountInfo
                           join m in
                               (from AM in entity.ActiveMarketServiceInfo
                                join M in entity.MarketSettingInfo on AM.TypeId equals M.TypeId
                                where AM.MarketServiceRecordInfo.Max(m => m.EndTime) >= endDate
                                select new
                                {
                                    ShopId = AM.ShopId,
                                    Price = M.Price,
                                    TypeId = M.TypeId
                                }) on a.ShopId equals m.ShopId
                           where (a.StartDate.Year <= startDate.Year && a.StartDate.Month <= startDate.Month) && a.EndDate >= endDate
                           select new
                           {
                               ShopId = m.ShopId,
                               Price = m.Price,
                               TypeId = m.TypeId,
                               Id = a.Id
                           }).ToList();*/

                //未结算的营销费用（当前结算开始日期之前最近的记录）
                var market = (from b in entity.ActiveMarketServiceInfo
                              join c in entity.MarketServiceRecordInfo on b.Id equals c.MarketServiceId
                              join d in shopIds on b.ShopId equals d
                              join aa in entity.MarketSettingInfo on b.TypeId equals aa.TypeId
                              where c.SettlementFlag == 0
                              select new
                              {
                                  ShopId = b.ShopId,
                                  TypeId = b.TypeId,
                                  Price = (c.EndTime.Year * 12 + c.EndTime.Month - (c.StartTime.Year * 12 + c.StartTime.Month)) * aa.Price,//购买的月数
                                  MSRecordId = c.Id,
                                  StartTime = c.StartTime,
                                  EndTime = c.EndTime
                              }).ToList();

                var account = entity.AccountInfo.Where(e => e.StartDate == startDate).ToList();
                var marketShop = market.Select(e => e.ShopId).Distinct();
                foreach (var item in account)
                {
                    var accInfo = market.Where(e => e.ShopId == item.ShopId);
                    foreach (var serviceType in accInfo)
                    {
                        item.PeriodSettlement = item.PeriodSettlement - serviceType.Price;//减掉营销服务费用
                        item.AdvancePaymentAmount = item.AdvancePaymentAmount + serviceType.Price;//存放营销服务费用
                        entity.AccountMetaInfo.Add(new Model.AccountMetaInfo
                        {
                            AccountId = item.Id,
                            MetaKey = serviceType.TypeId.ToDescription(),
                            MetaValue = serviceType.Price.ToString("f2"),
                            ServiceStartTime = serviceType.StartTime,
                            ServiceEndTime = serviceType.EndTime
                        });
                    }
                }
                var MSRecordInfos = (from a in entity.MarketServiceRecordInfo
                                     join b in entity.ActiveMarketServiceInfo on a.MarketServiceId equals b.Id
                                     join c in marketShop on b.ShopId equals c
                                     where a.SettlementFlag == 0
                                     select new
                                     {
                                         Shopid = b.ShopId,
                                         TypeId = b.TypeId,
                                         Msrecordid = a.Id
                                     }).ToList();
                foreach (var record in MSRecordInfos)
                {
                    var recordModel = entity.MarketServiceRecordInfo.FirstOrDefault(e => e.Id == record.Msrecordid);
                    recordModel.SettlementFlag = 1;
                }
                entity.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error("CalculationMoney 服务费：startDate=" + startDate.ToString() + " endDate=" + endDate.ToString() + "/r/n" + ex.Message);
            }
            #endregion 营销费用
        }
        private int GetMonths(DateTime dt1, DateTime dt2)
        {
            return dt2.Year * 12 + dt2.Month - (dt1.Year * 12 + dt1.Month);
        }
        private static DateTime GetDate(Entity.Entities entity, DateTime checkDate)
        {

            var firstFinishedOrder = entity.PendingSettlementOrdersInfo.OrderBy(c => c.OrderFinshTime).FirstOrDefault();
            if (firstFinishedOrder != null)
            {
                checkDate = firstFinishedOrder.OrderFinshTime;
            }
            else
            {
                checkDate = DateTime.MinValue;
            }
            return checkDate;
        }
        private static DateTime GetCheckDate(Entity.Entities entity, DateTime checkDate)
        {

            var firstFinishedOrder = entity.OrderInfo.Where(c => c.OrderStatus == OrderInfo.OrderOperateStatus.Finish).OrderBy(c => c.FinishDate).FirstOrDefault();
            //  return order;

            //  var firstFinishedOrder = new OrderService().GetFirstFinishedOrderForSettlement();
            //var firstFinishedAgreement = entity.PurchaseAgreementInfo.Where(c => c.AgreementStatus == Himall.Model.PurchaseAgreementInfo.AgreementState.HasComplete).OrderBy(c => c.FinishDate).FirstOrDefault();

            if (firstFinishedOrder != null)
            {
                checkDate = firstFinishedOrder.FinishDate.Value;
            }
            else if (firstFinishedOrder != null)
            {
                checkDate = firstFinishedOrder.FinishDate.Value;
            }
            //else if (firstFinishedAgreement != null)
            //{
            //    checkDate = firstFinishedAgreement.FinishDate.Value;
            //}
            else
            {
                checkDate = DateTime.MinValue;
            }
            return checkDate;
        }
    }
}
