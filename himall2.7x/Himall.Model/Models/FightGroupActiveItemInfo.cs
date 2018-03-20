using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Himall.Core;
using Himall.CommonModel;

namespace Himall.Model
{
    public partial class FightGroupActiveItemInfo
    {
        /// <summary>
        /// 规格名称
        /// </summary>
        [NotMapped]
        public string SkuName { get; set; }
        /// <summary>
        /// 商品售价
        /// </summary>
        [NotMapped]
        public decimal ProductPrice { get; set; }
        /// <summary>
        /// 商品成本价
        /// </summary>
        [NotMapped]
        public decimal ProductCostPrice { get; set; }
        /// <summary>
        /// 已售
        ///</summary>
        [NotMapped]
        public long ProductStock { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        [NotMapped]
        public string Color { get; set; }
        /// <summary>
        /// 尺码
        /// </summary>
        [NotMapped]
        public string Size { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        [NotMapped]
        public string Version { get; set; }
        /// <summary>
        /// 显示图片
        /// <para>颜色独有</para>
        /// </summary>
        [NotMapped]
        public string ShowPic { get; set; }
    }
}
