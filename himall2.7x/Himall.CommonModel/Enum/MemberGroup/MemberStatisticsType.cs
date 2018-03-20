using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 会员分组统计类型
    /// </summary>
    public enum MemberStatisticsType : int
    {
        #region 活跃会员
        /// <summary>
        /// 一个月活跃会员
        /// </summary>
        [Description("一个月活跃会员")]
        ActiveOne = 0,

        /// <summary>
        /// 三个月活跃会员
        /// </summary>
        [Description("三个月活跃会员")]
        ActiveThree = 1,

        /// <summary>
        /// 六个月活跃会员
        /// </summary>
        [Description("六个月活跃会员")]
        ActiveSix = 2,
        #endregion

        #region 沉睡会员
        /// <summary>
        /// 三个月沉睡会员
        /// </summary>
        [Description("三个月沉睡会员")]
        SleepingThree = 10,

        /// <summary>
        /// 六个月沉睡会员
        /// </summary>
        [Description("六个月沉睡会员")]
        SleepingSix = 11,

        /// <summary>
        /// 九个月沉睡会员
        /// </summary>
        [Description("九个月沉睡会员")]
        SleepingNine = 12,

        /// <summary>
        /// 十二个月沉睡会员
        /// </summary>
        [Description("十二个月沉睡会员")]
        SleepingTwelve = 13,

        /// <summary>
        /// 二十四个月沉睡会员
        /// </summary>
        [Description("二十四个月沉睡会员")]
        SleepingTwentyFour = 14,
        #endregion

        #region 生日会员
        /// <summary>
        /// 今日生日会员
        /// </summary>
        [Description("今日生日会员")]
        BirthdayToday = 100,

        /// <summary>
        /// 今月生日会员
        /// </summary>
        [Description("今月生日会员")]
        BirthdayToMonth = 101,

        /// <summary>
        /// 下月生日会员
        /// </summary>
        [Description("下月生日会员")]
        BirthdayNextMonth = 102,
        #endregion

        #region 注册会员
        /// <summary>
        /// 注册会员
        /// </summary>
        [Description("注册会员")]
        RegisteredMember = 1000
        #endregion
    }
}
