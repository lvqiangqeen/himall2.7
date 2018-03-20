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
    public class ShopJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Himall.Core.Log.Debug("ShopJob : checkDate=" + DateTime.Now);
            var service = ServiceProvider.Instance<IShopService>.Create;

            try
            {
                service.AutoSaleOffProductByShopExpiredOrFreeze();
            }
            catch (Exception ex)
            {
                Himall.Core.Log.Debug("ShopJob : AutoSaleOffProductByShopExpiredOrFreeze 有异常", ex);
            }

            try
            {
                service.AutoCloseMarketingActionByShopExpiredOrFreeze();
            }
            catch (Exception ex)
            {
                Himall.Core.Log.Debug("ShopJob : AutoCloseMarketingActionByShopExpiredOrFreeze 有异常", ex);
            }
        }
    }
}
