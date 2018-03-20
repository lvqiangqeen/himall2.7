using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core.Plugins.Message
{
    public enum MessageTypeEnum
    {
        /// <summary>
        /// 订单创建时
        /// </summary>
        [Description("订单创建时")]
        OrderCreated = 1,
        /// <summary>
        /// 订单付款时
        /// </summary>
        [Description("订单付款时")]
        OrderPay,
        /// <summary>
        /// 订单发货时
        /// </summary>
        [Description("订单发货")]
        OrderShipping,
        /// <summary>
        /// 订单退款
        /// </summary>
        [Description("订单退款")]
        OrderRefund,
        /// <summary>
        /// 售后发货
        /// </summary>
        [Description("售后发货")]
        RefundDeliver,

        [Description("找回密码")]
        FindPassWord,

        [Description("店铺审核")]
        ShopAudited,

        /// <summary>
        /// 发送优惠券
        /// </summary>
        [Description("发送优惠券通知")]
        SendCouponSuccess,

        //[Description("开店成功")] //2.4去除状态
        //ShopSuccess,
        /// <summary>
        /// 店铺有新订单
        /// </summary>
        [Description("店铺有新订单")]
        ShopHaveNewOrder,
        /// <summary>
        /// 领取红包通知
        /// </summary>
        [Description("领取红包通知")]
        ReceiveBonus,
        /// <summary>
        /// 限时购开始通知
        /// </summary>
        [Description("限时购开始通知")]
        LimitTimeBuy,
        /// <summary>
        /// 订阅限时购
        /// </summary>
        [Description("订阅限时购")]
        SubscribeLimitTimeBuy,

        #region 拼团
        /// <summary>
        /// 拼团：开团成功
        /// </summary>
        [Description("拼团：开团成功")]
        FightGroupOpenSuccess,
        /// <summary>
        /// 拼团：参团成功
        /// </summary>
        [Description("拼团：参团成功")]
        FightGroupJoinSuccess,
        /// <summary>
        /// 拼团：有新团员提示
        /// <para>只提示给团长</para>
        /// </summary>
        [Description("拼团：有新团员提示")]
        FightGroupNewJoin,
        /// <summary>
        /// 拼团：拼团失败
        /// </summary>
        [Description("拼团：拼团失败")]
        FightGroupFailed,
        /// <summary>
        /// 拼团：拼团成功
        /// </summary>
        [Description("拼团：拼团成功")]
        FightGroupSuccess,
        #endregion

      
    }
}
