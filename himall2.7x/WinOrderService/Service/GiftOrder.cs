using Himall.IServices;
using Himall.ServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinOrderService
{
    public class GiftOrder : ISyncData
    {
        public void SyncData()
        {
            AutoGiftOrder();
        }

        public void AutoGiftOrder()
        {
            try
            {

                //  LRL   添加礼品兑换订单过期
                var giftorderser = Instance<IGiftsOrderService>.Create;
                giftorderser.AutoConfirmOrder();
               // Himall.Core.Log.Error("AutoGiftOrder运行成功");
            }
            catch (Exception ex)
            {
                Himall.Core.Log.Error("关闭过期礼品错误:", ex);
            }
        }

    }
}
