using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 会员购买次数
    /// </summary>
    public enum Purchases : int
    {
        /// <summary>
        /// 0次
        /// </summary>
        [Description("0次")]
        ZeroTimes = 0,

        /// <summary>
        /// 1次
        /// </summary>
        [Description("1次+")]
        OneTimes = 1,

        /// <summary>
        /// 2次
        /// </summary>
        [Description("2次+")]
        TwoTimes = 2,

        /// <summary>
        /// 3次
        /// </summary>
        [Description("3次+")]
        ThreeTimes = 3,

        /// <summary>
        /// 4次
        /// </summary>
        [Description("4次+")]
        FourTimes = 4,
    }
}
