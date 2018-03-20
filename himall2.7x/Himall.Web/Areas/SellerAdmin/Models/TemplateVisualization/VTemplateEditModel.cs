using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class VTemplateEditModel
    {
        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 模板类型
        /// </summary>
        public VTemplateClientTypes ClientType { get; set; }
        /// <summary>
        /// 是否显示标头
        /// </summary>
        public bool IsShowTitle { get; set; }
        /// <summary>
        /// 是否显示标签
        /// </summary>
        public bool IsShowTags { get; set; }
        /// <summary>
        /// 是否显示图标
        /// </summary>
        public bool IsShowIcon { get; set; }
        /// <summary>
        /// 是否显示保存关预览
        /// </summary>
        public bool IsShowPrvPage { get; set; }
        /// <summary>
        /// 商铺编号
        /// </summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 微商铺编号
        /// </summary>
        public long VShopId { get; set; }
    }
}