using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Himall.Application;
using Himall.Core;
using Himall.IServices.QueryModel;
using Newtonsoft.Json;
using Himall.SmallProgAPI.Helper;
using Himall.SmallProgAPI.Model;
using Himall.CommonModel;

namespace Himall.SmallProgAPI
{

    public class LimitTimeBuyController : BaseApiController
    {
        /// <summary>
        /// 获取限时购列表接口
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public object GetLimitBuyList(int pageIndex, int pageSize)
        {
            #region 初始化查询Model
            FlashSaleQuery query = new FlashSaleQuery()
            {
                PageNo = pageIndex,
                PageSize = pageSize,
                CheckProductStatus = true,
                OrderKey = 5, /* 排序项（1：默认，2：销量，3：价格，4 : 结束时间,5:状态 开始排前面） */
                AuditStatus=FlashSaleInfo.FlashSaleStatus.Ongoing
            };

            #endregion
            var obj = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetAll(query);

            var list = obj.Models.ToList().Select(item => new
            {
                CountDownId = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Himall_Products.ProductName,
                SalePrice =  item.Himall_Products.MinSalePrice.ToString("0.##"),//市场价
                CountDownPrice =item.MinPrice,
                CountDownType = DateTime.Now < item.BeginDate ? 1 : 2,   //1=即将开始，2=立即抢购
                ThumbnailUrl160 = Core.HimallIO.GetRomoteProductSizeImage(item.Himall_Products.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_220)
            }).ToList();
            var json = new
            {
                Status = "OK",
                Data = list
            };
            return json;
        }

        ///// <summary>
        ///// 获取限时抢购商品详情
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        public object GetLimitBuyProduct(string openId,long countDownId)
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
            FlashSaleModel market = null;

            market = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(countDownId);
            

            if (market == null || market.Status != FlashSaleInfo.FlashSaleStatus.Ongoing)
            {
                //可能参数是商品ID
                market = market == null ? ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetFlaseSaleByProductId(countDownId) : market;
                if (market == null || market.Status != FlashSaleInfo.FlashSaleStatus.Ongoing)
                {
                    //跳转到404页面
                    return Json(new { Success = "false", ErrorMsg = "你所请求的限时购或者商品不存在！" });
                }
            }

            if (market != null && (market.Status != FlashSaleInfo.FlashSaleStatus.Ongoing || DateTime.Parse(market.EndDate) < DateTime.Now))
            {
                return Json(new { Success = "true", IsValidLimitBuy = "false" });
            }

            model.MaxSaleCount = market.LimitCountOfThePeople;
            model.Title = market.Title;

            product = ServiceProvider.Instance<IProductService>.Create.GetProduct(market.ProductId);

            bool hasSku = false;

            #region 商品SKU
            ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;


            List<object> SkuItemList = new List<object>();
            List<object> Skus = new List<object>();
            if (product.SKUInfo != null && product.SKUInfo.Count() > 0)
            {
                hasSku = true;
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
                    FlashSaleDetailInfo detailInfo = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetDetail(sku.Id);
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
                        SalePrice =sku.SalePrice,//限时抢购价格
                        StoreStock = 0,
                        StoreSalePrice = 0,
                        OldSalePrice = 0,
                        ImageUrl = "",
                        ThumbnailUrl40 = "",
                        ThumbnailUrl410 = "",
                        MaxStock = 15,
                        FreezeStock = 0,
                        ActivityStock = sku.Stock,//限时抢购库存
                        ActivityPrice = detailInfo.Price//限时抢购价格
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
            var comm = ServiceProvider.Instance<ICommentService>.Create.GetCommentsByProductId(product.Id);
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

            var productAndDescriptionMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMax).FirstOrDefault();
            var productAndDescriptionMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMin).FirstOrDefault();

            var sellerServiceAttitudeMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMax).FirstOrDefault();
            var sellerServiceAttitudeMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMin).FirstOrDefault();

            var sellerDeliverySpeedMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMax).FirstOrDefault();
            var sellerDeliverySpeedMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMin).FirstOrDefault();

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


            List<object> coupons = new List<object>();
            //优惠券
            var result = GetCouponList(shop.Id);//取设置的优惠券
            if (result != null)
            {
                var couponCount = result.Count();
                model.Shop.CouponCount = couponCount;                
                if (result.ToList().Count > 0)
                {
                    foreach (var item in result.ToList())
                    {
                        var couponInfo = new
                        {
                            CouponId = item.Id,
                            CouponName = item.CouponName,
                            Price = item.Price,
                            SendCount = item.Num,
                            UserLimitCount = item.PerMax,
                            OrderUseLimit = item.OrderAmount,
                            StartTime = item.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            ClosingTime = item.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            CanUseProducts = "",
                            ObtainWay = item.ReceiveType,
                            NeedPoint = item.NeedIntegral,
                            UseWithGroup = false,
                            UseWithPanicBuying = false,
                            UseWithFireGroup = false,
                            LimitText = item.CouponName,
                            CanUseProduct = "店铺通用",
                            StartTimeText = item.StartTime.ToString("yyyy.MM.dd"),
                            ClosingTimeText = item.EndTime.ToString("yyyy.MM.dd")
                        };
                        coupons.Add(couponInfo);
                    }
                }
            }


            #endregion

            #region 商品
            var consultations = ServiceProvider.Instance<IConsultationService>.Create.GetConsultations(product.Id);
            double total = product.Himall_ProductComments.Count();
            double niceTotal = product.Himall_ProductComments.Count(item => item.ReviewMark >= 4);
            bool isFavorite = false;
            if (CurrentUser == null)
                isFavorite = false;
            else
                isFavorite = ServiceProvider.Instance<IProductService>.Create.IsFavorite(product.Id, CurrentUser.Id);
            var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(product.Id);
            var productImage = new List<string>();
            for (int i = 1; i < 6; i++)
            {
                if (File.Exists(HttpContext.Current.Server.MapPath(product.RelativePath + string.Format("/{0}.png", i))))
                    productImage.Add(Core.HimallIO.GetRomoteImagePath(product.RelativePath + string.Format("/{0}.png", i)));
            }
            model.Product = new ProductInfoModel()
            {
                ProductId = product.Id,
                CommentCount = product.Himall_ProductComments.Count(),
                Consultations = consultations.Count(),
                ImagePath = productImage,
                IsFavorite = isFavorite,
                MarketPrice = product.MarketPrice,
                MinSalePrice = product.MinSalePrice,
                NicePercent = model.Shop.ProductMark == 0 ? 100 : (int)((niceTotal / total) * 100),
                ProductName = product.ProductName,
                ProductSaleStatus = product.SaleStatus,
                AuditStatus = product.AuditStatus,
                ShortDescription = product.ShortDescription,
                ProductDescription = product.ProductDescriptionInfo.ShowMobileDescription,
                IsOnLimitBuy = limitBuy != null
            };
            #endregion

            //LogProduct(market.ProductId);
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);

            TimeSpan end = new TimeSpan(DateTime.Parse(market.EndDate).Ticks);
            TimeSpan start = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan ts = end.Subtract(start);
            var second = ts.TotalSeconds < 0 ? 0 : ts.TotalSeconds;

            List<object> ProductImgs = new List<object>();
            for (int i = 1; i < 5; i++)
            {
                ProductImgs.Add(Core.HimallIO.GetRomoteProductSizeImage(product.ImagePath, i, (int)ImageSize.Size_350));
            }
            
            var countDownStatus=0;

            if (market.Status == FlashSaleInfo.FlashSaleStatus.Ended )
            {
                countDownStatus = 4;//"PullOff";  //已下架
            }
            else if (market.Status == FlashSaleInfo.FlashSaleStatus.Cancelled || market.Status == FlashSaleInfo.FlashSaleStatus.AuditFailed || market.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)
            {
                countDownStatus = 4;//"NoJoin";  //未参与
            }
            else if (DateTime.Parse(market.BeginDate) > DateTime.Now)
            {
                countDownStatus =6; // "AboutToBegin";  //即将开始   6
            }
            else if (DateTime.Parse(market.EndDate) < DateTime.Now)
            {
                countDownStatus = 4;// "ActivityEnd";   //已结束  4
            }
            else if (market.Status == FlashSaleInfo.FlashSaleStatus.Ended)
            {
                countDownStatus = 6;// "SoldOut";  //已抢完
            }
            else
            {
                countDownStatus = 2;//"Normal";  //正常  2
            }

            //Normal：正常
            //PullOff：已下架
            //NoJoin：未参与
            //AboutToBegin：即将开始
            //ActivityEnd：已结束
            //SoldOut：已抢完

            var json =new
            {
                Status = "OK",
                Data = new
                {
                    CountDownId = market.Id,//.CountDownId,
                    MaxCount = market.LimitCountOfThePeople,
                    CountDownStatus = countDownStatus,
                    StartDate = DateTime.Parse(market.BeginDate).ToString("yyyy/MM/dd HH:mm:ss"),
                    EndDate = DateTime.Parse(market.EndDate).ToString("yyyy/MM/dd HH:mm:ss"),
                    NowTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    MetaDescription =model.Product.ProductDescription,
                    ShortDescription = product.ShortDescription,
                    ShowSaleCounts = product.SaleCounts.ToString(),
                    Weight = product.Weight.ToString(),
                    MinSalePrice = market.MinPrice.ToString("0.##"),//product.SKUInfo.Min(c => c.SalePrice).ToString("0.##"),//限时抢购价格
                    MaxSalePrice = product.MarketPrice,
                    Stock = market.Quantity,//限时抢购库存
                    MarketPrice = product.MarketPrice,//product.SKUInfo.Min(c => c.SalePrice).ToString("0.##"),
                    IsfreeShipping =product.Himall_Shops.FreeFreight, //product.Product.IsfreeShipping.ToString(),
                    ThumbnailUrl60 = Core.HimallIO.GetRomoteProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350),
                    ProductImgs = ProductImgs,
                    SkuItemList = SkuItemList,
                    Skus =Skus,                    
                    Freight = 0,
                    Coupons = coupons,
                }
            };
            return json;
        }

        internal IEnumerable<CouponInfo> GetCouponList(long shopid)
        {
            var service = ServiceProvider.Instance<ICouponService>.Create;
            var result = service.GetCouponList(shopid);
            var couponSetList = ServiceProvider.Instance<IVShopService>.Create.GetVShopCouponSetting(shopid).Select(item => item.CouponID);
            if (result.Count() > 0 && couponSetList.Count() > 0)
            {
                var couponList = result.ToArray().Where(item => couponSetList.Contains(item.Id));//取设置的优惠卷
                return couponList;
            }
            return null;
        }
    }
}
