using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Himall.API.Model.ParamsModel;
using Himall.Application;

namespace Himall.API
{
    public class VShopHomeController : BaseApiController
    {
        public object GetVShopHome(int pageNo, int pageSize)
        {
            dynamic d = new System.Dynamic.ExpandoObject();
            TopShopModel topVShop = new TopShopModel();
            var service = ServiceProvider.Instance<IVShopService>.Create;
            var topShop = service.GetTopShop();
            if (topShop != null)
            {
                var products = ServiceProvider.Instance<IMobileHomeProductsService>.Create.GetMobileHomePageProducts(topShop.ShopId, Core.PlatformType.WeiXin).OrderBy(t => t.Sequence).ThenByDescending(o => o.Id).Take(2).Select(item => item.Himall_Products);
                var topShopProducts = products.ToArray().Select(item => new HomeProduct()
                {
                    Id = item.Id.ToString(),
                    //ImageUrl = "http://" + Url.Request.RequestUri.Host + item.GetImage(Model.ProductInfo.ImageSize.Size_350),
                    ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                    MarketPrice = item.MarketPrice.ToString(),
                    Name = item.ProductName,
                    SalePrice = item.MinSalePrice.ToString(),
                    //Url = "http://" + Url.Request.RequestUri.Host + "/m-IOS/product/detail/" + item.Id
                    Url = Core.HimallIO.GetRomoteImagePath("/m-IOS/product/detail/") + item.Id
                });
                topVShop.Success = "true";
                topVShop.ShopName = topShop.Name;
                topVShop.VShopId = topShop.Id.ToString();
                topVShop.ShopId = topShop.ShopId.ToString();
                //topVShop.ShopLogo = "http://" + Url.Request.RequestUri.Host + topShop.BackgroundImage;
                topVShop.ShopLogo = Core.HimallIO.GetRomoteImagePath(topShop.BackgroundImage);
                if(!string.IsNullOrEmpty(topShop.Tags))
                {
                    if (topShop.Tags.Contains(";"))
                    {
                        topVShop.Tag1 = topShop.Tags.Split(';')[0];
                        topVShop.Tag2 = topShop.Tags.Split(';')[1];
                    }
                    else
                    {
                        topVShop.Tag1 = topShop.Tags;
                        topVShop.Tag2 = "";
                    }
                }
                

                topVShop.Products = topShopProducts;//主推店铺的商品
                //topVShop.Url = "http://" + Url.Request.RequestUri.Host + "/m-IOS/vshop/detail/" + topShop.Id;
                topVShop.Url = Core.HimallIO.GetRomoteImagePath("/m-IOS/vshop/detail/") + topShop.Id;
                if (CurrentUser != null)
                {
                    var favoriteTShopIds = ServiceProvider.Instance<IShopService>.Create.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();//获取已关注店铺
                    topVShop.IsFavorite = favoriteTShopIds.Contains(topShop.ShopId)?true:false;
                }                
            }

                
            int total=0;
            var hotShops = ServiceProvider.Instance<IVShopService>.Create.GetHotShops(pageNo, pageSize, out total).ToArray();//获取热门微店
            var homeProductService = ServiceProvider.Instance<IMobileHomeProductsService>.Create;
            long[] favoriteShopIds = new long[] { };
            if (CurrentUser != null)
                favoriteShopIds = ServiceProvider.Instance<IShopService>.Create.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();
            var model = hotShops.Select(item =>
            {
                var products = homeProductService.GetMobileHomePageProducts(item.ShopId, Core.PlatformType.WeiXin).OrderBy(t => t.Sequence).ThenByDescending(o => o.Id).Take(2).Select(t => t.Himall_Products).ToArray();
                string tempTag1 = "";
                string tempTag2 = "";
                if(!string.IsNullOrEmpty(item.Tags))
                {
                    if (item.Tags.Contains(";"))
                    {
                        tempTag1 = item.Tags.Split(';')[0];
                        tempTag2 = item.Tags.Split(';')[1];
                    }
                    else
                        tempTag1 = item.Tags;
                }
                return new
                {
                    VShopId = item.Id.ToString(),
                    ShopName = item.Name,
                    //ShopLogo = "http://" + Url.Request.RequestUri.Host + item.BackgroundImage,
                    ShopLogo = Core.HimallIO.GetRomoteImagePath(item.BackgroundImage),
                    Tag1 = tempTag1,
                    Tag2 = tempTag2,
                    Products = products.Select(t => new
                    {
                        Id = t.Id.ToString(),
                        Name = t.ProductName,
                        //ImageUrl = "http://" + Url.Request.RequestUri.Host + t.GetImage(Model.ProductInfo.ImageSize.Size_220),
                        ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(t.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_220),
                        SalePrice = t.MinSalePrice,
                        //Url = "http://" + Url.Request.RequestUri.Host + "/m-IOS/product/detail/" + t.Id
                        Url = Core.HimallIO.GetRomoteImagePath("/m-IOS/product/detail/") + t.Id
                    }),
                    IsFavorite = favoriteShopIds.Contains(item.ShopId) ? true : false,
                    ShopId = item.ShopId.ToString(),
                    //Url = "http://" + Url.Request.RequestUri.Host + "/m-IOS/vshop/detail/" + item.Id
                    Url = Core.HimallIO.GetRomoteImagePath("/m-IOS/vshop/detail/") + item.Id
                };
            });
            d.Success = "true";
            d.HotShop = model;
            d.TopVShop = topVShop;
            d.Total = total;
            return d;
        }
        
    }
}
