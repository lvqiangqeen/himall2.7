using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public enum ValuationMethodType
    {
        /// <summary>
        /// 按件数
        /// </summary>
        [Description("按件数")]
        Piece,

        /// <summary>
        /// 按重量
        /// </summary>
        [Description("按重量")]
        Weight,
        /// <summary>
        /// 按体积
        /// </summary>
        [Description("按体积")]
        Bulk
    }
    public enum FreightTemplateType
    {
        /// <summary>
        /// 自定义
        /// </summary>
        [Description("自定义模板")]
        SelfDefine,
        /// <summary>
        /// 卖家承担运费(免运费)
        /// </summary>
        [Description("卖家承担运费")]
        Free
    }
    /// <summary>
    /// 发货时间
    /// </summary>
    public enum SendTimeEnum
    {
        /// <summary>
        /// 4小时
        /// </summary>
        [Description("4小时")]
        FourHours = 4,
        /// <summary>
        /// 8小时
        /// </summary>
        [Description("8小时")]
        EightHours = 8,
        /// <summary>
        /// 12小时
        /// </summary>
        [Description("12小时")]
        TwelveHours = 12,
        /// <summary>
        /// 1天内
        /// </summary>
        [Description("1天内")]
        OneDay = 24,
        /// <summary>
        /// 2天内
        /// </summary>
        [Description("2天内")]
        TwoDay = 48,
        /// <summary>
        /// 3天内
        /// </summary>
        [Description("3天内")]
        ThreeDay = 72,
        /// <summary>
        /// 5天内
        /// </summary>
        [Description("5天内")]
        FiveDay = 120,
        /// <summary>
        /// 8天内
        /// </summary>
        [Description("8天内")]
        EightDay = 192,
        /// <summary>
        /// 10天内
        /// </summary>
        [Description("10天内")]
        TenDay = 240,
        /// <summary>
        /// 15天内
        /// </summary>
        [Description("15天内")]
        FifteenDay = 360,
        /// <summary>
        /// 17天内
        /// </summary>
        [Description("17天内")]
        SeventeenDay = 408,
        /// <summary>
        /// 20天内
        /// </summary>
        [Description("20天内")]
        TwentyDay = 480,
        /// <summary>
        /// 25天内
        /// </summary>
        [Description("25天内")]
        TwentyFiveDay = 600,
        /// <summary>
        /// 30天内
        /// </summary>
        [Description("30天内")]
        ThirtyDay = 720
    }   
}
