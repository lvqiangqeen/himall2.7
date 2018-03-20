using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Mobile.Models
{
    public class LimitTimeBuyDetailModel
    {
        public LimitTimeBuyDetailModel()
        {
            ProductCommentList = new List<ProductDetailCommentModel>();
        }

        public int ProductNum { get; set; }

        /// <summary>
        /// 商品是否已收藏
        /// </summary>
        public bool IsFavorite { get; set; }
        /// <summary>
        /// 店铺是否已收藏
        /// </summary>
        public bool IsFavoriteShop { get; set; }
        /// <summary>
        /// 店铺收藏数
        /// </summary>
        public int FavoriteShopCount { get; set; }

        public string Price { get; set; }

        public  List<TypeAttributesModel> ProductAttrs { get; set; }

        public int CommentCount { get; set; }

        public int Consultations { get; set; }

        public double NicePercent { get; set; }

        public long VShopId { get; set; }

        public int Logined { get; set; }

        public bool EnabledBuy { get; set; }

        public double Second { get; set; }

        //宝贝描述
        public decimal ProductAndDescription { get; set; }
        public decimal ProductAndDescriptionPeer { get; set; }
        public decimal ProductAndDescriptionMin { get; set; }
        public decimal ProductAndDescriptionMax { get; set; }

        //卖家服务态度
        public decimal SellerServiceAttitude { get; set; }
        public decimal SellerServiceAttitudePeer { get; set; }
        public decimal SellerServiceAttitudeMax { get; set; }
        public decimal SellerServiceAttitudeMin { get; set; }

        //卖家发货速度
        public decimal SellerDeliverySpeed { get; set; }
        public decimal SellerDeliverySpeedPeer { get; set; }
        public decimal SellerDeliverySpeedMax { get; set; }
        public decimal sellerDeliverySpeedMin { get; set; }
        public List<ProductDetailCommentModel> ProductCommentList { get; set; }
    }
}