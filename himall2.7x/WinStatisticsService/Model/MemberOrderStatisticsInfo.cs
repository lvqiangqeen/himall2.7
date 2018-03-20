using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinStatisticsService.Model
{
    /// <summary>
    /// 会员订单统计表
    /// </summary>
    public class MemberOrderStatisticsInfo : BaseModel
    {
        /// <summary>
        /// 会员ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 订单数
        /// </summary>
        public int OrderNumber { get; set; }

        /// <summary>
        /// 对应天(天数%180)
        /// </summary>
        public int Days { get; set; }
    }
}
