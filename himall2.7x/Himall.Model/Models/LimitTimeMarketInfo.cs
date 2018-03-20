using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class LimitTimeMarketInfo
    {
        /// <summary>
        /// 审核状态
        /// </summary>
        public enum LimitTimeMarketAuditStatus:short
        {
            /// <summary>
            /// 等待审核
            /// </summary>
            [Description("待审核")]
            WaitForAuditing = 1,

            /// <summary>
            /// 进行中
            /// </summary>
            [Description("进行中")]
            Ongoing,

            /// <summary>
            /// 未通过(审核失败)
            /// </summary>
            [Description("未通过")]
            AuditFailed,

            /// <summary>
            /// 已结束
            /// </summary>
            [Description("已结束")]
            Ended,

            /// <summary>
            /// 已取消
            /// </summary>
            [Description("已取消")]
            Cancelled
        }

    }
}
