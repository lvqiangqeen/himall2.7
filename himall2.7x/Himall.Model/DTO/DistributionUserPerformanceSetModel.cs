using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// 分销用户业绩数据集
    /// </summary>
    public class DistributionUserPerformanceSetModel
    {
        public long UserId { get; set; }
        /// <summary>
        /// 7天总收益
        /// </summary>
        public decimal Day7SumIncome { get; set; }
        /// <summary>
        /// 7天已结收益
        /// </summary>
        public decimal Day7Settled { get; set; }
        /// <summary>
        /// 7天未结收益
        /// </summary>
        public decimal Day7NoSettled { get; set; }
        /// <summary>
        /// 累积总收益
        /// </summary>
        public decimal SumIncome { get; set; }
        /// <summary>
        /// 累积已结收益
        /// </summary>
        public decimal SumSettled { get; set; }
        /// <summary>
        /// 累积未结收益
        /// </summary>
        public decimal SumNoSettled { get; set; }
        /// <summary>
        /// 本月收益
        /// </summary>
        public decimal MonthSumIncome { get; set; }
        /// <summary>
        /// 本月已结收益
        /// </summary>
        public decimal MonthSumSettled { get; set; }
        /// <summary>
        /// 本月未结收益
        /// </summary>
        public decimal MonthSumNoSettled { get; set; }
        /// <summary>
        /// 成交总数
        /// </summary>
        public int SumOrderCount { get; set; }
        /// <summary>
        /// 月成交总数
        /// </summary>
        public int MonthSumOrderCount { get; set; }
        /// <summary>
        /// 累积客户数
        /// </summary>
        public int SumCustomer { get; set; }
        /// <summary>
        /// 本月新客户
        /// </summary>
        public int MonthNewCustomer { get; set; }

    }
}
