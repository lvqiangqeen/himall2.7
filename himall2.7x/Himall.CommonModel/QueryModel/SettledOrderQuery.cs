using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 已结算订单查询实体
    /// </summary>
    public class SettlementOrderQuery:PendingSettlementOrderQuery
    {      
        public DateTime? SettleStart { set; get; }
        /// <summary>
        /// 订单结算结束时间
        /// </summary>
        public DateTime? SettleEnd { set; get; }

        /// <summary>
        /// 结算周期Id(查询某个结算周期的明细)
        /// </summary>
        public long? WeekSettlementId { set; get; }
    }
}
