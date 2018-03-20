using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * 注册有礼优惠券设置状态
 * **/
namespace Himall.CommonModel
{
    /// <summary>
    /// 注册有礼优惠券设置状态枚举
    /// </summary>
    public enum CouponSendByRegisterStatus : int
    {
        /// <summary>
        /// 关闭
        /// </summary>
        [Description("关闭")]
        Shut = 0,

        /// <summary>
        /// 开启
        /// </summary>
        [Description("开启")]
        Open = 1
    }
}
