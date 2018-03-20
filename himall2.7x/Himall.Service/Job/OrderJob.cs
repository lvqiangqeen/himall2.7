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
    public class OrderJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            #region 放弃
            //
            //try
            //{
            //    //using (StreamWriter sw = File.CreateText(Core.Helper.IOHelper.GetMapPath("/orderlog1.txt")))
            //    //{
            //    //    sw.WriteLine(DateTime.Now.ToLongTimeString());
            //    //}

            //    using (Entity.Entities entity = new Entity.Entities())
            //    {
            //        // 关闭过期未付款的订单
            //        SqlParameter[] para = new SqlParameter[] { new SqlParameter("@OrderDate", DateTime.Now.AddDays(0 - 7)) };
            //        entity.Database.ExecuteSqlCommand("UPDATE Himall_Orders SET OrderStatus = 4, CloseReason='过期没付款，自动关闭' WHERE OrderStatus = 1 AND OrderDate <= @OrderDate", para);

            //        // 完成过期未确认收货的订单
            //        para = new SqlParameter[] { new SqlParameter("@ShippingDate", DateTime.Now.AddDays(0 - 7)) };
            //        entity.Database.ExecuteSqlCommand("UPDATE Hishop_Orders SET FinishDate = getdate(), OrderStatus = 5 WHERE OrderStatus=3 AND ShippingDate <= @ShippingDate", para);
            //    }
            //}catch{}
            /*
             Entity.Entities entity = new Entity.Entities();
             using (TransactionScope transaction = new TransactionScope())
             {
                 try
                 {
                     var siteSetting = new SiteSettingService().GetSiteSettings();
                     //退换货间隔天数
                     int intIntervalDay = siteSetting == null ? 7 : (siteSetting.NoReceivingTimeout == 0 ? 7 : siteSetting.NoReceivingTimeout);
                     DateTime waitPayDate = DateTime.Now;
                     DateTime waitReceivingDate = DateTime.Now.AddDays(-intIntervalDay);
                     // 关闭过期未付款的订单
                     //Entity.Entities entity = new Entity.Entities();
                     var productService = ServiceProvider.Instance<Himall.IServices.IProductService>.Create;
                     var orderService = ServiceProvider.Instance<Himall.IServices.IOrderService>.Create;
                     //查找待付款的订单
                     var ordersWaitPay = (
                         from p in entity.OrderInfo
                         where p.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitPay &&
                         p.OrderDate <= waitPayDate
                         select p
                         ).ToList();
                     foreach (var order in ordersWaitPay)
                     {
                         int hours = siteSetting == null ? 2 : (siteSetting.UnpaidTimeout == 0 ? 2 : siteSetting.UnpaidTimeout);
                         if (DateTime.Now.Subtract(order.OrderDate).Hours >= hours)
                         {
                             order.OrderStatus = Model.OrderInfo.OrderOperateStatus.Close;
                             order.CloseReason = "过期没付款，自动关闭";
                             var orderItems = entity.OrderItemInfo.Where(c => c.OrderId == order.Id).ToList();
                             foreach (var orderItem in orderItems)
                             {
                                 //加回库存
                                 productService.UpdateStock(orderItem.SkuId, orderItem.Quantity);
                             }
                         }


                     }
                     var ordersFinish = (
                         from p in entity.OrderInfo
                         where p.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitReceiving &&
                         p.ShippingDate <= waitReceivingDate
                         select p
                         ).ToList();
                     foreach (var order in ordersFinish)
                     {
                         order.FinishDate = DateTime.Now;
                         order.OrderStatus = Model.OrderInfo.OrderOperateStatus.Finish;
                         order.CloseReason = "完成过期未确认收货的订单";
                     }
                     entity.SaveChanges();
                     transaction.Complete();
                 }
                 catch
                 {

                 }

             }*/

            #endregion
           // Himall.Core.Log.Debug("OrderJob : checkDate=" +DateTime.Now);
           // var service = ServiceProvider.Instance<IOrderService>.Create;
           // service.AutoCloseOrder();
           // service.AutoConfirmOrder();
           // var service2 = ServiceProvider.Instance<ICommentService>.Create;
           // service2.AutoComment();
           //// TODO LRL  08/27 添加礼品兑换订单过期
           // var giftorderser = ServiceProvider.Instance<IGiftsOrderService>.Create;
           // giftorderser.AutoConfirmOrder();
        }
    }
}
