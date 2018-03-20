using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Himall.CommonModel;
using Himall.Application;
using Himall.Core;
using Newtonsoft.Json;
using Himall.SmallProgAPI.Model;
using Himall.SmallProgAPI.Helper;
using System.Data;
using Newtonsoft.Json.Linq;
using Himall.DTO;
using System.Runtime.Serialization.Json;
using Himall.DTO.Product;

namespace Himall.SmallProgAPI
{
    public class ProductController : BaseApiController
    {
        /// <summary>
        /// 搜索商品
        /// </summary>
        /// <returns></returns>
        public object GetProducts(
             string keyword, /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            //long b_id = 0, /* 品牌ID */
            string openId = "",
            //string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
            string sortBy = "", /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
            string sortOrder = "", /* 排序方式（1：升序，2：降序） */
            int pageIndex = 1, /*页码*/
            int pageSize = 10,/*每页显示数据量*/
            long vshopId = 0,
            long sid = 0/*商家ID*/
            )
        {
            //CheckUserLogin();
            //if (string.IsNullOrEmpty(keyword) && vshopId == 0 && cid <= 0)
            //    keyword = Application.SiteSettingApplication.GetSiteSettings().Keyword;
            #region 初始化查询Model
            SearchProductQuery model = new SearchProductQuery();
            model.VShopId = vshopId;
            model.ShopId = sid;
            //model.BrandId = b_id;
            if (vshopId == 0 && cid != 0)
            {
                var catelist = ServiceProvider.Instance<ICategoryService>.Create.GetCategories();
                var cate = catelist.FirstOrDefault(r => r.Id == cid);
                if (cate.Depth == 1)
                    model.FirstCateId = cid;
                else if (cate.Depth == 2)
                    model.SecondCateId = cid;
                else if (cate.Depth == 3)
                    model.ThirdCateId = cid;
            }
            else if (vshopId != 0 && cid != 0)
            {
                model.ShopCategoryId = cid;
            }

            model.Keyword = keyword;
            if (sortBy == "SalePrice")
            {
                model.OrderKey = 3;//默认
            }
            else if (sortBy == "SaleCounts")
            {
                model.OrderKey = 2;
            }
            else if (sortBy == "VistiCounts")
            {
                model.OrderKey = 4;
            }
            else
            {
                model.OrderKey = 1;
            }

            if (sortOrder == "desc")
            {
                model.OrderType = true;//降序
            }
            else
            {
                model.OrderType = false;//升序
            }


            model.PageNumber = pageIndex;
            model.PageSize = pageSize;
            #endregion
            SearchProductResult result = ServiceProvider.Instance<ISearchProductService>.Create.SearchProduct(model);
            int total = result.Total;
            //当查询的结果少于一页时用like进行补偿（与PC端同步）
            if (result.Total < pageSize)
            {
                model.IsLikeSearch = true;
                SearchProductResult result2 = ServiceProvider.Instance<ISearchProductService>.Create.SearchProduct(model);
                var idList1 = result.Data.Select(a => a.ProductId).ToList();
                var nresult = result2.Data.Where(a => !idList1.Contains(a.ProductId)).ToList();

                if (nresult.Count > 0)
                {
                    result.Total += nresult.Count;
                    result.Data.AddRange(nresult);
                }
            }

            total = result.Total;
            #region 价格更新
            //会员折扣
            decimal discount = 1M;
            long SelfShopId = 0;
            long currentUserId = 0;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
                var shopInfo = ShopApplication.GetSelfShop();
                SelfShopId = shopInfo.Id;
                currentUserId = CurrentUser.Id;
            }
            //填充商品和购物车数据
            var ids = result.Data.Select(d => d.ProductId).ToArray();            
            List<Product> products= ProductManagerApplication.GetProductsByIds(ids);
            List<SKU> skus = ProductManagerApplication.GetSKU(ids);
            List<ShoppingCartItem> cartitems = CartApplication.GetCartQuantityByIds(currentUserId, ids);
            List<object> productList = new List<object>();
            foreach (var item in result.Data)
            {
                Product proInfo = products.Where(d => d.Id == item.ProductId).FirstOrDefault();
                SKU skuInfo = skus.Where(d => d.ProductId == item.ProductId).FirstOrDefault();
                bool hasSku = proInfo.HasSKU;
                decimal marketPrice = proInfo.MarketPrice;
                string skuId = skuInfo.Id;
                int quantity = 0;
                quantity = cartitems.Where(d => d.ProductId == item.ProductId).Sum(d => d.Quantity);
                decimal salePrice = item.SalePrice;
                item.ImagePath = Core.HimallIO.GetRomoteProductSizeImage(Core.HimallIO.GetImagePath(item.ImagePath), 1, (int)Himall.CommonModel.ImageSize.Size_350);
                if (item.ShopId == SelfShopId)
                    salePrice = item.SalePrice * discount;

                long activeId = 0;
                int activetype = 0;
                var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(item.ProductId);
                if (limitBuy != null)
                {
                    item.SalePrice = limitBuy.MinPrice;
                    activeId = limitBuy.Id;
                    activetype = 1;
                }
                //var activeInfo = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveByProId(item.ProductId);
                //if (activeInfo != null)
                //{
                //    item.SalePrice = activeInfo.MiniGroupPrice;
                //    activeId = activeInfo.Id;
                //    activetype = 2;
                //}
                var pro = new
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Pic = item.ImagePath,// GetProductImageFullPath(d.Field<string>("ThumbnailUrl310")),
                    MarketPrice = marketPrice.ToString("0.##"),//市场价
                    SalePrice = salePrice.ToString("0.##"),//当前价
                    SaleCounts = item.SaleCount,
                    CartQuantity = quantity,// item.cartquantity,
                    HasSKU = hasSku,// d.Field<bool>("HasSKU"),//是否有规格
                    SkuId = skuId,// d.Field<string>("SkuId"),//规格ID
                    ActiveId = activeId,//活动Id
                    ActiveType = activetype//活动类型（1代表限购，2代表团购，3代表商品预售，4代表限购预售，5代表团购预售）
                };
                productList.Add(pro);
            }
            #endregion
            var json = new
            {
                Status = "OK",
                Data = productList
            };
            return json;
        }
        /// <summary>
        /// 获取商品详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object GetProductDetail(string openId, long ProductID)
        {
            CheckUserLogin();
            ProductDetailModelForMobie model = new ProductDetailModelForMobie()
            {
                Product = new ProductInfoModel(),
                Shop = new ShopInfoModel(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };

            ProductInfo product = null;
            ShopInfo shop = null;
            long activeId = 0;
            int activetype = 0;

            product = ServiceProvider.Instance<IProductService>.Create.GetProduct(ProductID);

            var cashDepositModel = ServiceProvider.Instance<ICashDepositsService>.Create.GetCashDepositsObligation(product.Id);//提供服务（消费者保障、七天无理由、及时发货）
            model.CashDepositsServer = cashDepositModel;
            var com = product.Himall_ProductComments.Where(item => !item.IsHidden.HasValue || item.IsHidden.Value == false);
            #region 商品SKU


            var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(ProductID);
            ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

            if (limitBuy != null)
            {
                var limitSku = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(limitBuy.Id);
                var limitSkuItem = limitSku.Details.OrderBy(d => d.Price).FirstOrDefault();
                if (limitSkuItem != null)
                    product.MinSalePrice = limitSkuItem.Price;
            }
            List<object> SkuItemList = new List<object>();
            List<object> Skus = new List<object>();


            long[] Ids = { ProductID };
            List<ShoppingCartItem> cartitems = CartApplication.GetCartQuantityByIds(CurrentUser.Id, Ids);
            var stock = 0;
            if (product.SKUInfo != null && product.SKUInfo.Count() > 0)
            {
                #region 颜色
                long colorId = 0, sizeId = 0, versionId = 0;
                List<object> colorAttributeValue = new List<object>();
                List<string> listcolor = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0)
                    {
                        if (long.TryParse(specs[1], out colorId)) { }//相同颜色规格累加对应值
                        if (colorId != 0)
                        {
                            if (!listcolor.Contains(sku.Color))
                            {
                                var c = product.SKUInfo.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                var colorvalue = new
                                {
                                    ValueId = colorId,
                                    UseAttributeImage = "False",
                                    Value = sku.Color,
                                    ImageUrl = Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listcolor.Add(sku.Color);
                                colorAttributeValue.Add(colorvalue);
                            }
                        }
                    }
                }
                var color = new
                {
                    AttributeName = colorAlias,
                    AttributeId = product.TypeId,
                    AttributeValue = colorAttributeValue
                };
                if (colorId > 0)
                {
                    SkuItemList.Add(color);
                }
                #endregion

                #region 容量
                List<object> sizeAttributeValue = new List<object>();
                List<string> listsize = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 1)
                    {
                        if (long.TryParse(specs[2], out sizeId)) { }
                        if (sizeId != 0)
                        {
                            if (!listsize.Contains(sku.Size))
                            {
                                var ss = product.SKUInfo.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                var sizeValue = new
                                {
                                    ValueId = sizeId,
                                    UseAttributeImage = false,
                                    Value = sku.Size,
                                    ImageUrl =""// Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listsize.Add(sku.Size);
                                sizeAttributeValue.Add(sizeValue);
                            }
                        }
                    }
                }
                var size = new
                {
                    AttributeName = sizeAlias,
                    AttributeId = product.TypeId,
                    AttributeValue = sizeAttributeValue
                };
                if (sizeId > 0)
                {
                    SkuItemList.Add(size);
                }
                #endregion

                #region 规格
                List<object> versionAttributeValue = new List<object>();
                List<string> listversion = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0)
                        {
                            if (!listversion.Contains(sku.Version))
                            {
                                var v = product.SKUInfo.Where(s => s.Version.Equals(sku.Version));
                                var versionValue = new
                                {
                                    ValueId = versionId,
                                    UseAttributeImage = false,
                                    Value = sku.Version,
                                    ImageUrl = ""// Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listversion.Add(sku.Version);
                                versionAttributeValue.Add(versionValue);
                            }
                        }
                    }
                }
                var version = new
                {
                    AttributeName = versionAlias,
                    AttributeId = product.TypeId,
                    AttributeValue = versionAttributeValue
                };
                if (versionId > 0)
                {
                    SkuItemList.Add(version);
                }
                #endregion

                #region Sku值
                foreach (var sku in product.SKUInfo)
                {
                    int quantity = 0;
                    quantity = cartitems.Where(d => d.SkuId == sku.Id).Sum(d => d.Quantity);//购物车购买数量
                    var prosku = new
                    {
                        SkuItems = "",
                        MemberPrices = "",
                        SkuId = sku.Id,
                        ProductId = product.Id,
                        SKU = sku.Sku,
                        Weight = 0,
                        Stock = sku.Stock,
                        WarningStock = sku.SafeStock,
                        CostPrice = sku.CostPrice,
                        SalePrice = sku.SalePrice,
                        StoreStock = 0,
                        StoreSalePrice = 0,
                        OldSalePrice = 0,
                        ImageUrl = "",
                        ThumbnailUrl40 = "",
                        ThumbnailUrl410 = "",
                        MaxStock = 0,
                        FreezeStock = 0,
                        Quantity = quantity
                    };
                    Skus.Add(prosku);
                }
                #endregion
            }
            #endregion
            #region 店铺
            shop = ServiceProvider.Instance<IShopService>.Create.GetShop(product.ShopId);
            var mark = ShopServiceMark.GetShopComprehensiveMark(shop.Id);

            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;

            var comm = ServiceProvider.Instance<ICommentService>.Create.GetCommentsByProductId(ProductID);
            model.Shop.Name = shop.ShopName;
            model.Shop.ProductMark = (comm == null || comm.Count() == 0) ? 0 : comm.Average(p => (decimal)p.ReviewMark);
            model.Shop.Id = product.ShopId;
            model.Shop.FreeFreight = shop.FreeFreight;
            model.Shop.ProductNum = ServiceProvider.Instance<IProductService>.Create.GetShopOnsaleProducts(product.ShopId);
            var shopStatisticOrderComments = ServiceProvider.Instance<IShopService>.Create.GetShopStatisticOrderComments(product.ShopId);

            var productAndDescription = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescription).FirstOrDefault();
            var sellerServiceAttitude = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitude).FirstOrDefault();
            var sellerDeliverySpeed = shopStatisticOrderComments.Where(c => c.CommentKey ==
                StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeed).FirstOrDefault();

            var productAndDescriptionPeer = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionPeer).FirstOrDefault();
            var sellerServiceAttitudePeer = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudePeer).FirstOrDefault();
            var sellerDeliverySpeedPeer = shopStatisticOrderComments.Where(c => c.CommentKey ==
                StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedPeer).FirstOrDefault();

            //var productAndDescriptionMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMax).FirstOrDefault();
            //var productAndDescriptionMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMin).FirstOrDefault();
            //var sellerServiceAttitudeMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMax).FirstOrDefault();
            //var sellerServiceAttitudeMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMin).FirstOrDefault();
            //var sellerDeliverySpeedMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMax).FirstOrDefault();
            //var sellerDeliverySpeedMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMin).FirstOrDefault();

            decimal defaultValue = 5;
            //宝贝与描述
            if (productAndDescription != null && productAndDescriptionPeer != null)
            {
                model.Shop.ProductAndDescription = productAndDescription.CommentValue;
            }
            else
            {
                model.Shop.ProductAndDescription = defaultValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null)
            {
                model.Shop.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
            }
            else
            {
                model.Shop.SellerServiceAttitude = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null)
            {
                model.Shop.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
            }
            else
            {
                model.Shop.SellerDeliverySpeed = defaultValue;
            }
            if (ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id) == null)
                model.Shop.VShopId = -1;
            else
                model.Shop.VShopId = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id).Id;

            //优惠券
            //var couponCount = GetCouponList(shop.Id, CurrentUser.Id, 10000);//取设置的优惠券   
            //if (couponCount > 0)
            //{
            //    model.Shop.CouponCount = couponCount;
            //}

            // 客服
            //var customerServices = CustomerServiceApplication.GetMobileCustomerService(shop.Id);
            //var meiqia = CustomerServiceApplication.GetPreSaleByShopId(shop.Id).FirstOrDefault(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            //if (meiqia != null)
            //    customerServices.Insert(0, meiqia);


            #endregion
            #region 商品
            var consultations = ServiceProvider.Instance<IConsultationService>.Create.GetConsultations(ProductID);
            double total = product.Himall_ProductComments.Count();
            double niceTotal = product.Himall_ProductComments.Count(item => item.ReviewMark >= 4);
            bool isFavorite = false;
            bool IsFavoriteShop = false;
            decimal discount = 1M;          
            if (CurrentUser == null)
            {
                isFavorite = false;
                IsFavoriteShop = false;
            }
            else
            {
                isFavorite = ServiceProvider.Instance<IProductService>.Create.IsFavorite(product.Id, CurrentUser.Id);
                var favoriteShopIds = ServiceProvider.Instance<IShopService>.Create.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();//获取已关注店铺
                IsFavoriteShop = favoriteShopIds.Contains(product.ShopId);
                discount = CurrentUser.MemberDiscount;
            }

            decimal maxprice = shop.IsSelf ? product.SKUInfo.Max(d => d.SalePrice) * discount : product.SKUInfo.Max(d => d.SalePrice);//最高SKU价格
            decimal minprice = shop.IsSelf ? product.SKUInfo.Min(d => d.SalePrice) * discount : product.SKUInfo.Min(d => d.SalePrice);//最低价

            var productImage = new List<string>();
            for (int i = 1; i < 6; i++)
            {
                if (Core.HimallIO.ExistFile(product.RelativePath + string.Format("/{0}.png", i)))
                {
                    var path = Core.HimallIO.GetRomoteProductSizeImage(product.RelativePath, i, (int)Himall.CommonModel.ImageSize.Size_350);
                    productImage.Add(path);
                }
            }
            if (limitBuy != null)
            {
                maxprice = limitBuy.MinPrice;
                minprice = limitBuy.MinPrice;
                activeId = limitBuy.Id;
                activetype = 1;
            }
            model.Product = new ProductInfoModel()
            {
                ProductId = product.Id,
                CommentCount = com.Count(),//product.Himall_ProductComments.Count(),
                Consultations = consultations.Count(),
                ImagePath = productImage,
                IsFavorite = isFavorite,
                MarketPrice = product.MarketPrice,
                //MinSalePrice = minSalePrice,
                NicePercent = model.Shop.ProductMark == 0 ? 100 : (int)((niceTotal / total) * 100),
                ProductName = product.ProductName,
                ProductSaleStatus = product.SaleStatus,
                AuditStatus = product.AuditStatus,
                ShortDescription = product.ShortDescription,
                ProductDescription = GetProductDescription(product.ProductDescriptionInfo),
                IsOnLimitBuy = limitBuy != null,
            };


            #endregion
            LogProduct(ProductID);
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);

            //图片集合
            List<object> ProductImgs = new List<object>();
            for (int i = 1; i < 5; i++)
            {
                ProductImgs.Add(Core.HimallIO.GetRomoteProductSizeImage(product.ImagePath, i, (int)ImageSize.Size_350));
            }
            //优惠劵
            var coupons = GetShopCouponList(shop.Id);

            //参与活动
            List<object> Promotes = new List<object>();
            var reducelist = ServiceProvider.Instance<IFullDiscountService>.Create.GetOngoingActiveByProductId(product.Id, shop.Id);//满额减
            //满额减
            if (reducelist != null)
            {
                var FullAmountReduceList = new
                {
                    StoreId = 0,
                    PromoteType = 12,
                    ActivityId = product.Id,
                    ActivityName = reducelist.ActiveName,
                    StartDate = reducelist.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    GiftIds = ""
                };
                Promotes.Add(FullAmountReduceList);
            }
            //免运费
            if (model.Shop.FreeFreight > 0)
            {
                var FullAmountSentFreightList = new
                {
                    StoreId = 0,
                    PromoteType = 17,
                    ActivityId = product.Id,
                    ActivityName = "满" + model.Shop.FreeFreight + "免运费",
                    StartDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    GiftIds = ""
                };
                Promotes.Add(FullAmountSentFreightList);
            }           

            var json = new
            {
                Status = "OK",
                Data = new
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    ShortDescription = product.ShortDescription,
                    ShowSaleCounts = product.SaleCounts,
                    MetaDescription = model.Product.ProductDescription.Replace("\"/Storage/Shop", "\"" +Core.HimallIO.GetImagePath("/Storage/Shop")),//替换链接  /Storage/Shop
                    MarketPrice = product.MarketPrice.ToString("0.##"),//市场价
                    IsfreeShipping = "False",//是否免费送货
                    MaxSalePrice = maxprice.ToString("0.##"),
                    MinSalePrice = minprice.ToString("0.##"),//限时抢购或商城价格
                    ThumbnailUrl60 = Core.HimallIO.GetRomoteProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350),
                    ProductImgs = ProductImgs,
                    ReviewCount = total,
                    Stock = product.SKUInfo.Max(d=>d.Stock),
                    SkuItemList = SkuItemList,
                    Skus = Skus,
                    Freight = 0,//运费
                    Coupons = coupons,//优惠劵
                    Promotes = Promotes,//活动
                    IsUnSale = product.SaleStatus == Himall.Model.ProductInfo.ProductSaleStatus.InStock ? true : false,
                    ActiveId = activeId,
                    ActiveType = activetype
                }
            };
            return json;
        }
        /// <summary>
        /// 获取商品的规格信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object GetProductSkus(long productId, string openId = "")
        {
            CheckUserLogin();
            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(productId);
            var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(productId);

           
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            ShoppingCartInfo cartInfo = CartApplication.GetCart(CurrentUser.Id);

            var skuArray = new List<ProductSKUModel>();
            object defaultsku = new object();
            int activetype = 0;
            string skuId = "", skucode = "", imageUrl = "";
            long weight = 0, stock = 0;
            decimal SalePrice = 0;

            foreach (var sku in product.SKUInfo.Where(s => s.Stock > 0))
            {
                var price = sku.SalePrice * discount;
                ProductSKUModel skuMode = new ProductSKUModel
                {
                    Price = sku.SalePrice,
                    SkuId = sku.Id,
                    Stock = (int)sku.Stock
                };
                if (limitBuy != null)
                {
                    activetype = 1;
                    var limitSku = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(limitBuy.Id);
                    var limitSkuItem = limitSku.Details.Where(r => r.SkuId.Equals(sku.Id)).FirstOrDefault();
                    if (limitSkuItem != null)
                        skuMode.Price = limitSkuItem.Price;
                }
                skuArray.Add(skuMode);
            }

            ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

            List<object> SkuItemList = new List<object>();
            List<object> Skus = new List<object>();
            if (product.SKUInfo != null && product.SKUInfo.Count() > 0)
            {
                #region 颜色
                long colorId = 0, sizeId = 0, versionId = 0;
                List<object> colorAttributeValue = new List<object>();
                List<string> listcolor = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0)
                    {
                        if (long.TryParse(specs[1], out colorId)) { }//相同颜色规格累加对应值
                        if (colorId != 0)
                        {
                            if (!listcolor.Contains(sku.Color))
                            {
                                var c = product.SKUInfo.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                var colorvalue = new
                                {
                                    ValueId = colorId,
                                    Value = sku.Color,
                                    UseAttributeImage = "False",
                                    ImageUrl = Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listcolor.Add(sku.Color);
                                colorAttributeValue.Add(colorvalue);
                            }
                        }
                    }
                }
                var color = new
                {
                    AttributeId = product.TypeId,
                    AttributeName = colorAlias,
                    AttributeValue = colorAttributeValue
                };
                if (colorId > 0)
                {
                    SkuItemList.Add(color);
                }
                #endregion

                #region 容量
                List<object> sizeAttributeValue = new List<object>();
                List<string> listsize = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 1)
                    {
                        if (long.TryParse(specs[2], out sizeId)) { }
                        if (sizeId != 0)
                        {
                            if (!listsize.Contains(sku.Size))
                            {
                                var ss = product.SKUInfo.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                var sizeValue = new
                                {
                                    ValueId = sizeId,
                                    Value = sku.Size,
                                    UseAttributeImage = false,
                                    ImageUrl = Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listsize.Add(sku.Size);
                                sizeAttributeValue.Add(sizeValue);
                            }
                        }
                    }
                }
                var size = new
                {
                    AttributeId = product.TypeId,
                    AttributeName = sizeAlias,
                    AttributeValue = sizeAttributeValue
                };
                if (sizeId > 0)
                {
                    SkuItemList.Add(size);
                }
                #endregion

                #region 规格
                List<object> versionAttributeValue = new List<object>();
                List<string> listversion = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0)
                        {
                            if (!listversion.Contains(sku.Version))
                            {
                                var v = product.SKUInfo.Where(s => s.Version.Equals(sku.Version));
                                var versionValue = new
                                {
                                    ValueId = versionId,
                                    Value = sku.Version,
                                    UseAttributeImage = false,
                                    ImageUrl = Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listversion.Add(sku.Version);
                                versionAttributeValue.Add(versionValue);
                            }
                        }
                    }
                }
                var version = new
                {
                    AttributeId = product.TypeId,
                    AttributeName = versionAlias,
                    AttributeValue = versionAttributeValue
                };
                if (versionId > 0)
                {
                    SkuItemList.Add(version);
                }
                #endregion

                #region Sku值
                foreach (var sku in product.SKUInfo)
                {
                    var prosku = new
                    {
                        SkuId = sku.Id,
                        SKU = sku.Sku,
                        Weight = product.Weight,
                        Stock = sku.Stock,
                        WarningStock = sku.SafeStock,
                        SalePrice = sku.SalePrice.ToString("0.##"),
                        CartQuantity = cartInfo.Items.Where(d=>d.SkuId==sku.Id).Sum(d=>d.Quantity),
                        ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(sku.ShowPic, 1, (int)ImageSize.Size_350)
                    };
                    Skus.Add(prosku);
                }
                defaultsku = Skus[0];
                #endregion
            }
            var json = new
            {
                Status = "OK",
                Data = new
                {
                    ProductId = productId,
                    ProductName = product.ProductName,
                    ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350), //GetImageFullPath(model.SubmitOrderImg),
                    Stock = skuArray.Sum(s => s.Stock),// skus.Sum(s => s.Stock),
                    ActivityUrl = activetype,
                    SkuItems = SkuItemList,
                    Skus = Skus,
                    DefaultSku = defaultsku
                }
            };
            return json;
        }

        /// <summary>
        /// 商品评价数接口
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public object GetStatisticsReview(long productId)
        {
            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(productId);
            var comments = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(productId);
            var json = new
            {
                Status = "OK",
                Data = new
                {
                    productName = product.ProductName,
                    reviewNum = comments.Where(c => c.IsHidden.Value == false).Count(),
                    reviewNum1 = comments.Where(c => c.ReviewMark >= 4).Count(),
                    reviewNum2 = comments.Where(c => c.ReviewMark == 3).Count(),
                    reviewNum3 = comments.Where(c => c.ReviewMark <= 2).Count(),
                    reviewNumImg = comments.Where(c => c.Himall_ProductCommentsImages.Count > 0).Count()
                }
            };
            return json;
        }
        /// <summary>
        /// 商品评价列表
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public object GetLoadReview(long productId, int pageIndex, int pageSize, int type)
        {
            var json = new object();
            IEnumerable<ProductCommentInfo> result;
            var comments = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(productId);
            if (type != null)
            {
                switch (type)
                {
                    case 1:
                        result = comments.Where(c => c.ReviewMark >= 4).OrderByDescending(c => c.ReviewMark);
                        break;
                    case 2:
                        result = comments.Where(c => c.ReviewMark == 3).OrderByDescending(c => c.ReviewMark);
                        break;
                    case 3:
                        result = comments.Where(c => c.ReviewMark <= 2).OrderByDescending(c => c.ReviewMark);
                        break;
                    case 4:
                        result = comments.Where(c => c.Himall_ProductCommentsImages.Count > 0);
                        break;
                    default:
                        result = comments.OrderByDescending(c => c.ReviewMark);
                        break;
                }
                var temp = result.OrderByDescending(a => a.ReviewDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArray();
                var data = temp.Select(c =>
                {
                    string picture = Core.HimallIO.GetRomoteImagePath(c.Himall_Members.Photo);
                    ProductTypeInfo typeInfo = ServiceHelper.Create<ITypeService>().GetTypeByProductId(c.ProductId);
                    string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    List<string> Images = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 0).Select(a => a.CommentImage).ToList();//首评图片
                    //var AppendImages = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 1).Select(a => new { CommentImage = Core.HimallIO.GetImagePath(a.CommentImage) });//追加图片
                    string images1 = "", images2 = "", images3 = "", images4 = "", images5 = "";
                    for (int i = 0; i < Images.Count; i++)
                    {
                        if (i == 0)
                        {
                            images1 = Core.HimallIO.GetRomoteImagePath(MoveImages(Images[i]));
                        }
                        if (i == 1)
                        {
                            images2 = Core.HimallIO.GetRomoteImagePath(MoveImages(Images[i]));
                        }
                        if (i == 2)
                        {
                            images3 = Core.HimallIO.GetRomoteImagePath(MoveImages(Images[i]));
                        }
                        if (i == 3)
                        {
                            images4 = Core.HimallIO.GetRomoteImagePath(MoveImages(Images[i]));
                        }
                        if (i == 4)
                        {
                            images5 = Core.HimallIO.GetRomoteImagePath(MoveImages(Images[i]));
                        }
                    }

                    return new
                    {
                        UserName = c.UserName,
                        Picture = picture,
                        ProductId = c.ProductId,
                        ProductName = c.ProductInfo.ProductName,
                        ThumbnailUrl100 = Core.HimallIO.GetRomoteImagePath(c.ProductInfo.ImagePath),
                        ReviewText = c.ReviewContent,
                        AppendContent = c.AppendContent,
                        SKUContent = "",
                        AppendDate = c.AppendDate.HasValue ? c.AppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                        ReplyAppendContent = c.ReplyAppendContent,
                        ReplyAppendDate = c.ReplyAppendDate.HasValue ? c.ReplyAppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                        FinshDate = c.Himall_OrderItems.OrderInfo.FinishDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                        Images1 = images1,//首评图片
                        Images2 = images2,
                        Images3 = images3,
                        Images4 = images4,
                        Images5 = images5,
                        //AppendImages = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 1).Select(a => new { CommentImage = Core.HimallIO.GetImagePath(a.CommentImage) }),//追加图片
                        ReviewDate = c.ReviewDate.ToString("yyyy-MM-dd"),
                        ReplyText = string.IsNullOrWhiteSpace(c.ReplyContent) ? null : c.ReplyContent,
                        ReplyDate = c.ReplyDate.HasValue ? c.ReplyDate.Value.ToString("yyyy-MM-dd") : " ",
                        ReviewMark = c.ReviewMark,
                        BuyDate = c.Himall_OrderItems.OrderInfo.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        Color = c.Himall_OrderItems.Color == null ? "" : c.Himall_OrderItems.Color,
                        Version = c.Himall_OrderItems.Version == null ? "" : c.Himall_OrderItems.Version,
                        Size = c.Himall_OrderItems.Size == null ? "" : c.Himall_OrderItems.Size,
                        ColorAlias = colorAlias,
                        SizeAlias = sizeAlias,
                        VersionAlias = versionAlias
                    };
                }).ToArray();
                json = new
                {
                    Status = "OK",
                    totalCount = result.Count(),
                    Data = data.AsEnumerable().Select(d => new
                    {
                        UserName = d.UserName,
                        Picture = d.Picture,
                        ProductId = d.ProductId,
                        ThumbnailUrl100 = d.ThumbnailUrl100,
                        ProductName = d.ProductName,
                        SKUContent = d.SKUContent,
                        ReviewText = d.ReviewText,
                        Score = d.ReviewMark,
                        ImageUrl1 = d.Images1,
                        ImageUrl2 = d.Images2,
                        ImageUrl3 = d.Images3,
                        ImageUrl4 = d.Images4,
                        ImageUrl5 = d.Images5,
                        ReplyText = d.ReplyText,
                        ReviewDate = d.ReviewDate,
                        ReplyDate = d.ReplyDate
                    })
                };
            }
            return json;
        }
        /// <summary>
        /// 添加商品评论（评价送积分）
        /// </summary>
        public object GetAddProductReview(string openId, string DataJson)
        {
            CheckUserLogin();
            var json = new object();
            if (!string.IsNullOrEmpty(DataJson))
            {
                bool result = false;
                List<OrderCommentModel> orderComment = ParseFormJson<List<OrderCommentModel>>(DataJson);
                if (orderComment != null)
                {
                    List<ProductComment> list = new List<ProductComment>();
                    string orderIds = "";
                    foreach (var item in orderComment)
                    {
                        OrderCommentModel ordercom = new OrderCommentModel();
                        ordercom.ReviewDate = DateTime.Now;
                        ordercom.UserId = CurrentUser.Id;
                        ordercom.UserName = CurrentUser.UserName;
                        ordercom.UserEmail = CurrentUser.Email;
                        ordercom.OrderId = item.OrderId;
                        if (!orderIds.Contains(item.OrderId))
                        {
                            AddOrderComment(ordercom);//添加订单评价（订单评价只一次）
                            orderIds += item.OrderId + ",";
                        }

                        var model = new ProductComment();

                        var OrderInfo = ServiceHelper.Create<IOrderService>().GetOrderItemsByOrderId(long.Parse(item.OrderId)).Where(d => d.ProductId == item.ProductId).FirstOrDefault();
                        if (OrderInfo != null)
                        {
                            model.ReviewDate = DateTime.Now;
                            model.ReviewContent = item.ReviewText;
                            model.UserId = CurrentUser.Id;
                            model.UserName = CurrentUser.UserName;
                            model.Email = CurrentUser.Email;
                            model.SubOrderId = OrderInfo.Id;//订单明细Id
                            model.ReviewMark = item.Score;
                            model.ProductId = item.ProductId;
                            model.Images = new List<ProductCommentImage>();
                            foreach (var img in item.ImageUrl1.Split(','))
                            {
                                var p = new ProductCommentImage();

                                p.CommentType = 0;//0代表默认的表示评论的图片
                                p.CommentImage = Core.HimallIO.GetImagePath(img);
                                if (!string.IsNullOrEmpty(p.CommentImage))
                                {
                                    model.Images.Add(p);
                                }
                            }
                            list.Add(model);
                        }
                        result = true;
                    }
                    CommentApplication.Add(list);
                }
                if (result)
                {
                    json = new
                    {
                        Status = "OK",
                        Message = "评价成功"
                    };
                }
                else
                {
                    json = new
                    {
                        Status = "NO",
                        Message = "评价失败"
                    };
                }
            }
            return json;
        }
        /// <summary>
        /// 增加订单评论
        /// </summary>
        /// <param name="comment"></param>
        void AddOrderComment(OrderCommentModel comment)
        {
            TradeCommentApplication.Add(new OrderComment()
            {
                OrderId = long.Parse(comment.OrderId),
                DeliveryMark = 5,//物流评价
                ServiceMark = 5,//服务评价
                PackMark = 5,//包装评价
                UserId = comment.UserId,
                CommentDate = comment.ReviewDate,
                UserName = comment.UserName
            });
        }

        public static T ParseFormJson<T>(string szJson)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
            {
                DataContractJsonSerializer dcj = new DataContractJsonSerializer(typeof(T));
                return (T)dcj.ReadObject(ms);
            }
        }
        internal void LogProduct(long pid)
        {
            if (CurrentUser != null)
            {
                BrowseHistrory.AddBrowsingProduct(pid, CurrentUser.Id);
            }
            else
            {
                BrowseHistrory.AddBrowsingProduct(pid);
            }
        }

        internal IQueryable<CouponInfo> GetCouponList(long shopId)
        {
            var service = ServiceProvider.Instance<ICouponService>.Create;
            return service.GetCouponList(shopId);//商铺可用优惠券，排除过期和已领完的
        }

        public object GetShopCouponList(long shopId)
        {
            var coupons = GetCouponList(shopId);
            if (coupons != null)
            {
                VShopInfo vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shopId);
                if (vshop == null)
                {
                    return null;
                }
                var userCoupon = coupons.ToList().Select(a => new
                {
                    CouponId = a.Id,
                    CouponName = a.CouponName,
                    Price = a.Price,
                    SendCount = a.Num,
                    UserLimitCount = a.PerMax,
                    OrderUseLimit = a.OrderAmount,
                    StartTime = a.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ClosingTime = a.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    CanUseProducts = "",
                    ObtainWay = a.ReceiveType,
                    NeedPoint = a.NeedIntegral,
                    UseWithGroup = false,
                    UseWithPanicBuying = false,
                    UseWithFireGroup = false,
                    LimitText = a.CouponName,
                    CanUseProduct = "店铺通用",
                    StartTimeText = a.StartTime.ToString("yyyy.MM.dd"),
                    ClosingTimeText = a.EndTime.ToString("yyyy.MM.dd"),
                    EndTime=a.EndTime,
                    Receive = Receive(a.ShopId, a.Id)
                });
                var data = userCoupon.Where(p => p.Receive != 2 && p.Receive != 4).OrderByDescending(d => d.EndTime);//优惠券已经过期、优惠券已领完，则不显示在店铺优惠券列表中
                return data;
            }
            else
                return null;
        }

        private int Receive(long vshopId, long couponId)
        {
            var couponService = ServiceProvider.Instance<ICouponService>.Create;
            var couponInfo = couponService.GetCouponInfo(couponId);
            if (couponInfo.EndTime < DateTime.Now) return 2;//已经失效
            if (CurrentUser!=null)
            {
                CouponRecordQuery crQuery = new CouponRecordQuery();
                crQuery.CouponId = couponId;
                crQuery.UserId = CurrentUser.Id;
                ObsoletePageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
                if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax) return 3;//达到个人领取最大张数
                crQuery = new CouponRecordQuery()
                {
                    CouponId = couponId
                };
                pageModel = couponService.GetCouponRecordList(crQuery);
                if (pageModel.Total >= couponInfo.Num) return 4;//达到领取最大张数
                if (couponInfo.ReceiveType == Himall.Model.CouponInfo.CouponReceiveType.IntegralExchange)
                {
                    var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
                    if (userInte.AvailableIntegrals < couponInfo.NeedIntegral) return 5;//积分不足
                }
            }
            return 1;//可正常领取
        }

        /// <summary>
        /// 将商品关联版式组合商品描述
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private string GetProductDescription(ProductDescriptionInfo productDescription)
        {
            if (productDescription == null)
            {
                throw new Himall.Core.HimallException("错误的商品信息");
            }
            string descriptionPrefix = "", descriptiondSuffix = "";//顶部底部版式
            string description = productDescription.ShowMobileDescription.Replace("src=\"/Storage/", "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));//商品描述

            var iprodestempser = ServiceHelper.Create<IProductDescriptionTemplateService>();
            if (productDescription.DescriptionPrefixId != 0)
            {
                var desc = iprodestempser.GetTemplate(productDescription.DescriptionPrefixId, productDescription.ProductInfo.ShopId);
                descriptionPrefix = desc == null ? "" : desc.MobileContent.Replace("src=\"/Storage/", "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));
            }

            if (productDescription.DescriptiondSuffixId != 0)
            {
                var desc = iprodestempser.GetTemplate(productDescription.DescriptiondSuffixId, productDescription.ProductInfo.ShopId);
                descriptiondSuffix = desc == null ? "" : desc.MobileContent.Replace("src=\"/Storage/", "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));
            }

            return string.Format("{0}{1}{2}", descriptionPrefix, description, descriptiondSuffix);
        }

        private string MoveImages(string image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }
            var oldname = Path.GetFileName(image);
            string ImageDir = string.Empty;

            //转移图片
            string relativeDir = "/Storage/Plat/Comment/";
            string fileName = oldname;
            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                var de = image.Substring(image.LastIndexOf("/temp/"));
                Core.HimallIO.CopyFile(de, relativeDir + fileName, true);
                return relativeDir + fileName;
            }  //目标地址
            else if (image.Contains("/Storage"))
            {
                return image.Substring(image.LastIndexOf("/Storage"));
            }
            return image;
        }
    }
}
