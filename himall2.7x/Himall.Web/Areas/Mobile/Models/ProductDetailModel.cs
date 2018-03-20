using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Mobile.Models
{
    public class ProductDetailModel
    {
        public ProductDetailModel()
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

        public int CommentCount { get; set; }

        public int NicePercent { get; set; }

        public int Consultations { get; set; }

        public long VShopId { get; set; }

        public int CouponCount { get; set; }

        public int BonusCount { get; set; }

        public decimal BonusGrantPrice { get; set; }

        public decimal BonusRandomAmountStart { get; set; }

        public decimal BonusRandomAmountEnd { get; set; }

        public Model.CashDepositsObligation CashDepositsObligation { get; set; }

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
        public List<ProductDetailCommentModel> ProductCommentList { get; set; }
    }
}