using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinStatisticsService.Model
{
    /// <summary>
    /// 会员活动表
    /// </summary>
    public class MemberActivityDegreeInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 会员ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 一个月活跃
        /// </summary>
        public bool OneMonth { get; set; }

        /// <summary>
        /// 三个月活跃
        /// </summary>
        public bool ThreeMonth { get; set; }

        /// <summary>
        /// 六个月活跃
        /// </summary>
        public bool SixMonth { get; set; }

        /// <summary>
        /// 一个月活跃会员有效时间
        /// </summary>
        public System.DateTime? OneMonthEffectiveTime { get; set; }

        /// <summary>
        /// 三个月活跃会员有效时间
        /// </summary>
        public System.DateTime? ThreeMonthEffectiveTime { get; set; }

        /// <summary>
        /// 六个月活跃会员有效时间
        /// </summary>
        public System.DateTime? SixMonthEffectiveTime { get; set; }
    }
}
