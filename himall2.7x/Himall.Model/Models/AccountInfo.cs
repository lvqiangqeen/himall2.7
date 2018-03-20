using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class AccountInfo
    {
        /// <summary>
        /// 结算状态
        /// </summary>
        public enum AccountStatus
        {
            /// <summary>
            /// 未结算
            /// </summary>
            [Description("未结算")]
            UnAccount = 0,

            /// <summary>
            /// 已结算
            /// </summary>
            [Description("已结算")]
            Accounted
        }

        /// <summary>
        /// 结算金额
        /// </summary>
        [NotMapped]
        public decimal AccountAmount { get { return 0; /*OrderAmount - RefundAmount - CommissionAmount; */} }
    }
}
