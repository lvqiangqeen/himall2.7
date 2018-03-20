using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices.QueryModel;

namespace Himall.IServices
{
    public interface IGiftsOrderService : IService
    {
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        GiftOrderInfo CreateOrder(GiftOrderModel model);
        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        GiftOrderInfo GetOrder(long orderId);
        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        GiftOrderInfo GetOrder(long orderId, long userId);
        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<GiftOrderInfo> GetOrders(GiftsOrderQuery query);
        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        IEnumerable<GiftOrderInfo> GetOrders(IEnumerable<long> ids);
        /// <summary>
        /// 获取订单计数
        /// </summary>
        /// <param name="status">为空表示所有订单</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        int GetOrderCount(GiftOrderInfo.GiftOrderStatus? status, long userId = 0);
        /// <summary>
        /// 获取订单项
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        GiftOrderItemInfo GetOrderItemById(long id);
        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="closeReason"></param>
        void CloseOrder(long id, string closeReason);
        /// <summary>
        /// 发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shipCompanyName"></param>
        /// <param name="shipOrderNumber"></param>
        void SendGood(long id, string shipCompanyName, string shipOrderNumber);
        /// <summary>
        /// 确认订单到货
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        void ConfirmOrder(long id, long userId);
        /// <summary>
        /// 过期自动确认订单到货
        /// </summary>
        void AutoConfirmOrder();
        /// <summary>
        /// 已购买数量
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="giftid"></param>
        /// <returns></returns>
        int GetOwnBuyQuantity(long userid, long giftid);
        /// <summary>
        /// 补充用户数据
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        IEnumerable<GiftOrderInfo> OrderAddUserInfo(IEnumerable<GiftOrderInfo> orders);
    }
}
