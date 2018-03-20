using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;

namespace Himall.Web.Areas.Web.Models
{
    public class ProductDetailModelForWeb
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

		public bool CanSelfTake { get; set; }
        public long FreightTemplateId { get; set; }
	}

    public class LimitTimeProductDetailModel:ProductDetailModelForWeb
    {
        public long MainId { get; set; }
        public string Price { get; set; }
        public List<TypeAttributesModel> ProductAttrs { get; set; }
        public int CommentCount { get; set; }

        public int Consultations { get; set; }

        public int Logined { get; set; }
        public bool EnabledBuy { get; set; }
        public Double Second { get; set; }

        public decimal GrantPrice { get; set; }

        public Model.CashDepositsObligation CashDeposits { get; set; }

        public bool IsFavorite { get; set; }

        public IOrderedQueryable<CustomerServiceInfo> Service { get; set; }

    }

    public class ProductDetailModelForMobie
    {
        public ProductInfoModel Product { get; set; }
        public ShopInfoModel Shop { get; set; }
        public CollectionSKU Color { get; set; }
        public CollectionSKU Size { get; set; }
        public CollectionSKU Version { get; set; }
        public int MaxSaleCount { get; set; }
        public string Title { get; set; }
        public CashDepositsObligation CashDepositsServer { get; set; }
        public string ProductAddress { get; set; }
        public FreightTemplateInfo FreightTemplate { get; set; }
        public string VShopLog { get; set; }

    }

    public class ProductInfoModel
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public long ProductId { get; set; }
        /// <summary>
        /// 商品状态
        /// </summary>
        public ProductInfo.ProductSaleStatus ProductSaleStatus { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public ProductInfo.ProductAuditStatus AuditStatus { get; set; }
        /// <summary>
        /// 商品图片地址
        /// </summary>
        public List<string> ImagePath { get; set; }
        /// <summary>
        /// 商品名
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品市场价
        /// </summary>
        public decimal MarketPrice { get; set; }
        /// <summary>
        /// 商品的简单描述
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        public string ProductDescription { get; set; }
        /// <summary>
        /// 商品最低价
        /// </summary>
        public decimal MinSalePrice { get; set; }
        /// <summary>
        /// 是否收藏
        /// </summary>
        public bool IsFavorite { get; set; }
        /// <summary>
        /// 咨询数
        /// </summary>
        public int Consultations { get; set; }
        /// <summary>
        /// 商品评论数
        /// </summary>
        public int CommentCount { get; set; }
        /// <summary>
        /// 好评率
        /// </summary>
        public int NicePercent { get; set; }

        /// <summary>
        /// 是否真正参与限时购
        /// </summary>
        public bool IsOnLimitBuy { get; set; }
    }
    public class CollectionSKU : List<ProductSKU>
    {
        public override string ToString()
        {
            string str = "";
            foreach (var item in this)
            {
                str +=item.Value+",";
            }
            return str.TrimEnd(',');
        }
    }

    public class ProductSKU
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public long SkuId { get; set; }
        public string EnabledClass { get; set; }
        public string SelectedClass { get; set; }
        public string Img { get; set; }
    }

    public class HotProductInfo
    {
        public string ImgPath { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int SaleCount { get; set; }
        public long Id { get; set; }

    }

    public class ShopInfoModel
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public decimal ProductMark { get; set; }
        public decimal PackMark { get; set; }
        public decimal ServiceMark { get; set; }
        public decimal ComprehensiveMark { get; set ; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        public decimal FreeFreight { get; set; }

        /// <summary>
        /// 店铺在售商品数
        /// </summary>
        public long ProductNum { get; set; }

        /// <summary>
        /// 店铺优惠券数
        /// </summary>
        public long CouponCount { get; set; }

        /// <summary>
        /// 微店Id
        /// </summary>
        public long VShopId { get; set; }

        /// <summary>
        /// 宝贝描述得分
        /// </summary>
        public decimal ProductAndDescription { get; set; }
        /// <summary>
        /// 商家服务得分
        /// </summary>
        public decimal SellerServiceAttitude { get; set; }
        /// <summary>
        /// 发货物流得分
        /// </summary>
        public decimal SellerDeliverySpeed { get; set; }

        public long FavoriteShopCount { get; set; }
    }

}