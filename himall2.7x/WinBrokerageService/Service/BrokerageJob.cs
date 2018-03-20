using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Configuration;
using Himall.Model;
using WinServiceBase;
using Himall.Entity;
using Himall.Service;
using Himall.IServices;
using System.Transactions;
using System.Data.Entity;

namespace WinBrokerageService
{
    public class BrokerageJob : ISyncData
    {
        /// <summary>
        /// 功能执行入口
        /// </summary>
        public void SyncData()
        {
            try
            {
                Entities entity = new Entities();
                //    var settings = new SiteSettingService().GetSiteSettings();
                var service = Himall.ServiceProvider.Instance<ISiteSettingService>.Create;
                var settings = service.GetSiteSettingsByObjectCache();
                var expried = settings.SalesReturnTimeout;
                var now = DateTime.Now.Date;
                var exday = now.AddDays(-expried);
                var t = entity.BrokerageIncomeInfo.Where(a => a.Status == BrokerageIncomeInfo.BrokerageStatus.NotSettled);
                var order = entity.OrderInfo.Where(b => b.OrderStatus == OrderInfo.OrderOperateStatus.Finish);
                var models = t.Join(order, a => a.OrderId, b => b.Id, (a, b) => new
                {
                    a.Id,
                    a.OrderId,
                    a.OrderTime,
                    a.OrderItemId,
                    a.Brokerage,
                    a.Himall_BrokerageRefund,
                    a.ProductID,
                    a.UserId,
                    b.FinishDate,
                    b.OrderStatus,
                }).ToList().Where(b => b.FinishDate.Value.Date <= exday);

                foreach (var model in models)
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        IMemberCapitalService capitalServicer = Himall.ServiceProvider.Instance<IMemberCapitalService>.Create;
                        CapitalDetailModel capita = new CapitalDetailModel
                        {
                            UserId = model.UserId,
                            SourceType = CapitalDetailInfo.CapitalDetailType.Brokerage,
                            CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        capita.Amount = model.Brokerage;
                        var refund = model.Himall_BrokerageRefund.FirstOrDefault();
                        if (refund != null)
                        {

                            capita.Amount = model.Brokerage - refund.Brokerage;
                        }
                        var paid = entity.ProductBrokerageInfo.Where(a => a.ProductId == model.ProductID).FirstOrDefault();
                        var b = entity.BrokerageIncomeInfo.Where(a => a.OrderItemId == model.OrderItemId && a.Status == BrokerageIncomeInfo.BrokerageStatus.NotSettled).FirstOrDefault();
                        if (b != null)
                        {
                            b.Status = BrokerageIncomeInfo.BrokerageStatus.Settled;
                            b.SettlementTime = DateTime.Now;
                            paid.BrokerageAmount += capita.Amount;
                            capita.Remark = "分销佣金,订单Id:" + model.OrderId + ",商品ID：" + model.ProductID;
                            capita.SourceData = model.OrderId.HasValue ? model.OrderId.Value.ToString() : "";
                            capitalServicer.AddCapital(capita);
                            //已结算累加
                            entity.SaveChanges();
                            transaction.Complete();
                        }
                    }
                }
                Log.Debug("运行正常");
            }
            catch (Exception ex)
            {
                Log.Debug("有问题啦:" + ex.Message + ":" + ex.StackTrace);
            }
        }
    }
}
