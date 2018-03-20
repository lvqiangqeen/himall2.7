using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 最近消费时间
    /// </summary>
    public enum RecentlySpentTime : int
    {
        /// <summary>
        /// 1周内
        /// </summary>
        [Description("1周内")]
        OneWeek = 0,

        /// <summary>
        /// 2周内
        /// </summary>
        [Description("2周内")]
        TwoWeek = 1,

        /// <summary>
        /// 1个月内
        /// </summary>
        [Description("1个月内")]
        OneMonthWithin = 2,

        /// <summary>
        /// 1个月前
        /// </summary>
        [Description("1个月前")]
        OneMonth = 3,


        /// <summary>
        /// 2个月前
        /// </summary>
        [Description("2个月前")]
        TwoMonth = 4,

        /// <summary>
        /// 3个月前
        /// </summary>
        [Description("3个月前")]
        ThreeMonth = 5,

        /// <summary>
        /// 6个月前
        /// </summary>
        [Description("6个月前")]
        SixMonth = 6
    }
}
