
using System.ComponentModel;
namespace Himall.Model
{
    /// <summary>
    /// 微店扩展信息
    /// </summary>
    public partial class VShopExtendInfo
    {
        /// <summary>
        /// 微店扩展类型
        /// </summary>
        public enum VShopExtendType
        {
            /// <summary>
            /// 主推微店
            /// </summary>
            [Description("主推微店")]
            TopShow=1,

            /// <summary>
            /// 热门微店
            /// </summary>
            [Description("热门微店")]
            HotVShop=2
        
        }

        /// <summary>
        /// 微店扩展信息状态
        /// </summary>
        public enum VShopExtendState
        {
            /// <summary>
            /// 申请未审核
            /// </summary>
            [Description("未审核")]
            NotAudit = 1,

            /// <summary>
            /// 审核通过
            /// </summary>
            [Description("审核通过")]
            Through = 2,

            /// <summary>
            /// 拒绝
            /// </summary>
            [Description("审核拒绝")]
            Refused = 3,

            /// <summary>
            /// 已关闭
            /// </summary>
            [Description("微店已关闭")]
            Close=4,

        }

    }
}
