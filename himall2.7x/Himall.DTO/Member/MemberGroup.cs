using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 会员分组信息
    /// </summary>
    public class MemberGroup
    {
        #region 活跃会员
        /// <summary>
        /// 一个月活跃会员
        /// </summary>
        public int ActiveOne { get; set; }

        /// <summary>
        /// 三个月活跃会员
        /// </summary>
        public int ActiveThree { get; set; }

        /// <summary>
        /// 六个月活跃会员
        /// </summary>
        public int ActiveSix { get; set; }
        #endregion

        #region 沉睡会员
        /// <summary>
        /// 三个月沉睡会员
        /// </summary>
        public int SleepingThree { get; set; }

        /// <summary>
        /// 六个月沉睡会员
        /// </summary>
        public int SleepingSix { get; set; }

        /// <summary>
        /// 九个月沉睡会员
        /// </summary>
        public int SleepingNine { get; set; }

        /// <summary>
        /// 十二个月沉睡会员
        /// </summary>
        public int SleepingTwelve { get; set; }

        /// <summary>
        /// 二十四个月沉睡会员
        /// </summary>
        public int SleepingTwentyFour { get; set; }
        #endregion

        #region 生日会员
        /// <summary>
        /// 今日生日会员
        /// </summary>
        public int BirthdayToday { get; set; }

        /// <summary>
        /// 今月生日会员
        /// </summary>
        public int BirthdayToMonth { get; set; }

        /// <summary>
        /// 下月生日会员
        /// </summary>
        public int BirthdayNextMonth { get; set; }
        #endregion

        #region 注册会员
        /// <summary>
        /// 注册会员
        /// </summary>
        public int RegisteredMember { get; set; }
        #endregion
    }
}
