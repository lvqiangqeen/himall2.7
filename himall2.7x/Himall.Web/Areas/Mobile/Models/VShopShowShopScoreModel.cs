using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Mobile.Models
{
    public class VShopShowShopScoreModel
    {
        public string ShopName { get; set; }
        public long ShopId { get; set; }
        /// <summary>
        /// 店铺logo
        /// </summary>
        public string VShopLog { get; set; }
        public int ProductNum { get; set; }
        /// <summary>
        /// 店铺是否已收藏
        /// </summary>
        public bool IsFavoriteShop { get; set; }
        /// <summary>
        /// 店铺收藏数
        /// </summary>
        public int FavoriteShopCount { get; set; }
        

        public long VShopId { get; set; }

        public decimal ProductAndDescription { get; set; }

        public decimal ProductAndDescriptionPeer { get; set; }

        public decimal ProductAndDescriptionMin { get; set; }

        public decimal ProductAndDescriptionMax { get; set; }

        public decimal SellerServiceAttitude { get; set; }

        public decimal SellerServiceAttitudePeer { get; set; }

        public decimal SellerServiceAttitudeMax { get; set; }

        public decimal SellerServiceAttitudeMin { get; set; }

        public decimal SellerDeliverySpeed { get; set; }

        public decimal SellerDeliverySpeedPeer { get; set; }

        public decimal SellerDeliverySpeedMax { get; set; }

        public decimal sellerDeliverySpeedMin { get; set; }
    }
}