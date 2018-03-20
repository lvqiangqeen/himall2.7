using System.ComponentModel;

namespace Himall.Model
{
    public partial class ProductDescriptionTemplateInfo
    {
        /// <summary>
        /// 模板位置
        /// </summary>
        public enum TemplatePosition
        {
            /// <summary>
            /// 顶部
            /// </summary>
            [Description("顶部")]
            Top = 1,

            /// <summary>
            /// 底部
            /// 
            /// </summary>
            [Description("底部")]
            Bottom
        }
    }
}
