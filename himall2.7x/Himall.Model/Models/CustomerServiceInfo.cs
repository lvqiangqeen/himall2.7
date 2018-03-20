using System.ComponentModel;

namespace Himall.Model
{
    public partial class CustomerServiceInfo
    {
        /// <summary>
        /// 客服工具
        /// </summary>
        public enum ServiceTool
        {
            /// <summary>
            /// QQ
            /// </summary>
            [Description("QQ")]
            QQ = 1,

            /// <summary>
            /// 旺旺
            /// </summary>
            [Description("旺旺")]
            Wangwang,

			/// <summary>
			/// 美洽
			/// </summary>
			[Description("美洽")]
			MeiQia,
        }

        public enum ServiceType
        {
            /// <summary>
            /// 售前
            /// </summary>
            [Description("售前")]
            PreSale = 1,

            /// <summary>
            /// 售后
            /// </summary>
            [Description("售后")]
            AfterSale
        }

        public enum ServiceTerminalType
        {
            /// <summary>
            /// PC
            /// </summary>
            [Description("PC")]
            PC = 0,

            /// <summary>
            /// Mobile
            /// </summary>
            [Description("Mobile")]
            Mobile = 1,

            /// <summary>
            /// ALL （主平台客服使用）
            /// </summary>
            [Description("All")]
            All
        }

        public enum ServiceStatusType
        {
            /// <summary>
            /// 关闭
            /// </summary>
            [Description("关闭")]
            Close = 0,

            /// <summary>
            /// 开启
            /// </summary>
            [Description("开启")]
            Open = 1
        }
    }
}
