using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Himall.CommonModel
{
    /// <summary>
    /// 拼团成团状态
    /// </summary>
    public enum FightGroupManageAuditStatus : int
    {
        /// <summary>
        /// 下架
        /// </summary>
        [Description("下架")]
        SoldOut = -1,
        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")]
        Normal = 0,
    }
}
