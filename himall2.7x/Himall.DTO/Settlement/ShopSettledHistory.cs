using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public class ShopSettledHistory
    {
        /// <summary>
        /// 结算周期Id
        /// </summary>
        public long AccountId { set; get; }
        public DateTime StartTime { set; get; }

        public DateTime EndTime { set; get; }

        public DateTime AccountTime { set; get; }
        /// <summary>
        /// 结算金额
        /// </summary>
        public decimal SettlementAmount { set; get; }

        /// <summary>
        /// 结算周期字符串表示
        /// </summary>
        public string BillingCycleStr
        {
            get
            {
                return StartTime.ToString("MM/dd") + "-" + EndTime.ToString("MM/dd");
            }
        }

        public string AccountTimeStr
        {
            get
            {
                return AccountTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }
}
