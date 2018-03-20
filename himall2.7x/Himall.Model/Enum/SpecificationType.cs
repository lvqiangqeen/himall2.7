using System.ComponentModel;

namespace Himall.Model
{
    public enum SpecificationType:int
    {
        /// <summary>
        /// 颜色
        /// </summary>
        [Description("颜色")]
        Color = 1,

        /// <summary>
        /// 尺码
        /// </summary>
        [Description("尺码")]
        Size,

        /// <summary>
        /// 规格
        /// </summary>
        [Description("规格")]
        Version
    }
}
