using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinShopServices
{
    public class Shop : ISyncData
    {
        public void SyncData()
        {
            AutoShop();
        }

        private void AutoShop()
        {
            var service = Himall.ServiceProvider.Instance<IShopService>.Create;

            try
            {
                service.AutoSaleOffProductByShopExpiredOrFreeze();
            }
            catch (Exception ex)
            {
                WinServiceBase.Log.Error("ShopJob : AutoSaleOffProductByShopExpiredOrFreeze 有异常"+ex.Message+"::"+ex.StackTrace);
            }

            try
            {
                service.AutoCloseMarketingActionByShopExpiredOrFreeze();
            }
            catch (Exception ex)
            {
                WinServiceBase.Log.Error("ShopJob : AutoCloseMarketingActionByShopExpiredOrFreeze 有异常" + ex.Message + "::" + ex.StackTrace);
            }
        }
    }
}
