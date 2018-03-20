using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public class SettlementCycle
    {
        /// <summary>
        /// 结算开始时间
        /// </summary>
        public DateTime StartTime { set; get; }

        /// <summary>
        /// 结算结束时间
        /// </summary>
        public DateTime EndTime { set; get; }

        /// <summary>
        /// 结算周期字符串表示
        /// </summary>
        public string BillingCycleStr
        {
            get
            {
                return StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "--" + EndTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }

    /// <summary>
    /// 平台待结算列表显示的汇总信息
    /// </summary>
    public class PlatSettlementCycle : SettlementCycle
    {
        /// <summary>
        /// 平台佣金总额
        /// </summary>
        public decimal PlatCommission { set; get; }
    }

    /// <summary>
    /// 店铺待结算列表显示的汇总信息
    /// </summary>
    public class ShopSettlementCycle : SettlementCycle
    {
        /// <summary>
        /// 待结算总额
        /// </summary>
        public decimal PendingSettlement { set; get; }
    }
}
