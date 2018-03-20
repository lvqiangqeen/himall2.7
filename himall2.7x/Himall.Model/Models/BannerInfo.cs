using System.ComponentModel;

namespace Himall.Model
{
    public partial class BannerInfo
    {
        /// <summary>
        /// Url所属类型
        /// </summary>
        public enum BannerUrltypes
        {
            /// <summary>
            /// 链接
            /// </summary>
            [Description("链接")]
            Link=0,

            /// <summary>
            /// 全部商品
            /// </summary>
            [Description("全部商品")]
            AllProducts=1,

            /// <summary>
            /// 商品分类
            /// </summary>
            [Description("商品分类")]
            Category=2,

            /// <summary>
            /// 店铺简介
            /// </summary>
            [Description("店铺简介")]
            VShopIntroduce=3
        }

    }
}
