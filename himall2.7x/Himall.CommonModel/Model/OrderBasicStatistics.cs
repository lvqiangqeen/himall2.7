using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 订单基本统计
    /// </summary>
    public class OrderBasicStatistics
    {
        /// <summary>
        /// 交易次数
        /// </summary>
        public int TradeCount { get; set; }

        /// <summary>
        /// 交易金额
        /// </summary>
        public decimal TradeAmount { get; set; }

        /// <summary>
        /// 休眠时间
        /// </summary>
        public int? SleepDays { get; set; }
    }
}
