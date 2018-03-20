using System.Collections.Generic;

namespace Himall.Core.Plugins.Message
{
    public interface IMessagePlugin : IPlugin
    {
        /// <summary>
        /// 获取表单数据
        /// </summary>
        FormData GetFormData();

        /// <summary>
        /// 设置表单数据
        /// </summary>
        /// <param name="values">表单数据键值对集合，键为表单项的name,值为用户填写的值</param>
        void SetFormValues(IEnumerable<KeyValuePair<string, string>> values);


        /// <summary>
        /// Logo图片路径
        /// </summary>
        string Logo {get;}

        /// <summary>
        /// 简称
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// 是否开启记录
        /// </summary>
        bool EnableLog{ get; }

        /// <summary>
        /// 发送验证码
        /// </summary>
        string SendMessageCode(string destination, MessageUserInfo info);

        /// <summary>
        /// 发送测试信息
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="content"></param>
       /// <param name="title">邮件标题，短信可不填写</param>
        string SendTestMessage(string destination, string content,string title = "");

        /// <summary>
        /// 批量发送消息
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="content"></param>
        /// <param name="title">邮件标题，短信可不填写</param>
        void SendMessages(string[] destination, string content, string title = "");

        /// <summary>
        /// 店铺审核通过
        /// </summary>
        /// <param name="sellerName"></param>
        /// <param name="shopName"></param>
        string SendMessageOnShopAudited(string destination, MessageShopInfo info);

        /// <summary>
        /// 店铺开店成功
        /// </summary>
        /// <param name="sellerName"></param>
        /// <param name="shopName"></param>
        string SendMessageOnShopSuccess(string destination, MessageShopInfo info);

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="code"></param>
        string SendMessageOnFindPassWord(string destination, MessageUserInfo info);

        /// <summary>
        /// 订单创建
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="?"></param>
        string SendMessageOnOrderCreate(string destination, MessageOrderInfo info);

        /// <summary>
        /// 订单支付
        /// </summary>
        /// 
        /// 
        /// <param name="destination"></param>
        /// <param name="info"></param>
        string SendMessageOnOrderPay(string destination, MessageOrderInfo info);

        /// <summary>
        /// 订单发货
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        string SendMessageOnOrderShipping(string destination, MessageOrderInfo info);

        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        string SendMessageOnOrderRefund(string destination, MessageOrderInfo info);

        /// <summary>
        /// 售后发货
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        string SendMessageOnRefundDeliver(string destination, MessageOrderInfo info);


        /// <summary>
        /// 优惠券发送成功提醒
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnCouponSuccess(string destination, MessageCouponInfo info);

        /// <summary>
        /// 关闭某项信息的发送
        /// </summary>
        /// <param name="e"></param>
        void Disable(MessageTypeEnum e);
        /// <summary>
        /// 开启某项信息的发送
        /// </summary>
        /// <param name="e"></param>
        void Enable(MessageTypeEnum e);

        /// <summary>
        /// 获取某项信息的状态
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
       StatusEnum GetStatus(MessageTypeEnum e);
        /// <summary>
        /// 获取所有项的状态
        /// </summary>
        /// <returns></returns>
       Dictionary<MessageTypeEnum,StatusEnum> GetAllStatus();

       ///批量设置所有项
       ///
       void SetAllStatus(Dictionary<MessageTypeEnum,StatusEnum> dic);

        /// <summary>
        /// 检查发送格式
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
       bool CheckDestination(string destination);

       /// <summary>
       /// 是否已设置完成
       /// </summary>
       bool IsSettingsValid { get; }
    }
}
