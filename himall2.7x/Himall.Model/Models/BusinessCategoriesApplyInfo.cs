using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class BusinessCategoriesApplyInfo
    {
        /// <summary>
        /// 审核状态
        /// </summary>
        public enum BusinessCateApplyStatus
        {
            /// <summary>
            /// 待审核
            /// </summary>

            [Description("待审核")]
            UnAudited = 0,
            /// <summary>
            /// 未结算
            /// </summary>
            [Description("已审核")]
            Audited =1,

            /// <summary>
            /// 已结算
            /// </summary>
            [Description("已拒绝")]
            Refused = 2
        }
    }
}
