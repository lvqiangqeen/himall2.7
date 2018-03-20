using Himall.CommonModel;

using Himall.Entity;
using Himall.Model;
using Himall.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinServiceBase;

namespace WinSettlementService
{
    public class Settlement:ISyncData
    {
        public void SyncData()
        {
            AutoSettlement();
        }
        public void AutoSettlement()
        {
            //using (TransactionScope transaction = new TransactionScope())
            //{
            try
            {
                Himall.Entity.Entities entity = new Entities();
                var checkDate = DateTime.MinValue;
                Log.Debug("AccountJob : start");
               // var settings = new SiteSettingService().GetSiteSettings();   
                var settings = new SiteSettingService().GetSiteSettingsByObjectCache();
                if (settings.WeekSettlement < 1)
                {
                    Log.Error("结算周期设置不正确！ ");
                    return;
                }
                var account = entity.AccountInfo.OrderByDescending(a => a.Id).FirstOrDefault();
                if (account == null)
                {
                    //第一笔结算数据
                    checkDate = GetDate(entity, checkDate).Date;
                }
                else
                {
                    checkDate = account.EndDate.Date;//上一次结束日期（2015-11-23 00:00:00），作为开始时间
                }
                if (checkDate.Equals(DateTime.MinValue))
                    return;

                DateTime startDate = checkDate.Date;
                DateTime endDate = startDate.AddDays(settings.WeekSettlement);
                Log.Debug("AccountJob:endDate" + endDate + "DateTime:" + DateTime.Now.Date + "result:" + (endDate < DateTime.Now.Date));
                while (endDate < DateTime.Now)
                {
                    //结算日期内的待结算订单 不计算开始时间，防止漏单
                    var pendingSetllementData = entity.PendingSettlementOrdersInfo.Where(c => c.OrderFinshTime < endDate).OrderByDescending(c => c.OrderFinshTime).ToList();
                    Log.Debug("Count:" + pendingSetllementData.Count());
                    var accountInfo = new AccountInfo();
                    accountInfo.ShopId = 0;
                    accountInfo.ShopName = "系统定时任务结算";
                    accountInfo.AccountDate = DateTime.Now;
                    accountInfo.StartDate = startDate;
                    accountInfo.EndDate = endDate;
                    accountInfo.Status = AccountInfo.AccountStatus.Accounted;
                    accountInfo.ProductActualPaidAmount = pendingSetllementData.Sum(a => a.ProductsAmount);
                    accountInfo.FreightAmount = pendingSetllementData.Sum(b => b.FreightAmount);
                    accountInfo.CommissionAmount = pendingSetllementData.Sum(c => c.PlatCommission);
                    accountInfo.RefundCommissionAmount = pendingSetllementData.Sum(d => d.PlatCommissionReturn);
                    accountInfo.RefundAmount = pendingSetllementData.Sum(e => e.RefundAmount);
                    accountInfo.AdvancePaymentAmount = 0;
                    accountInfo.Brokerage = pendingSetllementData.Sum(f => f.DistributorCommission);
                    accountInfo.ReturnBrokerage = pendingSetllementData.Sum(g => g.DistributorCommissionReturn);
                    accountInfo.PeriodSettlement = pendingSetllementData.Sum(h => h.SettlementAmount);
                    //结算主表汇总数据

                    foreach (var item in pendingSetllementData)
                    {
                        var accountDetail = new AccountDetailInfo
                        {
                            //AccountId = result.Id,
                            ShopId = item.ShopId,
                            ShopName = item.ShopName,
                            OrderType = Himall.Model.AccountDetailInfo.EnumOrderType.FinishedOrder,
                            Date = DateTime.Now,
                            OrderFinshDate = item.OrderFinshTime,
                            OrderId = item.OrderId,
                            ProductActualPaidAmount = item.ProductsAmount,
                            FreightAmount = item.FreightAmount,
                            CommissionAmount = item.PlatCommission,
                            RefundCommisAmount = item.PlatCommissionReturn,
                            OrderRefundsDates = "",
                            RefundTotalAmount = item.RefundAmount,
                            OrderAmount = item.OrderAmount,
                            BrokerageAmount = item.DistributorCommission,
                            ReturnBrokerageAmount = item.DistributorCommissionReturn,
                            SettlementAmount = item.SettlementAmount,
                            PaymentTypeName = item.PaymentTypeName
                        };
                        accountInfo.Himall_AccountDetails.Add(accountDetail);
                        //var detail = entity.AccountDetailInfo.Add(accountDetail);

                    }
                    entity.AccountInfo.Add(accountInfo);

                    entity.SaveChanges();
                    Random r = new Random();

                    var plat = entity.PlatAccountInfo.FirstOrDefault();//平台账户
                    //写平台资金明细表
                    var platAccountItem = new PlatAccountItemInfo();
                    platAccountItem.AccoutID = plat.Id;
                    platAccountItem.CreateTime = DateTime.Now;
                    platAccountItem.AccountNo = string.Format("{0:yyyyMMddHHmmssfff}{1}", DateTime.Now, r.Next(1000, 9999));
                    platAccountItem.Amount = accountInfo.CommissionAmount - accountInfo.RefundCommissionAmount;//平台佣金-平台佣金退还
                    platAccountItem.Balance = plat.Balance + platAccountItem.Amount;//账户余额+平台佣金-平台佣金退还
                    platAccountItem.TradeType = PlatAccountType.SettlementIncome;
                    platAccountItem.IsIncome = true;
                    platAccountItem.ReMark = DateTime.Now + "平台结算" + accountInfo.Id;
                    platAccountItem.DetailId = accountInfo.Id.ToString();
                    entity.PlatAccountItemInfo.Add(platAccountItem);

                    if (plat != null)
                    {
                        //平台账户总金额(加这次平台的佣金)
                        plat.Balance += platAccountItem.Amount;//平台佣金-平台佣金退还
                        //平台待结算金额
                        plat.PendingSettlement -= accountInfo.PeriodSettlement;//本次结算的总金额。//platAccountItem.Amount;//平台佣金-平台佣金退还
                        //平台已结算金额
                        plat.Settled += accountInfo.PeriodSettlement;//本次结算的总金额。//platAccountItem.Amount;//平台佣金-平台佣金退还
                    }


                    var shoppendingSetllement = pendingSetllementData.GroupBy(a => a.ShopId).ToList();


                    foreach (var item in shoppendingSetllement)
                    {

                        //商户资金明细表
                        var shopAccount = entity.ShopAccountInfo.Where(a => a.ShopId == item.Key).FirstOrDefault();

                        var shopAccountItemInfo = new ShopAccountItemInfo();
                        shopAccountItemInfo.AccoutID = shopAccount.Id;
                        shopAccountItemInfo.AccountNo = string.Format("{0:yyyyMMddHHmmssfff}{1}", DateTime.Now, r.Next(1000, 9999));
                        shopAccountItemInfo.ShopId = shopAccount.ShopId;
                        shopAccountItemInfo.ShopName = shopAccount.ShopName;
                        shopAccountItemInfo.CreateTime = DateTime.Now;
                        shopAccountItemInfo.Amount = item.Sum(a => a.SettlementAmount);//结算金额
                        shopAccountItemInfo.Balance = shopAccount.Balance + shopAccountItemInfo.Amount; ;//账户余额+结算金额
                        shopAccountItemInfo.TradeType = ShopAccountType.SettlementIncome;
                        shopAccountItemInfo.IsIncome = true;
                        shopAccountItemInfo.ReMark = "店铺结算明细" + accountInfo.Id; ;
                        shopAccountItemInfo.DetailId = accountInfo.Id.ToString();
                        shopAccountItemInfo.SettlementCycle = settings.WeekSettlement;
                        entity.ShopAccountItemInfo.Add(shopAccountItemInfo);

                        if (shopAccount != null)
                        {
                            shopAccount.Balance += shopAccountItemInfo.Amount;//结算金额
                            shopAccount.PendingSettlement -= shopAccountItemInfo.Amount;
                            shopAccount.Settled += shopAccountItemInfo.Amount;//平台佣金-平台佣金退还
                        }

                    }
                    entity.PendingSettlementOrdersInfo.RemoveRange(pendingSetllementData);//结算完了删除已结算数据 
                    entity.SaveChanges();
                    //transaction.Complete();

                    account = entity.AccountInfo.Where(c => c.StartDate >= startDate).OrderByDescending(c => c.EndDate).FirstOrDefault();
                    if (account != null)
                    {
                        checkDate = account.EndDate.Date;//上一次结束日期（2015-11-23 00:00:00），作为开始时间
                    }
                    else
                    {
                        checkDate = startDate.AddDays(settings.WeekSettlement);
                    }
                    startDate = checkDate.Date;
                    endDate = startDate.AddDays(settings.WeekSettlement);
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                Log.Error("AccountJob : " + ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("AccountJob : " + ex.Message);
            }
        }



        private  DateTime GetDate(Himall.Entity.Entities entity, DateTime checkDate)
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

      
    }
}
