using Himall.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class VshopHomeSiteViewModel
    {
        public VShopInfo VShop { get; set; }
        public long ShopId { get; set; }
        public List<SlideAdInfo> SlideImage { get; set; }
        public List<BannerInfo> Banner { get; set; }
    }
}