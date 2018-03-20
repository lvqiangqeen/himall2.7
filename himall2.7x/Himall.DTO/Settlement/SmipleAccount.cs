using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 基础结算信息
    /// </summary>
    public class SmipleAccount
    {
        public System.DateTime StartDate { get; set; }

        public System.DateTime EndDate { get; set; }

        public long AccountId { set; get; }

        /// <summary>
        /// 结算周期字符串表示
        /// </summary>
        public string BillingCycleStr
        {
            get
            {
                return StartDate.ToString("MM/dd") + "-" + EndDate.ToString("MM/dd");
            }
        }
    }
}
