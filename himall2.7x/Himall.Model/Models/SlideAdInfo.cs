
using System.ComponentModel;
using System.Configuration;
namespace Himall.Model
{
    public partial class SlideAdInfo
    {

        public enum SlideAdType
        {
            /// <summary>
            /// 平台首页轮播图
            /// </summary>
            [Description("平台首页轮播图")]
            PlatformHome = 1,

            /// <summary>
            /// 平台限时购轮播图
            /// </summary>
            [Description("平台限时购轮播图")]
            PlatformLimitTime=2,

            /// <summary>
            /// 店铺首页轮播图
            /// </summary>
            [Description("店铺首页轮播图")]
            ShopHome=3,

            /// <summary>
            /// 微店轮播图
            /// </summary>
            [Description("微店轮播图")]
            VShopHome=4,

            [Description("微信首页轮播图")]
            WeixinHome=5,

            /// <summary>
            /// 触屏版首页轮播图
            /// </summary>
            [Description("触屏版首页轮播图")]
            WapHome=6,

            /// <summary>
            /// 触屏版微店首页轮播图
            /// </summary>
            [Description("触屏版微店首页轮播图")]
            WapShopHome=7,

            /// <summary>
            /// IOS首页轮播图
            /// </summary>
            [Description("APP首页轮播图")]
            IOSShopHome=8,

            /// <summary>
            /// 分销首页轮播图
            /// </summary>
            [Description("分销首页轮播图")]
            DistributionHome=9,


            /// <summary>
            ///APP首页图标图
            /// </summary>
            [Description("APP首页轮播图")]
            APPIcon=10,

            /// <summary>
            /// APP积分商城轮播图
            /// </summary>
            [Description("APP积分商城轮播图")]
            AppGifts = 11,

            /// <summary>
            /// 引导页图
            /// </summary>
            [Description("APP积分商城轮播图")]
            AppGuide =12,
        }


        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImageUrl
        {
            get { return ImageServerUrl + imageUrl; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    imageUrl = value.Replace(ImageServerUrl, "");
                else
                    imageUrl = value;
            }
        }
    }
}
