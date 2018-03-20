using Himall.IServices;
using Himall.ServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinOrderService
{
    public class OrderRefund : ISyncData
    {
        public void SyncData()
        {
            AutoRefund();
        }

        private void AutoRefund()
        {
            var service = Instance<IRefundService>.Create;
            try
            {
                service.AutoAuditRefund();
              //  Himall.Core.Log.Error("AutoRefund运行成功");
            }
            catch (Exception ex)
            {
                Himall.Core.Log.Error("RefundJob : AutoAuditRefund 有异常", ex);
            }
            try
            {
                service.AutoCloseByDeliveryExpired();
            }
            catch (Exception ex)
            {
                Himall.Core.Log.Error("RefundJob : AutoCloseByDeliveryExpired 有异常", ex);
            }
            try
            {
                service.AutoShopConfirmArrival();
            }
            catch (Exception ex)
            {
                Himall.Core.Log.Error("RefundJob : AutoShopConfirmArrival 有异常", ex);
            }
        }
    }
}
