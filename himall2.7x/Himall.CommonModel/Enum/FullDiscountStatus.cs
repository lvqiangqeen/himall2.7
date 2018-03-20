using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Himall.CommonModel
{
    /// <summary>
    /// 满减活动状态
    /// </summary>
    public enum FullDiscountStatus
    {
        /// <summary>
        /// 进行中
        /// </summary>
        [Description("进行中")]
        Ongoing = 1,
        /// <summary>
        /// 未开始
        /// </summary>
        [Description("未开始")]
        WillStart = 0,
        /// <summary>
        /// 已结束
        /// </summary>
        [Description("已结束")]
        Ending = -1,
    }
}
