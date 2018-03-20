using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Himall.CommonModel
{
    /// <summary>
    /// 拼团成团状态
    /// </summary>
    public enum FightGroupBuildStatus : int
    {
        /// <summary>
        /// 开团中
        /// <para>团长订单未付款</para>
        /// </summary>
        [Description("等待开团")]
        Opening = -10,
        /// <summary>
        /// 成团失败
        /// </summary>
        [Description("拼团失败")]
        Failed = -1,
        /// <summary>
        /// 成团中
        /// </summary>
        [Description("成团中")]
        Ongoing = 0,
        /// <summary>
        /// 成团成功
        /// </summary>
        [Description("拼团成功")]
        Success = 1,
    }
}
