using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Areas.Web.Models;

namespace Himall.Web.Areas.Mobile.Models
{
    public class DistributionProductDetailShowModel
    {
        public string Title { get; set; }
        public int MaxSaleCount { get; set; }
        public ProductInfo Product { get; set; }
        public ShopInfoModel Shop { get; set; }
        public List<HotProductInfo> HotSaleProducts { get; set; }
        public List<HotProductInfo> BoughtProducts { get; set; }
        public List<HotProductInfo> HotAttentionProducts { get; set; }
        public List<CategoryJsonModel> ShopCategory { get; set; }
        public string DescriptionPrefix { get; set; }
        public string DescriptiondSuffix { get; set; }
        public string ProductDescription { get; set; }
        public CollectionSKU Color { get; set; }
        public CollectionSKU Size { get; set; }
        public CollectionSKU Version { get; set; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }

        public FlashSaleModel FlashSale { get; set; }

        public FlashSaleConfigModel FlashSaleConfig { get; set; }
        public CashDepositsObligation CashDepositsServer { get; set; }

        public string ProductAddress { get; set; }
        public string ShippingAddress { get; set; }

        public string ShippingValue { get; set; }
        public string Freight { get; set; }

        public string Price { get; set; }

        public bool IsSellerAdminProdcut { get; set; }
        public int CouponCount { get; set; }
        public bool IsExpiredShop { get; set; }
        /// <summary>
        /// 宝贝与描述
        /// </summary>
        public decimal ProductAndDescription { get; set; }
        /// <summary>
        /// 宝贝与描述
        /// </summary>
        public decimal ProductAndDescriptionPeer { get; set; }
        /// <summary>
        /// 宝贝与描述
        /// </summary>
        public decimal ProductAndDescriptionMin { get; set; }
        /// <summary>
        /// 宝贝与描述
        /// </summary>
        public decimal ProductAndDescriptionMax { get; set; }
        /// <summary>
        /// 卖家服务态度
        /// </summary>
        public decimal SellerServiceAttitude { get; set; }
        /// <summary>
        /// 卖家服务态度
        /// </summary>
        public decimal SellerServiceAttitudePeer { get; set; }
        /// <summary>
        /// 卖家服务态度
        /// </summary>
        public decimal SellerServiceAttitudeMax { get; set; }
        /// <summary>
        /// 卖家服务态度
        /// </summary>
        public decimal SellerServiceAttitudeMin { get; set; }
        /// <summary>
        /// 卖家发货速度
        /// </summary>
        public decimal SellerDeliverySpeed { get; set; }
        /// <summary>
        /// 卖家发货速度
        /// </summary>
        public decimal SellerDeliverySpeedPeer { get; set; }
        /// <summary>
        /// 卖家发货速度
        /// </summary>
        public decimal SellerDeliverySpeedMax { get; set; }
        /// <summary>
        /// 卖家发货速度
        /// </summary>
        public decimal sellerDeliverySpeedMin { get; set; }
        /// <summary>
        /// 商品二维码
        /// </summary>
        public string Code { get; set; }

        public CashDepositsObligation CashDepositsObligation { get; set; }

        /// <summary>
        /// 关注数
        /// </summary>
        public int Favorites { get; set; }

        public bool EnabledBuy { get; set; }

        public string VShopLog { get; set; }
        /// <summary>
        /// 推广商品数
        /// </summary>
        public long ShopDistributionProductNum { get; set; }
        /// <summary>
        /// 店铺关注数
        /// </summary>
        public long FavoriteShopCount { get; set; }
        /// <summary>
        /// 是否已代理
        /// </summary>
        public bool DistributionIsAgent { get; set; }
        /// <summary>
        /// 成交量
        /// </summary>
        public int DistributionSaleNum { get; set; }
        /// <summary>
        /// 代理量
        /// </summary>
        public int DistributionAgentNum { get; set; }
        /// <summary>
        /// 分销佣金
        /// </summary>
        public decimal DistributionCommission { get; set; }
    }
}