using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 广告类别
    /// </summary>
    public enum ImageAdsType
    {
        /// <summary>
        /// 原数据
        /// </summary>
        [Description("原数据")]
        Initial = 0,

        /// <summary>
        /// Banner右侧广告
        /// </summary>
        [Description("Banner右侧广告")]
        BannerAds = 1,

        /// <summary>
        /// 头部右侧广告
        /// </summary>
        [Description("Banner右侧广告")]
        HeadRightAds = 2,

        /// <summary>
        /// 首页自定义广告
        /// </summary>
        [Description("首页自定义广告")]
        Customize = 3,

        /// <summary>
        /// 人气单品
        /// </summary>
        [Description("人气单品")]
        Single = 4,

        /// <summary>
        /// 品牌推荐广告
        /// </summary>
        [Description("品牌推荐广告")]
        BrandsAds = 5,

        /// <summary>
        /// APP首页专题
        /// </summary>
        [Description("APP首页专题")]
        APPSpecial = 6,

        /// <summary>
        /// APP首页图标
        /// </summary>
        [Description("APP首页图标")]
        APPIcon = 7,
    }
}
