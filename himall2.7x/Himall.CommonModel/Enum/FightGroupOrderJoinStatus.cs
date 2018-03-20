using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Himall.CommonModel
{
    /// <summary>
    /// 拼团参团状态
    /// </summary>
    public enum FightGroupOrderJoinStatus : int
    {
        /// <summary>
        /// 开团中
        /// <para>团长订单未付款</para>
        /// </summary>
        [Description("开团中")]
        BuildOpening = -10,
        /// <summary>
        /// 参团失败
        /// <para>未付款等情况</para>
        /// </summary>
        [Description("参团失败")]
        JoinFailed = -1,
        /// <summary>
        /// 正在参与
        /// </summary>
        [Description("正在参与")]
        Ongoing = 0,
        /// <summary>
        /// 参团成功
        /// <para>等待其他团友，不可发货</para>
        /// </summary>
        [Description("参团成功")]
        JoinSuccess = 1,
        /// <summary>
        /// 拼团失败
        /// <para>已付款，但拼团超时人数未满</para>
        /// </summary>
        [Description("拼团失败")]
        BuildFailed = 2,
        /// <summary>
        /// 组团成功
        /// <para>可以发货</para>
        /// </summary>
        [Description("组团成功")]
        BuildSuccess = 4,
    }
}
