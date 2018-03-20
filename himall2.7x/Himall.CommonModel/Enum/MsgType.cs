using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MsgType : int
    {
        /// <summary>
        /// 微信
        /// </summary>
        [Description("微信")]
        WeiXin = 0,
        /// <summary>
        /// 邮件
        /// </summary>
        [Description("邮件")]
        Email = 1,
        /// <summary>
        /// 优惠券
        /// </summary>
        [Description("优惠券")]
        Coupon = 2,

         /// <summary>
        /// 短信
        /// </summary>
        [Description("短信")]
        SMS = 3
    }
}
