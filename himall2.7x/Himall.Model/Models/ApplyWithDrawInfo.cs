using System.ComponentModel;
using System.Configuration;

namespace Himall.Model
{
    public partial class ApplyWithDrawInfo
    {

        /// <summary>
        /// 申请退款状态
        /// </summary>
        public enum ApplyWithDrawStatus
        {

            /// <summary>
            /// 待处理
            /// </summary>
            [Description("待处理")]
            WaitConfirm = 1,

            /// <summary>
            /// 付款失败
            /// </summary>
            [Description("付款失败")]
            PayFail = 2,

            /// <summary>
            /// 提现成功
            /// </summary>
            [Description("提现成功")]
            WithDrawSuccess = 3,

            /// <summary>
            /// 已拒绝
            /// </summary>
            [Description("已拒绝")]
            Refuse = 4
        }

    }
}
