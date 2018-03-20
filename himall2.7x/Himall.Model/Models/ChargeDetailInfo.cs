using System.ComponentModel;
using System.Configuration;

namespace Himall.Model
{
    public partial class ChargeDetailInfo
    {

        /// <summary>
        /// 申请退款状态
        /// </summary>
        public enum ChargeDetailStatus
        {

            /// <summary>
            /// 未付款
            /// </summary>
            [Description("未付款")]
            WaitPay = 1,

            /// <summary>
            /// 充值成功
            /// </summary>
            [Description("充值成功")]
            ChargeSuccess = 2

        }

    }
}
