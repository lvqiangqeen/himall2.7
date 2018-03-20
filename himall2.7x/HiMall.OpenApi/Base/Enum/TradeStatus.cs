using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.OpenApi
{
    public enum TradeStatus
    {
         /// <summary>
        /// 正常状态
        /// </summary>
        [Description("正常状态")]
        Trade_NORMAL = 1,
        /// <summary>
        /// 等待买家付款
        /// </summary>
        [Description("等待买家付款")]
        WAIT_BUYER_PAY,
        /// <summary>
        /// 等待卖家发货
        /// </summary>
        [Description("等待卖家发货")]
        WAIT_SELLER_SEND_GOODS,
        /// <summary>
        /// 等待买家确认收货
        /// </summary>
        [Description("等待买家确认收货")]
        WAIT_BUYER_CONFIRM_GOODS,
        /// <summary>
        /// 等待买家提货
        /// </summary>
        [Description("等待买家提货")]
        WAIT_BUYER_TAKE_GOODS,
        /// <summary>
        /// 交易关闭
        /// </summary>
        [Description("交易关闭")]
        TRADE_CLOSED,
        /// <summary>
        /// 交易成功
        /// </summary>
        [Description("交易成功")]
        TRADE_FINISHED,
        /// <summary>
        /// 申请退款
        /// </summary>
        [Description("申请退款")]
        TRADE_APPLY_FOR_REFUND,
        /// <summary>
        /// 申请退货
        /// </summary>
        [Description("申请退货")]
        TRADE_APPLY_FOR_RETURN,
        /// <summary>
        /// 等待退货用户发货
        /// </summary>
        [Description("等待退货用户发货")]
        WAIT_RETURN_BUYER_SEND_GOODS,
        /// <summary>
        /// 退货用户已发货，等待商家收货
        /// </summary>
        [Description("退货用户已发货，等待商家收货")]
        WAIT_RETURN_SELLER_GOODS,
        /// <summary>
        /// 商家确认收货
        /// </summary>
        [Description("商家确认收货")]
        WAIT_REFUND_SELLER_CONFIRM_GOODS,
        /// <summary>
        /// 申请换货
        /// </summary>
        [Description("申请换货")]
        TRADE_APPLY_FOR_REPLACE,
        /// <summary>
        /// 商家已同意换货, 待用户发货
        /// </summary>
        [Description("商家已同意换货, 待用户发货")]
        WAIT_REPLACE_BUYER_SEND_GOODS,
        /// <summary>
        /// 申请换货的用户已发货，待商家确认收到货
        /// </summary>
        [Description("申请换货的用户已发货，待商家确认收到货")]
        WAIT_REPLACE_SELLER_GOODS,
        /// <summary>
        /// 商家换货，待用户确认收货
        /// </summary>
        [Description("商家换货，待用户确认收货")]
        WAIT_REPLACE_BUYER_CONFIRM_GOODS,
        /// <summary>
        /// 退款成功
        /// </summary>
        [Description("退款成功")]
        TRADE_REFUND_FINISHED,
        /// <summary>
        /// 退货成功
        /// </summary>
        [Description("退货成功")]
        TRADE_RETURNED_FINISHED,
        /// <summary>
        /// 换货成功
        /// </summary>
        [Description("换货成功")]
        TRADE_REPLACE_FINISHED,
        /// <summary>
        /// 退货申请被拒绝
        /// </summary>
        [Description("退货申请被拒绝")]
        TRADE_REFUND_REFUSED,
        /// <summary>
        /// 退款申请被拒绝
        /// </summary>
        [Description("退款申请被拒绝")]
        TRADE_RETURN_REFUSED,
        /// <summary>
        /// 换货申请被拒绝
        /// </summary>
        [Description("换货申请被拒绝")]
        TRADE_REPLACE_REFUSED,
    }
}
