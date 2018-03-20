using Himall.IServices;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Himall.Web;
using Himall.Model;
using System.IO;
using System.Drawing;
using Himall.Core;
using Himall.Application;

namespace Himall.Web.Areas.Web.Controllers
{
    public class ProductPartialController : BaseWebController
    {
        private IMemberIntegralService _iMemberIntegralService;
        private IProductService _iProductService;
        private ICouponService _iCouponService;
        private IShopBonusService _iShopBonusService;
        private IArticleService _iArticleService;
        private IArticleCategoryService _iArticleCategoryService;
        private INavigationService _iNavigationService;
        private IHomeCategoryService _iHomeCategoryService;
        private IBrandService _iBrandService;
        private ICategoryService _iCategoryService;
        private ICustomerService _iCustomerService;
        private ISlideAdsService _iSlideAdsService;

        public ProductPartialController(
            IMemberIntegralService iMemberIntegralService,
            IProductService iProductService,
            ICouponService iCouponService,
            IShopBonusService iShopBonusService,
            IArticleService iArticleService,
            IArticleCategoryService iArticleCategoryService,
            INavigationService iNavigationService,
            IHomeCategoryService iHomeCategoryService,
            IBrandService iBrandService,
            ICategoryService iCategoryService,
            ICustomerService iCustomerService,
            ISlideAdsService iSlideAdsService

            )
        {
            _iMemberIntegralService = iMemberIntegralService;
            _iProductService = iProductService;
            _iCouponService = iCouponService;
            _iShopBonusService = iShopBonusService;
            _iArticleService = iArticleService;
            _iArticleCategoryService = iArticleCategoryService;
            _iNavigationService = iNavigationService;
            _iHomeCategoryService = iHomeCategoryService;
            _iBrandService = iBrandService;
            _iCategoryService = iCategoryService;
            _iCustomerService = iCustomerService;
            _iSlideAdsService = iSlideAdsService;

        }
        /// <summary>
        /// 页面缓存时间
        /// </summary>


        // GET: Web/ProductPartial
        [OutputCache(Duration = ConstValues.PAGE_CACHE_DURATION)]
        public ActionResult Header()
        {
            ViewBag.Now = DateTime.Now;
            bool isLogin = CurrentUser != null;
            var model = new ProductPartialHeaderModel();
            model.PlatformCustomerServices = CustomerServiceApplication.GetPlatformCustomerService(true, false);
            model.isLogin = isLogin ? "true" : "false";
            //用户积分
            model.MemberIntegral = isLogin ? _iMemberIntegralService.GetMemberIntegral(CurrentUser.Id).AvailableIntegrals : 0;

            //关注商品
            //var concern = isLogin ? _iProductService.GetUserAllConcern(CurrentUser.Id) : new List<FavoriteInfo>();
            //model.Concern = concern.Take(10).ToList();

            List<IBaseCoupon> baseCoupons = new List<IBaseCoupon>();
            //优惠卷
            var coupons = isLogin ? _iCouponService.GetAllUserCoupon(CurrentUser.Id).ToList() : new List<UserCouponInfo>();
            coupons = coupons == null ? new List<UserCouponInfo>() : coupons;
            baseCoupons.AddRange(coupons);

            //红包
            var shopBonus = isLogin ? _iShopBonusService.GetCanUseDetailByUserId(CurrentUser.Id) : new List<ShopBonusReceiveInfo>();
            shopBonus = shopBonus == null ? new List<ShopBonusReceiveInfo>() : shopBonus;
            baseCoupons.AddRange(shopBonus);
            model.BaseCoupon = baseCoupons;

            //广告
            var imageAds = _iSlideAdsService.GetImageAds(0).Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.HeadRightAds).ToList();
            if (imageAds.Count > 0)
            {
                ViewBag.HeadAds = imageAds;
            }
            else
            {
                ViewBag.HeadAds = _iSlideAdsService.GetImageAds(0).Take(1).ToList();
            }
            //浏览的商品
            //var browsingPro = isLogin ? BrowseHistrory.GetBrowsingProducts(10, CurrentUser == null ? 0 : CurrentUser.Id) : new List<ProductBrowsedHistoryModel>();
            //model.BrowsingProducts = browsingPro;

            InitHeaderData();

            return PartialView("~/Areas/Web/Views/Shared/Header.cshtml", model);
        }

        [OutputCache(VaryByParam = "id", Duration = ConstValues.PAGE_CACHE_DURATION)]

        public ActionResult ShopHeader(long id)
        {
            InitHeaderData();
            #region 获取店铺的评价统计
            var shopStatisticOrderComments = ServiceHelper.Create<IShopService>().GetShopStatisticOrderComments(id);

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
                ViewBag.ProductAndDescription = productAndDescription.CommentValue;
                ViewBag.ProductAndDescriptionPeer = productAndDescriptionPeer.CommentValue;
                ViewBag.ProductAndDescriptionMin = productAndDescriptionMin.CommentValue;
                ViewBag.ProductAndDescriptionMax = productAndDescriptionMax.CommentValue;
            }
            else
            {
                ViewBag.ProductAndDescription = defaultValue;
                ViewBag.ProductAndDescriptionPeer = defaultValue;
                ViewBag.ProductAndDescriptionMin = defaultValue;
                ViewBag.ProductAndDescriptionMax = defaultValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null)
            {
                ViewBag.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
                ViewBag.SellerServiceAttitudePeer = sellerServiceAttitudePeer.CommentValue;
                ViewBag.SellerServiceAttitudeMax = sellerServiceAttitudeMax.CommentValue;
                ViewBag.SellerServiceAttitudeMin = sellerServiceAttitudeMin.CommentValue;
            }
            else
            {
                ViewBag.SellerServiceAttitude = defaultValue;
                ViewBag.SellerServiceAttitudePeer = defaultValue;
                ViewBag.SellerServiceAttitudeMax = defaultValue;
                ViewBag.SellerServiceAttitudeMin = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null)
            {
                ViewBag.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
                ViewBag.SellerDeliverySpeedPeer = sellerDeliverySpeedPeer.CommentValue;
                ViewBag.SellerDeliverySpeedMax = sellerDeliverySpeedMax.CommentValue;
                ViewBag.sellerDeliverySpeedMin = sellerDeliverySpeedMin.CommentValue;
            }
            else
            {
                ViewBag.SellerDeliverySpeed = defaultValue;
                ViewBag.SellerDeliverySpeedPeer = defaultValue;
                ViewBag.SellerDeliverySpeedMax = defaultValue;
                ViewBag.sellerDeliverySpeedMin = defaultValue;
            }
            #endregion

            #region 微店二维码
            var vshop = ServiceHelper.Create<IVShopService>().GetVShopByShopId(id);
            string vshopUrl = "";
            if (vshop != null)
            {
                vshopUrl = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/vshop/detail/" + vshop.Id;
                ViewBag.VShopQR = CreateQR(vshop.Logo, vshopUrl);
            }
            else
            {
                vshopUrl = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString();
                ViewBag.VShopQR = CreateQR(null, vshopUrl);
            }
            #endregion

            ViewBag.ShopName = ServiceHelper.Create<IShopService>().GetShop(id).ShopName;


            bool isLogin = CurrentUser != null;
            var model = new ProductPartialHeaderModel();
            model.ShopId = id;
            model.isLogin = isLogin ? "true" : "false";
            //用户积分
            model.MemberIntegral = isLogin ? _iMemberIntegralService.GetMemberIntegral(CurrentUser.Id).AvailableIntegrals : 0;

            //关注商品
            //var concern = isLogin ? _iProductService.GetUserAllConcern(CurrentUser.Id) : new List<FavoriteInfo>();
            //model.Concern = concern.Take(10).ToList();

            List<IBaseCoupon> baseCoupons = new List<IBaseCoupon>();
            //优惠卷
            var coupons = isLogin ? _iCouponService.GetAllUserCoupon(CurrentUser.Id).ToList() : new List<UserCouponInfo>();
            coupons = coupons == null ? new List<UserCouponInfo>() : coupons;
            baseCoupons.AddRange(coupons);

            //红包
            var shopBonus = isLogin ? _iShopBonusService.GetCanUseDetailByUserId(CurrentUser.Id) : new List<ShopBonusReceiveInfo>();
            shopBonus = shopBonus == null ? new List<ShopBonusReceiveInfo>() : shopBonus;
            baseCoupons.AddRange(shopBonus);
            model.BaseCoupon = baseCoupons;

            //浏览的商品
            //var browsingPro = isLogin ? BrowseHistrory.GetBrowsingProducts(10, CurrentUser == null ? 0 : CurrentUser.Id) : new List<ProductBrowsedHistoryModel>();
            //model.BrowsingProducts = browsingPro;
            InitHeaderData();
            setTheme();//主题设置
            return PartialView("~/Areas/Web/Views/Shared/ShopHeader.cshtml", model);
        }

        private string CreateQR(string shopLogo, string vshopUrl)
        {
            Image qrcode;
            //string logoFullPath = Server.MapPath( vshop.Logo );
            if (string.IsNullOrWhiteSpace(shopLogo) || !HimallIO.ExistFile(shopLogo))// || !System.IO.File.Exists( logoFullPath ) 
                qrcode = Core.Helper.QRCodeHelper.Create(vshopUrl);
            else
                qrcode = Core.Helper.QRCodeHelper.Create(vshopUrl, Core.HimallIO.GetRomoteImagePath(shopLogo));

            Bitmap bmp = new Bitmap(qrcode);
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] arr = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();

            qrcode.Dispose();
            return Convert.ToBase64String(arr);

        }

        //public ActionResult UserInfo()
        //{
        //    return PartialView("~/Areas/Web/Views/Shared/_UserCenterLeft.cshtml", CurrentUser);
        //}

        [ChildActionOnly]
        public ActionResult TopInfo()
        {

            ViewBag.SiteName = CurrentSiteSetting.SiteName;
            //  ViewBag.IsSeller = false;

            var model = CurrentUser;

            ViewBag.APPCanDownload = CurrentSiteSetting.CanDownload;
            if (CurrentSiteSetting.CanDownload)
            {
                string host = Request.Url.Host;
                host += Request.Url.Port != 80 ? ":" + Request.Url.Port.ToString() : "";
                var link = String.Format("http://{0}/m-wap/home/downloadApp", host);
                var map = Core.Helper.QRCodeHelper.Create(link);
                MemoryStream ms = new MemoryStream();
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                //  将图片内存流转成base64,图片以DataURI形式显示  
                string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
                ms.Dispose();
                ViewBag.APPQR = strUrl;
            }
            ViewBag.QR = CurrentSiteSetting.QRCode;
            return PartialView("~/Areas/Web/Views/Shared/_TopInfo.cshtml", model);
        }

        [OutputCache(Duration = ConstValues.PAGE_CACHE_DURATION)]
        public ActionResult Foot()
        {
            //页脚
            var articleCategoryService = _iArticleCategoryService;
            var articleService = _iArticleService;
            //服务文章
            var pageFootServiceCategory = articleCategoryService.GetSpecialArticleCategory(SpecialCategory.PageFootService);
            if (pageFootServiceCategory == null) { return PartialView("~/Areas/Web/Views/Shared/Foot.cshtml"); }
            var pageFootServiceSubCategies = articleCategoryService.GetArticleCategoriesByParentId(pageFootServiceCategory.Id);
            var pageFootService = pageFootServiceSubCategies.ToArray().Select(item =>
                 new PageFootServiceModel()
                 {
                     CateogryName = item.Name,
                     Articles = articleService.GetArticleByArticleCategoryId(item.Id).Where(t => t.IsRelease)
                 }
                );
            ViewBag.PageFootService = pageFootService;

            //页脚
            ViewBag.PageFoot = CurrentSiteSetting.PageFoot;
            ViewBag.QRCode = CurrentSiteSetting.QRCode;
            ViewBag.SiteName = CurrentSiteSetting.SiteName;
            return PartialView("~/Areas/Web/Views/Shared/Foot.cshtml");
        }

        public ActionResult GetBrowedProductList()
        {
            var list = BrowseHistrory.GetBrowsingProducts(5, CurrentUser == null ? 0 : CurrentUser.Id);
            return PartialView("_ProductBrowsedHistory", list);
        }

        public ActionResult OrderTopBarCss()
        {
            Himall.DTO.Theme vmTheme = ThemeApplication.getTheme();
            return PartialView("_OrderTopBarCss", vmTheme);
        }

        public ActionResult UserCenterCss()
        {
            Himall.DTO.Theme vmTheme = ThemeApplication.getTheme();
            return PartialView("_UserCenterCss", vmTheme);
        }

        [HttpGet]
        public JsonResult GetBrowedProduct()
        {
            var p = BrowseHistrory.GetBrowsingProducts(5, CurrentUser == null ? 0 : CurrentUser.Id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 初始化页面头部  此方法在使用redis缓存时有可能比较慢，所以增加调试信息输出
        /// </summary>
        void InitHeaderData()
        {
            //SiteName
            ViewBag.SiteName = CurrentSiteSetting.SiteName;

            //Logo
            ViewBag.Logo = CurrentSiteSetting.Logo;

            //搜索输入框关键字
            ViewBag.Keyword = CurrentSiteSetting.Keyword;

            //热门关键字
            ViewBag.HotKeyWords = !string.IsNullOrWhiteSpace(CurrentSiteSetting.Hotkeywords) ? CurrentSiteSetting.Hotkeywords.Split(',') : new string[] { };

            //导航
            ViewBag.Navigators = _iNavigationService.GetPlatNavigations().ToArray();
            //分类

            var categories = _iHomeCategoryService.GetHomeCategorySets().ToList();
            ViewBag.Categories = categories;

            var categoryService = _iCategoryService;
            ViewBag.AllSecondCategoies = categoryService.GetFirstAndSecondLevelCategories().Where(item => item.Depth == 2 && item.IsDeleted==false).ToList();

            var service = _iBrandService;
            var brands = new Dictionary<int, IEnumerable<Model.BrandInfo>>();

            //页脚
            ViewBag.PageFoot = CurrentSiteSetting.PageFoot;

            //分类品牌
            ViewBag.CategoryBrands = brands;

            //会员信息
            ViewBag.Member = CurrentUser;

            ViewBag.APPCanDownload = CurrentSiteSetting.CanDownload;
            if (CurrentSiteSetting.CanDownload)
            {
                string host = Request.Url.Host;
                host += Request.Url.Port != 80 ? ":" + Request.Url.Port.ToString() : "";
                var link = String.Format("http://{0}/m-wap/home/downloadApp", host);
                var map = Core.Helper.QRCodeHelper.Create(link);
                MemoryStream ms = new MemoryStream();
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                //  将图片内存流转成base64,图片以DataURI形式显示  
                string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
                ms.Dispose();
                ViewBag.APPQR = strUrl;
            }

            setTheme();//主题设置
        }

        /// <summary>
        /// 主题设置
        /// </summary>
        private void setTheme()
        {
            Himall.DTO.Theme vmTheme = ThemeApplication.getTheme();
            ViewBag.TypeId = vmTheme.TypeId;
            ViewBag.WritingColor = vmTheme.WritingColor;
            ViewBag.SecondaryColor = vmTheme.SecondaryColor;
            ViewBag.MainColor = vmTheme.MainColor;
            ViewBag.FrameColor = vmTheme.FrameColor;
            ViewBag.ClassifiedsColor = vmTheme.ClassifiedsColor;
        }

        public ActionResult Logo()
        {
            return Content(HimallIO.GetImagePath(CurrentSiteSetting.Logo));
        }

        public ActionResult GetShopCoupon(long shopId)
        {
            var model = _iCouponService.GetTopCoupon(shopId);
            return PartialView("_ShopCoupon", model);
        }
    }
}