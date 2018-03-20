using Himall.Application;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Himall.API
{
    public class HomeController : BaseApiController
    {
        //APP首页配置共用于安卓和IOS，这里的平台类型写的为IOS，安卓调用首页接口数据时平台类型也选IOS
        public APPHome Get(int pageNo, int pageSize)
        {
            var slideImageSettings = ServiceProvider.Instance<ISlideAdsService>.Create.GetSlidAds(0, SlideAdInfo.SlideAdType.IOSShopHome);
            //var slides = slideImageSettings.ToArray().Select(item => new HomeSlides { ImageUrl = "http://" + Url.Request.RequestUri.Host + item.ImageUrl, Url = item.Url });
            var slides = slideImageSettings.ToArray().Select(item => new HomeSlides { ImageUrl = Core.HimallIO.GetRomoteImagePath(item.ImageUrl), Url = item.Url });


            var images = ServiceProvider.Instance<ISlideAdsService>.Create.GetImageAds(0, Himall.CommonModel.ImageAdsType.APPSpecial).ToList();
            //var images = ServiceProvider.Instance<ISlideAdsService>.Create.GetImageAds(0).Take(5).ToList();
            var homeImage = images.Select(item => new HomeImage
                {
                    //ImageUrl = "http://" + Url.Request.RequestUri.Host + item.ImageUrl,
                    ImageUrl = Core.HimallIO.GetRomoteImagePath(item.ImageUrl),
                    Url = item.Url
                });
            var totalProducts = ServiceProvider.Instance<IMobileHomeProductsService>.Create.GetMobileHomePageProducts(0, Core.PlatformType.IOS).Count().ToString();
            var homeProducts = ServiceProvider.Instance<IMobileHomeProductsService>.Create.GetMobileHomePageProducts(0, Core.PlatformType.IOS).OrderBy(item => item.Sequence).ThenByDescending(o => o.Id).Skip((pageNo - 1) * pageSize).Take(pageSize);
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var products = new List<HomeProduct>();
            if (homeProducts != null)
            {
                var limitService=ServiceProvider.Instance<ILimitTimeBuyService>.Create;
                foreach (var item in homeProducts.ToArray())
                {
                    var limitBuy = limitService.GetLimitTimeMarketItemByProductId(item.ProductId);
                    decimal minSalePrice = item.Himall_Products.Himall_Shops.IsSelf ? item.Himall_Products.MinSalePrice * discount : item.Himall_Products.MinSalePrice;
                    var isValidLimitBuy = "false";
                    if (limitBuy != null)
                    {
                        minSalePrice = limitBuy.MinPrice; //限时购不打折
                    }
                    products.Add(new HomeProduct()
                    {
                        Id = item.ProductId.ToString(),
                        //    //ImageUrl = "http://" + Url.Request.RequestUri.Host + item.Himall_Products.GetImage(ProductInfo.ImageSize.Size_220),
                        ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(item.Himall_Products.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_220),
                        Name = item.Himall_Products.ProductName,
                        MarketPrice = item.Himall_Products.MarketPrice.ToString(),
                        SalePrice = minSalePrice.ToString("f2"),
                        Discount = (minSalePrice / item.Himall_Products.MarketPrice).ToString("0.0"),
                        //Url = "http://" + Url.Request.RequestUri.Host + "/m-ios/product/detail/" + item.ProductId
                        Url = Core.HimallIO.GetRomoteImagePath("/m-ios/product/detail/" + item.ProductId)
                    });
                }
            }
            //var products = homeProducts.ToArray().Select(item => new HomeProduct
            //{ //CommentsCount=item.Himall_Products.Himall_Shops.IsSelf
            //    Id = item.ProductId.ToString(),
            //    //ImageUrl = "http://" + Url.Request.RequestUri.Host + item.Himall_Products.GetImage(ProductInfo.ImageSize.Size_220),
            //    ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(item.Himall_Products.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_220),
            //    Name = item.Himall_Products.ProductName,
            //    MarketPrice = item.Himall_Products.MarketPrice.ToString(),
            //    //  SalePrice = item.Himall_Products.MinSalePrice.ToString(),
            //    SalePrice = item.Himall_Products.Himall_Shops.IsSelf ? item.Himall_Products.MinSalePrice * discount.ToString() : item.Himall_Products.MinSalePrice.ToString(),

            //    Discount = (item.Himall_Products.MinSalePrice / item.Himall_Products.MarketPrice).ToString("0.0"),
            //    //Url = "http://" + Url.Request.RequestUri.Host + "/m-ios/product/detail/" + item.ProductId
            //    Url = Core.HimallIO.GetRomoteImagePath("/m-ios/product/detail/" + item.ProductId)
            //});

            var iconSettings = ServiceProvider.Instance<ISlideAdsService>.Create.GetSlidAds(0, SlideAdInfo.SlideAdType.APPIcon);
            var icon = iconSettings.ToArray().Select(item => new HomeSlides { Desc = item.Description, ImageUrl = Core.HimallIO.GetRomoteImagePath(item.ImageUrl), Url = item.Url });

            var services = CustomerServiceApplication.GetPlatformCustomerService(true, true);
            var meiqia = CustomerServiceApplication.GetPlatformCustomerService(true, false).FirstOrDefault(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            if (meiqia != null)
                services.Insert(0, meiqia);

            APPHome appHome = new APPHome();
            appHome.Success = "true";
            appHome.TotalProduct = totalProducts;
            appHome.Icon = icon;
            appHome.Slide = slides;
            appHome.Topic = homeImage;
            appHome.Product = products;
            appHome.CustomerServices = services;
            return appHome;
        }

        public object GetUpdateApp(string appVersion, int type)
        {
            var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();

            if (string.IsNullOrWhiteSpace(appVersion) || (3 < type && type < 2))
            {
                return Json(new { Success = "false", Code = 10006, Msg = "版本号不能为空或者平台类型错误" });
            }
            Version ver = null;
            try
            {
                ver = new Version(appVersion);
            }
            catch (Exception)
            {
                return Json(new { Success = "false", Code = 10005, Msg = "错误的版本号" });
            }
            if (string.IsNullOrWhiteSpace(siteSetting.AppVersion))
            {
                siteSetting.AppVersion = "0.0.0";
            }
            var downLoadUrl = "";
            Version v1 = new Version(siteSetting.AppVersion), v2 = new Version(appVersion);
            if (v1 > v2)
            {
                if (type == (int)PlatformType.IOS)
                {
                    if (string.IsNullOrWhiteSpace(siteSetting.IOSDownLoad))
                    {
                        return Json(new { Success = "false", Code = 10004, Msg = "站点未设置IOS下载地址" });
                    }
                    downLoadUrl = siteSetting.IOSDownLoad;
                }
                if (type == (int)PlatformType.Android)
                {
                    if (string.IsNullOrWhiteSpace(siteSetting.AndriodDownLoad))
                    {
                        return Json(new { Success = "false", Code = 10003, Msg = "站点未设置Andriod下载地址" });
                    }
                    string str = siteSetting.AndriodDownLoad.Substring(siteSetting.AndriodDownLoad.LastIndexOf("/"), siteSetting.AndriodDownLoad.Length - siteSetting.AndriodDownLoad.LastIndexOf("/"));
                    var curProjRootPath = System.Web.Hosting.HostingEnvironment.MapPath("~/app") + str;
                    if (!File.Exists(curProjRootPath))
                    {
                        return Json(new { Success = "false", Code = 10002, Msg = "站点未上传app安装包" });
                    }
                    downLoadUrl = siteSetting.AndriodDownLoad;
                }
            }
            else
            {
                return Json(new { Success = "false", Code = 10001, Msg = "当前为最新版本" });
            }

            return Json(new { Success = "true", Code = 10000, DownLoadUrl = downLoadUrl, Description = siteSetting.AppUpdateDescription });
        }

        /// <summary>
        /// 获取App引导页图片
        /// </summary>
        /// <returns></returns>
        public List<Himall.DTO.SlideAdModel> GetAppGuidePages()
        {
            var result = SlideApplication.GetGuidePages();
            foreach (var item in result)
            {
                item.ImageUrl = HimallIO.GetRomoteImagePath(item.ImageUrl);
            }
            return result;
        }


        public object GetAboutUs()
        {
            var appModel = SystemAgreementApplication.GetAgreement(AgreementInfo.AgreementTypes.APP);
            var content = string.Empty;
            if (appModel != null)
                content = appModel.AgreementContent.Replace("src=\"/Storage/", "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));
            return SuccessResult(content);
        }
    }
}
