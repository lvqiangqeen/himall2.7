using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.IServices;
using Himall.Model;

namespace Himall.Application
{
    public class MessageApplication
    {
        private static IMessageService _iMessageService = ObjectContainer.Current.Resolve<IMessageService>();
        //更新信息=用户表
        public static  void UpdateMemberContacts(MemberContactsInfo info)
        {
            _iMessageService.UpdateMemberContacts(info);
        }
        /// <summary>
        /// 获取发送目标
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="pluginId">插件ID</param>
        /// <param name="type">用户类型</param>
        /// <returns></returns>
        public static string GetDestination(long userId, string pluginId, MemberContactsInfo.UserTypes type)
        {
          return   _iMessageService.GetDestination(userId, pluginId, type);
        }

        /// <summary>
        /// 根据插件类型和ID和目标获取信息
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="contact"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MemberContactsInfo GetMemberContactsInfo(string pluginId, string contact, MemberContactsInfo.UserTypes type)
        {
          return  _iMessageService.GetMemberContactsInfo(pluginId, contact, type);
        }

        /// <summary>
        /// 根据用户ID获取目标信息
        /// </summary>
        /// <param name="UserId">用户ID</param>
        /// <returns></returns>
        public static List<MemberContactsInfo> GetMemberContactsInfo(long UserId)
        {
          return   _iMessageService.GetMemberContactsInfo(UserId);
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageCode(string destination, string pluginId, MessageUserInfo info)
        {
             _iMessageService.SendMessageCode(destination, pluginId, info);
        }

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnFindPassWord(long userId, MessageUserInfo info)
        {
            _iMessageService.SendMessageOnFindPassWord(userId, info);
        }
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnOrderCreate(long userId, MessageOrderInfo info)
        {
            _iMessageService.SendMessageOnOrderCreate(userId, info);
        }

        /// <summary>
        /// 订单支付
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnOrderPay(long userId, MessageOrderInfo info)
        {
            _iMessageService.SendMessageOnOrderPay(userId, info);
        }
        /// <summary>
        /// 店铺有新订单
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnShopHasNewOrder(long shopId, MessageOrderInfo info)
        {
            _iMessageService.SendMessageOnShopHasNewOrder(shopId, info);
        }
        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnOrderRefund(long userId, MessageOrderInfo info, long refundid = 0)
        {
            _iMessageService.SendMessageOnOrderRefund(userId, info, refundid);
        }
        /// <summary>
        /// 售后发货信息提醒
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnRefundDeliver(long userId, MessageOrderInfo info, long refundid = 0)
        {
            _iMessageService.SendMessageOnRefundDeliver(userId, info, refundid);
        }

        /// <summary>
        /// 订单发货
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnOrderShipping(long userId, MessageOrderInfo info)
        {
            _iMessageService.SendMessageOnOrderShipping(userId, info);
        }
        /// <summary>
        /// 店铺审核
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnShopAudited(long userId, MessageShopInfo info)
        {
            _iMessageService.SendMessageOnShopAudited(userId, info);
        }

        /// <summary>
        /// 发送优惠券成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public static void SendMessageOnCouponSuccess(long userId, MessageCouponInfo info)
        {
            _iMessageService.SendMessageOnCouponSuccess(userId, info);
        }

        ///// <summary>
        ///// 店铺成功2.4去除
        ///// </summary>
        ///// <param name="destination"></param>
        ///// <param name="info"></param>
        //void SendMessageOnShopSuccess(long userId, MessageShopInfo info);
        /// <summary>
        /// 添加群发消息记录
        /// </summary>
        /// <param name="model"></param>
        public static void AddSendMessageRecord(dynamic model)
        {
            _iMessageService.AddSendMessageRecord(model);
        }
        /// <summary>
        /// 查询群发消息记录
        /// </summary>
        /// <param name="querymodel"></param>
        /// <returns></returns>
        public static ObsoletePageModel<object> GetSendMessageRecords(object querymodel)
        {
           return  _iMessageService.GetSendMessageRecords(querymodel);
        }
    }
}
