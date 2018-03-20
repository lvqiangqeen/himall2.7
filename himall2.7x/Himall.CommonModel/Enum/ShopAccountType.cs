using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 店铺收支类型
    /// </summary>
    public enum ShopAccountType
    {
        /// <summary>
        /// 结算入帐
        /// </summary>
        [Description("结算入帐")]
        SettlementIncome = 1,

        /// <summary>
        /// 退款
        /// </summary>
        [Description("退款")]
        Refund = 2,
        /// <summary>
        /// 平台佣金退还
        /// </summary>
        [Description("平台佣金退还")]
        PlatCommissionRefund = 3,   
        /// <summary>
        /// 分销佣金退还
        /// </summary>
        [Description("分销佣金退还")]
        DistributorCommissionRefund = 4,
        /// <summary>
        /// 营销服务费
        /// </summary>
        [Description("营销服务费")]
        MarketingServices = 5,
        /// <summary>
        /// 提现
        /// </summary>
        [Description("提现")]
        WithDraw = 6,
        /// <summary>
        /// 充值
        /// </summary>
        [Description("充值")]
        Recharge = 7
    }
}
