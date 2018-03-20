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

namespace Himall.Service.Job
{
    public class BrokerageJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Entity.Entities entity = new Entities();
            var settings = new SiteSettingService().GetSiteSettings();
            var expried = settings.SalesReturnTimeout;
            var now = DateTime.Now;
            var exday = now.AddDays(-expried);
            var t = entity.BrokerageIncomeInfo.Where(a => a.Status == Model.BrokerageIncomeInfo.BrokerageStatus.NotSettled);
            var order = entity.OrderInfo.Where(b => b.OrderStatus == Model.OrderInfo.OrderOperateStatus.Finish && b.FinishDate.Value <= exday);
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
            }).ToList();

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
        }
    }
}
