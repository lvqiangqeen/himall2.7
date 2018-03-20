using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    ///平台收支类型
    /// </summary>
    public enum PlatAccountType
    {
        /// <summary>
        /// 结算入帐
        /// </summary>
        [Description("佣金收入")]
        SettlementIncome = 1,
        /// <summary>
        /// 营销服务费
        /// </summary>
        [Description("营销服务费")]
        MarketingServices = 2,
        /// <summary>
        /// 佣金退还
        /// </summary>
        [Description("佣金退还")]
        PlatCommissionRefund = 3,
        /// <summary>
        /// 入驻缴费
        /// </summary>
        [Description("入驻缴费")]
        SettledPayment = 4,

    }
}
