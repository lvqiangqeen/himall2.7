using Himall.Web.Areas.SellerAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;
using Himall.CommonModel;

namespace Himall.Web.Areas.Web.Models
{
    public class ShopHomeModel
    {
        //public List<ProductInfo> Products { get; set; }
        public List<ProductView> Products { get; set; }
        public List<CategoryJsonModel> ShopCategory { get; set; }
        public List<SlideAdInfo> Slides { get; set; }
        public string Logo { get; set; }
        public List <BannerInfo> Navignations { get; set; }
        public List<ShopHomeFloor> Floors { get; set; }
        public ShopInfoModel Shop { get; set; }
        public List<ImageAdInfo> ImageAds { get; set; }

        public List<HotProductInfo> HotSaleProducts { get; set; }
        public List<HotProductInfo> HotAttentionProducts { get; set; }

        public ImageAdInfo TransverseAD { get; set; }

        public string Footer { get; set; }

        public string VShopQR { get; set; }
    }

    public class ShopHomeFloor
    {
        public List<ShopHomeFloorProduct> Products { get; set; }

        public List<ShopHomeFloorTopImg> TopImgs { get; set; }

        public string FloorName { get; set; }

        public string FloorUrl { get; set; }
    }

    public class ShopHomeFloorProduct
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string Pic { get; set; }
        public int SaleCount { get; set; }
    }

    public class ShopHomeFloorTopImg
    {

        public string Url { get; set; }

        public string ImgPath { get; set; }
    }


}