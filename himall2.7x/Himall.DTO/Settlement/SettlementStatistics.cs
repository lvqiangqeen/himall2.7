using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 已结算统计
    /// </summary>
    public class SettlementStatistics
    {
        /// <summary>
        /// 结算总额
        /// </summary>
        public decimal SettlementAmount { set; get; }

        /// <summary>
        /// 分销佣金总额
        /// </summary>
        public decimal DistributorCommission { set; get; }

        /// <summary>
        /// 平台佣金总额
        /// </summary>
        public decimal PlatCommission { set;get;}
    }

    /// <summary>
    /// 平台已结算统计
    /// </summary>
    public class PlatSettlementStatistics : SettlementStatistics
    {
        /// <summary>
        /// 入帐总额
        /// </summary>
        public decimal PlatRecordAmount { get { return SettlementAmount + DistributorCommission + PlatCommission; } }
    }
}
