using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class BrokerageIncomeInfo
    {
        /// <summary>
        /// 结算状态
        /// </summary>
        public enum BrokerageStatus
        {
            /// <summary>
            /// 不可结算
            /// </summary>

            [Description("不可结算")]
            NotAvailable = -1,
            /// <summary>
            /// 未结算
            /// </summary>
            [Description("未结算")]
            NotSettled = 0,

            /// <summary>
            /// 已结算
            /// </summary>
            [Description("已结算")]
            Settled = 1
        }
    }
}
