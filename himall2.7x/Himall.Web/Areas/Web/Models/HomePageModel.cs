using Himall.Model;
using System.Collections.Generic;

namespace Himall.Web.Areas.Web.Models
{
    /// <summary>
    /// 首页数据
    /// </summary>
    public class HomePageModel
    {

        public List<string> OAuthValidateContents { set; get; }

        public string SiteName { get; set; }

        public string Title { get; set; }

        public List<HandSlideAdInfo> handImage { get; set; }

        public List<SlideAdInfo> slideImage { get; set; }

        /// <summary>
        /// 人气单品
        /// </summary>
        public List<ImageAdInfo> imageAds { get; set; }

        /// <summary>
        /// banner右侧广告
        /// </summary>
        public List<ImageAdInfo> imageAdsTop { get; set; }

        /// <summary>
        /// 中间广告
        /// </summary>
        public List<ImageAdInfo> CenterAds { get; set; }

        /// <summary>
        /// 商家推荐广告
        /// </summary>
        public List<ImageAdInfo> ShopAds { get; set; }

        /// <summary>
        /// 楼层循环
        /// </summary>
        public List<HomeFloorModel> floorModels { get; set; }

        /// <summary>
        /// 品牌旗舰店
        /// </summary>
        public HomeBrands brands { get; set; }

        /// <summary>
        /// 限时购
        /// </summary>
        public List<FlashSaleModel> FlashSaleModel { get; set; }

		public string AdvertisementUrl { get; set; }

		public string AdvertisementImagePath { get; set; }
	}
}
