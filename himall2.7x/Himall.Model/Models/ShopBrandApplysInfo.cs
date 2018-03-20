using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class ShopBrandApplysInfo
    {
        /// <summary>
        /// 审核状态
        /// </summary>
        public enum BrandAuditStatus
        {
            /// <summary>
            /// 未审核
            /// </summary>
            [Description("未审核")]
            UnAudit = 0,

            /// <summary>
            /// 通过审核
            /// </summary>
            [Description("通过审核")]
            Audited = 1,

            /// <summary>
            /// 拒绝通过
            /// </summary>
            [Description("拒绝通过")]
            Refused = 2
        }

        /// <summary>
        /// 申请品牌类型
        /// </summary>
        public enum BrandApplyMode
        {
            /// <summary>
            /// 平台已有品牌
            /// </summary>
            [Description("平台已有品牌")]
            Exist = 1,

            /// <summary>
            /// 新品牌
            /// </summary>
            [Description("新品牌")]
            New = 2
        }
    }
}
