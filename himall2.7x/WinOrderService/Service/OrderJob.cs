using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.ServiceProvider;
using WinServiceBase;


namespace WinOrderService
{
    public class OrderJob : ISyncData
    {
        public void SyncData()
        {
            AutoOrder();
        }


        private void AutoOrder()
        {
            //try
            //{
            //    var newservice = Instance<IOrderService>.Create;
            //}
            //catch(Exception  ex)
            //{
            //   Log.Error("AutoOrder运行失败::"+ex.Message+"::::"+ex.StackTrace);
            //}
            var service = Himall.ServiceProvider.Instance<IOrderService>.Create;
            try
            {
                service.AutoCloseOrder();//自动关闭超时订单
               // Himall.Core.Log.Error("AutoOrder运行成功");
            }
            catch (Exception ex)
            {
                Himall.Core.Log.Error("自动关闭超时订单出错:", ex);
            }
            try
            {
                service.AutoConfirmOrder();//自动确认收货
            //    Himall.Core.Log.Error("AutoOrder.AutoConfirmOrder运行成功");
            }
            catch (Exception ex)
            {
                Himall.Core.Log.Error("自动确认完成订单出错:", ex);
            }
        }


    }
}
