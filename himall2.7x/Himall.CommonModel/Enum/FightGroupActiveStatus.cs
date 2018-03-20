using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Himall.CommonModel
{
    /// <summary>
    /// 活动状态
    /// <para>实体中以开始结束时间自动计算</para>
    /// </summary>
    public enum FightGroupActiveStatus:int
    {
        /// <summary>
        /// 已结束
        /// </summary>
        [Description("已结束")]
        Ending = -1,
        /// <summary>
        /// 正在进行
        /// </summary>
        [Description("正在进行")]
        Ongoing = 0,
        /// <summary>
        /// 即将开始
        /// </summary>
        [Description("即将开始")]
        WillStart = 1,
    }
}
