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
using Himall.API.Model;
using Himall.API.Helper;
using Himall.Application;
using Himall.Core;
using Himall.IServices.QueryModel;

namespace Himall.API
{
    /// <summary>
    /// 此模块暂时没用，已与Product合并
    /// </summary>
    public class LimitTimeBuyController : BaseApiController
    {
        public object GetLismitBuyList(int pageNo, int pageSize, string cateName = "")
        {
            #region 初始化查询Model
            FlashSaleQuery query = new FlashSaleQuery()
            {
                ItemName = cateName,
                IsPreheat = true,
                PageNo = pageNo,
                PageSize = pageSize,
                AuditStatus = FlashSaleInfo.FlashSaleStatus.Ongoing,
                CheckProductStatus = true,
                OrderKey = 5 /* 排序项（1：默认，2：销量，3：价格，4 : 结束时间,5:状态 开始排前面） */
            };

            #endregion
            var obj = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetAll(query);

            var list = obj.Models.ToList().Select(item => new
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductImg = Core.HimallIO.GetRomoteProductSizeImage(item.Himall_Products.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                ProductName = item.Himall_Products.ProductName,
                MinPrice = item.MinPrice,
                EndDate = item.EndDate
            }).ToList();

            return Json(new { Total = obj.Total, List = list });
        }


        public object GetLimitBuyProduct( long id )
        {
            ProductDetailModelForMobie model = new ProductDetailModelForMobie()
            {
                Product = new ProductInfoModel() ,
                Shop = new ShopInfoModel() ,
                Color = new CollectionSKU() ,
                Size = new CollectionSKU() ,
                Version = new CollectionSKU()
            };
            ProductInfo product = null;
            ShopInfo shop = null;
            FlashSaleModel market = null;

            market = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get( id );

            if( market == null || market.Status != FlashSaleInfo.FlashSaleStatus.Ongoing )
            {

                //可能参数是商品ID
                market = market == null ? ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetFlaseSaleByProductId( id ) : market;
                if( market == null || market.Status != FlashSaleInfo.FlashSaleStatus.Ongoing )
                {
                    //跳转到404页面
                    return Json( new { Success = "false" , ErrorMsg = "你所请求的限时购或者商品不存在！" } );
                }
            }

            if( market != null && ( market.Status != FlashSaleInfo.FlashSaleStatus.Ongoing || DateTime.Parse( market.EndDate ) < DateTime.Now ) )
            {
                return Json( new { Success = "true" , IsValidLimitBuy = "false" } );
            }

            model.MaxSaleCount = market.LimitCountOfThePeople;
            model.Title = market.Title;

            product = ServiceProvider.Instance<IProductService>.Create.GetProduct( market.ProductId );

            bool hasSku = false;
            #region 商品SKU
            ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

            if ( product.SKUInfo != null && product.SKUInfo.Count() > 0 )
            {
                hasSku = true;
                long colorId = 0 , sizeId = 0 , versionId = 0;
                foreach( var sku in product.SKUInfo )
                {

                    var specs = sku.Id.Split( '_' );
                    if( specs.Count() > 0 )
                    {
                        if( long.TryParse( specs[ 1 ] , out colorId ) ) { }
                        if( colorId != 0 )
                        {
                            if( !model.Color.Any( v => v.Value.Equals( sku.Color ) ) )
                            {
                                var c = product.SKUInfo.Where( s => s.Color.Equals( sku.Color ) ).Sum( s => s.Stock );
                                model.Color.Add(new ProductSKU
                                {
                                    //Name = "选择颜色" ,
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color,
                                    Img = sku.ShowPic
                                });
                            }
                        }
                    }
                    if( specs.Count() > 1 )
                    {
                        if( long.TryParse( specs[ 2 ] , out sizeId ) ) { }
                        if( sizeId != 0 )
                        {
                            if( !model.Size.Any( v => v.Value.Equals( sku.Size ) ) )
                            {
                                var ss = product.SKUInfo.Where( s => s.Size.Equals( sku.Size ) ).Sum( s1 => s1.Stock );
                                model.Size.Add(new ProductSKU
                                {
                                    //Name = "选择尺码" ,
                                    Name = "选择" + sizeAlias,
                                    EnabledClass = ss != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Size.Any(s1 => s1.SelectedClass.Equals("selected")) && ss != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = sizeId,
                                    Value = sku.Size
                                });
                            }
                        }
                    }

                    if( specs.Count() > 2 )
                    {
                        if( long.TryParse( specs[ 3 ] , out versionId ) ) { }
                        if( versionId != 0 )
                        {
                            if( !model.Version.Any( v => v.Value.Equals( sku.Version ) ) )
                            {
                                var v = product.SKUInfo.Where( s => s.Version.Equals( sku.Version ) ).Sum( s => s.Stock );
                                model.Version.Add(new ProductSKU
                                {
                                    //Name = "选择版本" ,
                                    Name = "选择" + versionAlias,
                                    EnabledClass = v != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Version.Any(v1 => v1.SelectedClass.Equals("selected")) && v != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = versionId,
                                    Value = sku.Version
                                });
                            }
                        }
                    }

                }

            }
            #endregion

            #region 店铺
            shop = ServiceProvider.Instance<IShopService>.Create.GetShop( product.ShopId );
            var mark = ShopServiceMark.GetShopComprehensiveMark( shop.Id );
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;
            var comm = ServiceProvider.Instance<ICommentService>.Create.GetCommentsByProductId( product.Id );
            model.Shop.Name = shop.ShopName;
            model.Shop.ProductMark = ( comm == null || comm.Count() == 0 ) ? 0 : comm.Average( p => ( decimal )p.ReviewMark );
            model.Shop.Id = product.ShopId;
            model.Shop.FreeFreight = shop.FreeFreight;
            model.Shop.ProductNum = ServiceProvider.Instance<IProductService>.Create.GetShopOnsaleProducts( product.ShopId );

            var shopStatisticOrderComments = ServiceProvider.Instance<IShopService>.Create.GetShopStatisticOrderComments( product.ShopId );

            var productAndDescription = shopStatisticOrderComments.Where( c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescription ).FirstOrDefault();
            var sellerServiceAttitude = shopStatisticOrderComments.Where( c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitude ).FirstOrDefault();
            var sellerDeliverySpeed = shopStatisticOrderComments.Where( c => c.CommentKey ==
                StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeed ).FirstOrDefault();

            var productAndDescriptionPeer = shopStatisticOrderComments.Where( c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionPeer ).FirstOrDefault();
            var sellerServiceAttitudePeer = shopStatisticOrderComments.Where( c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudePeer ).FirstOrDefault();
            var sellerDeliverySpeedPeer = shopStatisticOrderComments.Where( c => c.CommentKey ==
                StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedPeer ).FirstOrDefault();

            var productAndDescriptionMax = shopStatisticOrderComments.Where( c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMax ).FirstOrDefault();
            var productAndDescriptionMin = shopStatisticOrderComments.Where( c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMin ).FirstOrDefault();

            var sellerServiceAttitudeMax = shopStatisticOrderComments.Where( c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMax ).FirstOrDefault();
            var sellerServiceAttitudeMin = shopStatisticOrderComments.Where( c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMin ).FirstOrDefault();

            var sellerDeliverySpeedMax = shopStatisticOrderComments.Where( c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMax ).FirstOrDefault();
            var sellerDeliverySpeedMin = shopStatisticOrderComments.Where( c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMin ).FirstOrDefault();

            decimal defaultValue = 5;
            //宝贝与描述
            if( productAndDescription != null && productAndDescriptionPeer != null )
            {
                model.Shop.ProductAndDescription = productAndDescription.CommentValue;
            }
            else
            {
                model.Shop.ProductAndDescription = defaultValue;
            }
            //卖家服务态度
            if( sellerServiceAttitude != null && sellerServiceAttitudePeer != null )
            {
                model.Shop.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
            }
            else
            {
                model.Shop.SellerServiceAttitude = defaultValue;
            }
            //卖家发货速度
            if( sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null )
            {
                model.Shop.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
            }
            else
            {
                model.Shop.SellerDeliverySpeed = defaultValue;
            }
            if( ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId( shop.Id ) == null )
                model.Shop.VShopId = -1;
            else
                model.Shop.VShopId = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId( shop.Id ).Id;

            //优惠券
            var result = GetCouponList( shop.Id );//取设置的优惠券
            if( result != null )
            {
                var couponCount = result.Count();
                model.Shop.CouponCount = couponCount;
            }
            #endregion

            #region 商品
            var consultations = ServiceProvider.Instance<IConsultationService>.Create.GetConsultations( product.Id );
            double total = product.Himall_ProductComments.Count();
            double niceTotal = product.Himall_ProductComments.Count( item => item.ReviewMark >= 4 );
            bool isFavorite = false;
            if( CurrentUser == null )
                isFavorite = false;
            else
                isFavorite = ServiceProvider.Instance<IProductService>.Create.IsFavorite( product.Id , CurrentUser.Id );
            var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId( product.Id );
            var productImage = new List<string>();
            for( int i = 1 ; i < 6 ; i++ )
            {
                if( File.Exists( HttpContext.Current.Server.MapPath( product.RelativePath + string.Format("/{0}.png", i) ) ) )
                    productImage.Add(Core.HimallIO.GetRomoteImagePath(product.RelativePath + string.Format("/{0}.png", i)));
            }
            model.Product = new ProductInfoModel()
            {
                ProductId = product.Id ,
                CommentCount = product.Himall_ProductComments.Count() ,
                Consultations = consultations.Count() ,
                ImagePath = productImage ,
                IsFavorite = isFavorite ,
                MarketPrice = market.MinPrice ,
                MinSalePrice = product.MinSalePrice ,
                NicePercent = model.Shop.ProductMark == 0 ? 100 : ( int )( ( niceTotal / total ) * 100 ) ,
                ProductName = product.ProductName ,
                ProductSaleStatus = product.SaleStatus ,
                AuditStatus = product.AuditStatus ,
                ShortDescription = product.ShortDescription ,
                ProductDescription = product.ProductDescriptionInfo.ShowMobileDescription ,
                IsOnLimitBuy = limitBuy != null
            };
            #endregion

            LogProduct( market.ProductId );
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);

            TimeSpan end = new TimeSpan( DateTime.Parse( market.EndDate ).Ticks );
            TimeSpan start = new TimeSpan( DateTime.Now.Ticks );
            TimeSpan ts = end.Subtract( start );
            var second = ts.TotalSeconds < 0 ? 0 : ts.TotalSeconds;

            return Json(new
            {
                Success = "true",
                IsOnLimitBuy = "true",
                HasSku = hasSku,
                MaxSaleCount = market.LimitCountOfThePeople,
                Title = market.Title,
                Second = second,
                Product = model.Product,
                Shop = model.Shop,
                Color = model.Color,
                Size = model.Size,
                Version = model.Version,
                ColorAlias = colorAlias,
                SizeAlias = sizeAlias,
                VersionAlias = versionAlias
            });
        }
        internal void LogProduct( long pid )
        {
            if (CurrentUser != null)
            {
                BrowseHistrory.AddBrowsingProduct(pid, CurrentUser.Id);
            }
            else
            {
                BrowseHistrory.AddBrowsingProduct(pid);
            }
            //ServiceProvider.Instance<IProductService>.Create.LogProductVisti( pid );
        }

        internal IEnumerable<CouponInfo> GetCouponList( long shopid )
        {
            var service = ServiceProvider.Instance<ICouponService>.Create;
            var result = service.GetCouponList( shopid );
            var couponSetList = ServiceProvider.Instance<IVShopService>.Create.GetVShopCouponSetting( shopid ).Select( item => item.CouponID );
            if( result.Count() > 0 && couponSetList.Count() > 0 )
            {
                var couponList = result.ToArray().Where( item => couponSetList.Contains( item.Id ) );//取设置的优惠卷
                return couponList;
            }
            return null;
        }
    }
}
