using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
namespace Himall.CommonModel
{
    /// <summary>
    /// 微信活动奖品类型
    /// </summary>
    public enum WeiActivityAwardType : int
    {
        /// <summary>
        /// 积分
        /// </summary>
        [Description("积分")]
        Integral = 0,

        /// <summary>
        /// 红包
        /// </summary>
        [Description("红包")]
        Bonus = 1,

        /// <summary>
        /// 优惠卷
        /// </summary>
        [Description("优惠卷")]
        Coupon = 2,
    }

}
