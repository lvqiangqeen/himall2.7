using System.ComponentModel;

namespace Himall.Model
{
    public partial class OrderComplaintInfo
    {
        /// <summary>
        /// 投诉状态
        /// </summary>
        public enum ComplaintStatus
        {
            /// <summary>
            /// 待处理
            /// </summary>
            [Description("等待商家处理")]
            WaitDeal = 1,

            /// <summary>
            /// 等待会员确认
            /// </summary>
            [Description("商家处理完成")]
            Dealed,

            /// <summary>
            /// 等待平台介入
            /// </summary>
            [Description("等待平台介入")]
            Dispute,

            /// <summary>
            /// 已结束
            /// </summary>
            [Description("已结束")]
            End
        }
    }
}
