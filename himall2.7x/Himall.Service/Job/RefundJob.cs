using Himall.IServices;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Himall.Service.Job
{
    public class RefundJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            //Himall.Core.Log.Debug("RefundJob : checkDate=" + DateTime.Now);
            //var service = ServiceProvider.Instance<IRefundService>.Create;

            //try
            //{
            //    service.AutoAuditRefund();
            //}
            //catch (Exception ex)
            //{
            //    Himall.Core.Log.Debug("RefundJob : AutoAuditRefund 有异常", ex);
            //}

            //try
            //{
            //    service.AutoCloseByDeliveryExpired();
            //}
            //catch (Exception ex)
            //{
            //    Himall.Core.Log.Debug("RefundJob : AutoCloseByDeliveryExpired 有异常", ex);
            //}

            //try
            //{
            //    service.AutoShopConfirmArrival();
            //}
            //catch (Exception ex)
            //{
            //    Himall.Core.Log.Debug("RefundJob : AutoShopConfirmArrival 有异常", ex);
            //}
        }
    }
}
