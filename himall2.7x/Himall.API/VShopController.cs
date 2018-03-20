using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Drawing;
using Himall.IServices.QueryModel;
using System.Web.Http;
using Himall.API.Model;
using Himall.API.Helper;
using Himall.API.Model.ParamsModel;
using Himall.Application;

namespace Himall.API
{
    public class VShopController : BaseApiController
    {
        public object GetVShops(int pageNo, int pageSize)
        {
            int total;
            var vshops = ServiceProvider.Instance<IVShopService>.Create.GetVShops(pageNo, pageSize, out total, VShopInfo.VshopStates.Normal).ToArray();
            long[] favoriteShopIds = new long[] { };
            if (CurrentUser != null)
                favoriteShopIds = ServiceProvider.Instance<IShopService>.Create.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();
            var model = vshops.Select(item => new
            {
                id = item.Id,
                //image = "http://" + Url.Request.RequestUri.Host + item.BackgroundImage,
                image = Core.HimallIO.GetRomoteImagePath(item.BackgroundImage),
                tags = item.Tags,
                name = item.Name,
                shopId = item.ShopId,
                favorite = favoriteShopIds.Contains(item.ShopId)
            });
            return Json(new { Success = "true", VShops = model, Total = total });
        }

        public object GetVShop(long id, bool sv = false)
        {
            var vshopService = ServiceProvider.Instance<IVShopService>.Create;
            var vshop = vshopService.GetVShop(id);

            //轮播图配置只有商家微店首页配置页面可配置，现在移动端都读的这个数据
            var slideImgs = ServiceProvider.Instance<ISlideAdsService>.Create.GetSlidAds(vshop.ShopId, SlideAdInfo.SlideAdType.VShopHome).ToList();

            //首页商品现在只有商家配置微信首页，APP读的也是这个数据所以平台类型选的的微信端
            var homeProducts = ServiceProvider.Instance<IMobileHomeProductsService>.Create.GetMobileHomePageProducts(vshop.ShopId, Himall.Core.PlatformType.WeiXin).OrderBy(item => item.Sequence).ThenByDescending(o => o.Id).Take(8);
            var products = homeProducts.ToArray().Select(item => new ProductItem()
            {
                Id = item.ProductId,
                //ImageUrl = "http://" + Url.Request.RequestUri.Host + item.Himall_Products.GetImage(Model.ProductInfo.ImageSize.Size_350),
                ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(item.Himall_Products.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                Name = item.Himall_Products.ProductName,
                MarketPrice = item.Himall_Products.MarketPrice,
                SalePrice = item.Himall_Products.MinSalePrice
            });
            var banner = ServiceProvider.Instance<INavigationService>.Create.GetSellerNavigations(vshop.ShopId, Core.PlatformType.WeiXin).ToList();

            var couponInfo = GetCouponList(vshop.ShopId);

            //var SlideAds = slideImgs.ToArray().Select(item => new HomeSlideAdsModel() { ImageUrl = "http://" + Url.Request.RequestUri.Host + item.ImageUrl, Url = item.Url });
            var SlideAds = slideImgs.ToArray().Select(item => new HomeSlideAdsModel() { ImageUrl = Core.HimallIO.GetRomoteImagePath(item.ImageUrl), Url = item.Url });

            var Banner = banner;
            var Products = products;

            bool favoriteShop = false;
            if (CurrentUser != null)
                favoriteShop = ServiceProvider.Instance<IShopService>.Create.IsFavoriteShop(CurrentUser.Id, vshop.ShopId);
            string followUrl = "";
            //快速关注
            var vshopSetting = ServiceProvider.Instance<IVShopService>.Create.GetVShopSetting(vshop.ShopId);
            if (vshopSetting != null)
                followUrl = vshopSetting.FollowUrl;
            var model = new
            {
                Id = vshop.Id,
                //Logo = "http://" + Url.Request.RequestUri.Host + vshop.Logo,
                Logo = Core.HimallIO.GetRomoteImagePath(vshop.Logo),
                Name = vshop.Name,
                ShopId = vshop.ShopId,
                Favorite = favoriteShop,
                State = vshop.State,
                FollowUrl = followUrl
            };

            // 客服
            var customerServices = CustomerServiceApplication.GetMobileCustomerService(vshop.ShopId);
            var meiqia = CustomerServiceApplication.GetPreSaleByShopId(vshop.ShopId).FirstOrDefault(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            if (meiqia != null)
                customerServices.Insert(0, meiqia);
          
            //统计访问量
            if (!sv)
            {
                vshopService.LogVisit(id);
                //统计店铺访问人数
                StatisticApplication.StatisticShopVisitUserCount(vshop.ShopId);
            }
            return Json(new { Success = "True", VShop = model, SlideImgs = SlideAds, Products = products, Banner = banner, Coupon = couponInfo, CustomerServices= customerServices });
        }
        public object GetVShopCategory(long id)
        {
            var vshopInfo = ServiceProvider.Instance<IVShopService>.Create.GetVShop(id);
            var bizCategories = ServiceProvider.Instance<IShopCategoryService>.Create.GetShopCategory(vshopInfo.ShopId).ToList();
            var shopCategories = GetSubCategories(bizCategories, 0, 0);
            long shopId = 0;
            if (vshopInfo != null) shopId = vshopInfo.ShopId;
            return Json(new { Success = "True", VShopId = id, ShopCategories = shopCategories, ShopId= shopId });
        }

        public object GetVShopIntroduce(long id)
        {
            var vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShop(id);
            string qrCodeImagePath = string.Empty;
            if (vshop != null)
            {
                Image qrcode;
                string vshopUrl = "http://" + Url.Request.RequestUri.Host + "/m-" + PlatformType.WeiXin.ToString() + "/vshop/detail/" + id;
                //string vshopUrl = Core.HimallIO.GetRomoteImagePath("/m-" + PlatformType.WeiXin.ToString() + "/vshop/detail/") + id;
                //string logoFullPath = HttpContext.Current.Server.MapPath(vshop.Logo);
                string logoFullPath = Core.HimallIO.GetRomoteImagePath(vshop.Logo);
                if (string.IsNullOrWhiteSpace(vshop.Logo) || !Core.HimallIO.ExistFile(logoFullPath))
                    qrcode = Core.Helper.QRCodeHelper.Create(vshopUrl);
                else
                    qrcode = Core.Helper.QRCodeHelper.Create(vshopUrl, logoFullPath);

                string fileName = DateTime.Now.ToString("yyMMddHHmmssffffff") + ".jpg";
                qrCodeImagePath = "http://" + Url.Request.RequestUri.Host + "/temp/" + fileName;
                qrcode.Save(HttpContext.Current.Server.MapPath("~/temp/") + fileName);
            }
            var qrCode = qrCodeImagePath;
            bool favorite = false;

            if (CurrentUser != null)
                favorite = ServiceProvider.Instance<IShopService>.Create.IsFavoriteShop(CurrentUser.Id, vshop.ShopId);

            var mark = ShopServiceMark.GetShopComprehensiveMark(vshop.ShopId);
            var shopMark = mark.ComprehensiveMark.ToString();

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = ServiceProvider.Instance<IShopService>.Create.GetShopStatisticOrderComments(vshop.ShopId);

            var productAndDescription = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescription).FirstOrDefault();
            var sellerServiceAttitude = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitude).FirstOrDefault();
            var sellerDeliverySpeed = shopStatisticOrderComments.Where(c => c.CommentKey ==
                StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeed).FirstOrDefault();

            var productAndDescriptionPeer = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionPeer).FirstOrDefault();
            var sellerServiceAttitudePeer = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudePeer).FirstOrDefault();
            var sellerDeliverySpeedPeer = shopStatisticOrderComments.Where(c => c.CommentKey ==
                StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedPeer).FirstOrDefault();

            decimal defaultValue = 5;
            var _productAndDescription = defaultValue;
            var _sellerServiceAttitude = defaultValue;
            var _sellerDeliverySpeed = defaultValue;
            //宝贝与描述
            if (productAndDescription != null && productAndDescriptionPeer != null)
            {
                _productAndDescription = productAndDescription.CommentValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null)
            {
                _sellerServiceAttitude = sellerServiceAttitude.CommentValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null)
            {
                _sellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
            }
            #endregion
            var vshopModel = new
            {
                QRCode = qrCode,
                Name = vshop.Name,
                IsFavorite = favorite,
                ProductAndDescription = _productAndDescription,
                SellerDeliverySpeed = _sellerDeliverySpeed,
                SellerServiceAttitude = _sellerServiceAttitude,
                Description = vshop.Description,
                ShopId = vshop.ShopId,
                Id = vshop.Id,
                //Logo = "http://" + Url.Request.RequestUri.Host+vshop.Logo
                Logo = Core.HimallIO.GetRomoteImagePath(vshop.Logo)
            };
            return Json(new { Success = "True", VShop = vshopModel });
        }
        //新增或删除店铺收藏
        public object PostAddFavoriteShop(VShopAddFavoriteShopModel value)
        {
            CheckUserLogin();
            long shopId = value.shopId;
            var favoriteTShopIds = ServiceProvider.Instance<IShopService>.Create.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();
            if (favoriteTShopIds.Contains(shopId))
            {
                ServiceProvider.Instance<IShopService>.Create.CancelConcernShops(shopId, CurrentUser.Id);
                return Json(new { Success = "true", Msg = "取消成功" });
            }
            else
            {
                ServiceProvider.Instance<IShopService>.Create.AddFavoriteShop(CurrentUser.Id, shopId);
                return Json(new { Success = "true", Msg = "关注成功" });
            }
        }

        public object GetVShopSearchProducts(long vshopId,
        string keywords = "", /* 搜索关键字 */
        string exp_keywords = "", /* 渐进搜索关键字 */
        long cid = 0,  /* 分类ID */
        long b_id = 0, /* 品牌ID */
        string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
        int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
        int orderType = 1, /* 排序方式（1：升序，2：降序） */
        int pageNo = 1, /*页码*/
        int pageSize = 10 /*每页显示数据量*/
        )
        {
            int total;
            long shopId = -1;
            var vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShop(vshopId);
            if (vshop != null)
                shopId = vshop.ShopId;
            
            if (!string.IsNullOrWhiteSpace(keywords))
                keywords = keywords.Trim();

            ProductSearch model = new ProductSearch()
            {
                shopId = shopId,
                BrandId = b_id,
                Ex_Keyword = exp_keywords,
                Keyword = keywords,
                OrderKey = orderKey,
                OrderType = orderType == 1,
                AttrIds = new System.Collections.Generic.List<string>(),
                PageNumber = pageNo,
                PageSize = pageSize,
                ShopCategoryId = cid
            };

            var productsResult = ServiceProvider.Instance<IProductService>.Create.SearchProduct(model);
            total = productsResult.Total;
            var products = productsResult.Models.ToArray();

            var commentService = ServiceProvider.Instance<ICommentService>.Create;
            var productsModel = products.Select(item =>
                new ProductItem()
                {
                    Id = item.Id,
                    ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                    SalePrice = item.MinSalePrice,
                    Name = item.ProductName,
                    CommentsCount = commentService.GetCommentsByProductId(item.Id).Count()
                }
            );
            var bizCategories = ServiceProvider.Instance<IShopCategoryService>.Create.GetShopCategory(shopId);
            var shopCategories = GetSubCategories(bizCategories, 0, 0);
            //统计店铺访问人数
            StatisticApplication.StatisticShopVisitUserCount(vshop.ShopId);
            return Json(new { Success = "true", Total = total,ShopCategory=shopCategories, Products = productsModel, VShopId = vshopId, Keywords =keywords});
        }

        

        private object GetCouponList(long shopid)
        {
            var service = ServiceProvider.Instance<ICouponService>.Create;
            var result = service.GetCouponList(shopid);
            var couponSetList = ServiceProvider.Instance<IVShopService>.Create.GetVShopCouponSetting(shopid).Where(d=>d.PlatForm== PlatformType.Wap).Select(item => item.CouponID);
            if (result.Count() > 0 && couponSetList.Count() > 0)
            {
                var couponList = result.ToArray().Where(item => couponSetList.Contains(item.Id)).Select(item => new
                    {
                        Id = item.Id,
                        Price = item.Price.ToString("F2"),
                        OrderAmount = item.OrderAmount.ToString("F2")
                    });//取设置的优惠券
                return couponList;
            }
            return null;
        }

        IEnumerable<CategoryModel> GetSubCategories(IEnumerable<ShopCategoryInfo> allCategoies, long categoryId, int depth)
        {
            var categories = allCategoies
                .Where(item => item.ParentCategoryId == categoryId)
                .Select(item =>
                {
                    string image = string.Empty;
                    return new CategoryModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        SubCategories = GetSubCategories(allCategoies, item.Id, depth + 1),
                        Depth = 1
                    };
                }).OrderBy(item=>item.Id);
            return categories;
        }
    }
}
