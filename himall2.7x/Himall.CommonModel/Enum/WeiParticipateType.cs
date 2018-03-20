using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
namespace Himall.CommonModel
{
    /// <summary>
    /// 微信活动参与类型
    /// </summary>
    public enum WeiParticipateType : int
    {
        /// <summary>
        /// 活动总次数
        /// </summary>
        [Description("活动总次数")]
        CommonCount = 0,

        /// <summary>
        /// 活动天次数
        /// </summary>
        [Description("活动天次数")]
        DayCount = 1,

        /// <summary>
        /// 无限制
        /// </summary>
        [Description("无限制")]
        Unlimited = 2,
    }
}
