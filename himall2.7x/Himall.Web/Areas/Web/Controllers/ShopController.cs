using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Areas.Web.Helper;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Drawing;
using Himall.Core;
using System.IO;
using Himall.Application;
using Himall.CommonModel;

namespace Himall.Web.Areas.Web.Controllers
{
    public class ShopController : BaseWebController
    {
        #region 字段
        private ICouponService _iCouponService;
        private IShopService _iShopService;
        private IShopCategoryService _iShopCategoryService;
        private IVShopService _iVShopService;
        private IShopHomeModuleService _iShopHomeModuleService;
        private IRegionService _iRegionService;
        private INavigationService _iNavigationService;
        private ISlideAdsService _iSlideAdsService;
        private IProductService _iProductService;
        private IMemberService _iMemberService;
        private ISearchProductService _iSearchProductService;

        #endregion

        #region 构造函数
        public ShopController()
        { }

        public ShopController(ICouponService iCouponService, IShopService iShopService, IShopCategoryService iShopCategoryService, IVShopService iVShopService
            , IShopHomeModuleService iShopHomeModuleService, IRegionService iRegionService, INavigationService iNavigationService, ISlideAdsService iSlideAdsService
            , IProductService iProductService,
            IMemberService iMemberService,
            ISearchProductService iSearchProductService
            )
        {
            _iCouponService = iCouponService;
            _iShopService = iShopService;
            _iShopCategoryService = iShopCategoryService;
            _iVShopService = iVShopService;
            _iShopHomeModuleService = iShopHomeModuleService;
            _iRegionService = iRegionService;
            _iNavigationService = iNavigationService;
            _iSlideAdsService = iSlideAdsService;
            _iProductService = iProductService;
            _iMemberService = iMemberService;
            _iSearchProductService = iSearchProductService;
        }
        #endregion

        #region 方法
        // GET: Web/Shop
        [OutputCache(VaryByParam = "id", Duration = ConstValues.PAGE_CACHE_DURATION)]
        public ActionResult Home(string id)
        {
            long shopId = 0;
            ShopInfo shopObj = null;

            //shopId 不是数字
            if (!long.TryParse(id, out shopId))
            {
                return RedirectToAction("Error404", "Error", new { area = "Web" });
                //404 页面
            }

            //店铺Id不存在
            shopObj = _iShopService.GetShop(shopId);
            if (null == shopObj)
            {
                return RedirectToAction("Error404", "Error", new { area = "Web" });
                //404 页面
            }

            #region 初始化Model

            ShopHomeModel model = new ShopHomeModel
            {
                HotAttentionProducts = new List<HotProductInfo>(),
                HotSaleProducts = new List<HotProductInfo>(),
                Floors = new List<ShopHomeFloor>(),
                Navignations = new List<BannerInfo>(),
                Shop = new ShopInfoModel(),
                ShopCategory = new List<CategoryJsonModel>(),
                Slides = new List<SlideAdInfo>(),
                Logo = ""
            };


            #endregion

            #region 店铺信息

            var mark = ShopServiceMark.GetShopComprehensiveMark(shopObj.Id);
            model.Shop.Name = shopObj.ShopName;
            model.Shop.CompanyName = shopObj.CompanyName;
            model.Shop.Id = shopObj.Id;
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;
            model.Shop.Phone = shopObj.CompanyPhone;
            model.Shop.Address = _iRegionService.GetFullName(shopObj.CompanyRegionId);
            model.Logo = shopObj.Logo;

            #endregion

            if (shopObj.IsSelf)
            {
                model.Shop.PackMark = 5;
                model.Shop.ServiceMark = 5;
                model.Shop.ComprehensiveMark = 5;
            }



            #region 导航和3个推荐商品

            //导航
            model.Navignations = _iNavigationService.GetSellerNavigations(shopObj.Id).ToList();

            //banner和3个推荐商品
            var list = _iSlideAdsService.GetImageAds(shopObj.Id).OrderBy(item => item.Id).ToList();
            model.ImageAds = list.Where(p => !p.IsTransverseAD).ToList();
            model.TransverseAD = list.FirstOrDefault(p => p.IsTransverseAD);
            model.Slides = _iSlideAdsService.GetSlidAds(shopObj.Id, SlideAdInfo.SlideAdType.ShopHome).ToList();

            #endregion

            #region 店铺分类

            var categories = _iShopCategoryService.GetShopCategory(shopObj.Id).ToList();
            foreach (var main in categories.Where(s => s.ParentCategoryId == 0))
            {
                var topC = new CategoryJsonModel()
                {
                    Name = main.Name,
                    Id = main.Id.ToString(),
                    SubCategory = new List<SecondLevelCategory>()
                };
                foreach (var secondItem in categories.Where(s => s.ParentCategoryId == main.Id))
                {
                    var secondC = new SecondLevelCategory()
                    {
                        Name = secondItem.Name,
                        Id = secondItem.Id.ToString(),
                    };

                    topC.SubCategory.Add(secondC);
                }
                model.ShopCategory.Add(topC);
            }

            #endregion

            #region 楼层信息

            var shopHomeModules = _iShopHomeModuleService.GetAllShopHomeModuleInfos(shopObj.Id).Where(a => a.IsEnable).ToArray().OrderBy(p => p.DisplaySequence);
            foreach (var item in shopHomeModules)
            {
                List<ShopHomeFloorProduct> products = new List<ShopHomeFloorProduct>();
                foreach (var p in item.ShopHomeModuleProductInfo.Where(d => d.ProductInfo.AuditStatus == ProductInfo.ProductAuditStatus.Audited && d.ProductInfo.SaleStatus == ProductInfo.ProductSaleStatus.OnSale).ToList().OrderBy(p => p.DisplaySequence))
                {
                    products.Add(new ShopHomeFloorProduct
                    {
                        Id = p.ProductId,
                        Name = p.ProductInfo.ProductName,
                        Pic = p.ProductInfo.ImagePath,
                        Price = p.ProductInfo.MinSalePrice.ToString("f2"),
                        SaleCount = (int)_iProductService.GetProductVistInfo(p.ProductInfo.Id).SaleCounts
                    });
                }

                List<ShopHomeFloorTopImg> topimgs = new List<ShopHomeFloorTopImg>();
                foreach (var i in item.Himall_ShopHomeModulesTopImg.ToList().OrderBy(p => p.DisplaySequence))
                {
                    topimgs.Add(new ShopHomeFloorTopImg
                    {
                        Url = i.Url,
                        ImgPath = i.ImgPath
                    });
                }
                ShopHomeFloor floor = new ShopHomeFloor
                {
                    FloorName = item.Name,
                    FloorUrl = item.Url,
                    Products = products,
                    TopImgs = topimgs
                };

                model.Floors.Add(floor);
            }
            #endregion

            #region 热门销售

            var sale = _iProductService.GetHotSaleProduct(shopObj.Id, 5);
            if (sale != null)
            {
                foreach (var item in sale.ToArray())
                {
                    model.HotSaleProducts.Add(new HotProductInfo
                    {
                        ImgPath = item.ImagePath,
                        Name = item.ProductName,
                        Price = item.MinSalePrice,
                        Id = item.Id,
                        SaleCount = (int)item.SaleCounts
                    });
                }
            }

            #endregion

            #region 热门关注

            var hot = _iProductService.GetHotConcernedProduct(shopObj.Id, 5);
            if (hot != null)
            {
                foreach (var item in hot.ToList())
                {
                    model.HotAttentionProducts.Add(new HotProductInfo
                    {
                        ImgPath = item.ImagePath,
                        Name = item.ProductName,
                        Price = item.MinSalePrice,
                        Id = item.Id,
                        SaleCount = (int)item.ConcernedCount
                    });
                }
            }
            #endregion

            #region 累加浏览次数

            _iShopService.LogShopVisti(shopObj.Id);

            #endregion

            #region 微店二维码
            var vshop = _iVShopService.GetVShopByShopId(shopObj.Id);
            string vshopUrl = "";
            if (vshop != null)
            {
                vshopUrl = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/vshop/detail/" + vshop.Id;
                CreateQR(model, vshop.Logo, vshopUrl);
            }
            else
            {
                vshopUrl = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString();
                CreateQR(model, string.Empty, vshopUrl);
            }
            #endregion

            #region 店铺页脚
            model.Footer = _iShopHomeModuleService.GetFooter(shopObj.Id);
            #endregion

            ViewBag.IsExpired = _iShopService.IsExpiredShop(shopId);
            ViewBag.IsFreeze = _iShopService.IsFreezeShop(shopId);

            //补充当前店铺红包功能
            ViewBag.isShopPage = true;
            ViewBag.CurShopId = shopId;
            TempData["isShopPage"] = true;
            TempData["CurShopId"] = shopId;
            //统计店铺访问人数
            StatisticApplication.StatisticShopVisitUserCount(shopId);
            return View(model);
        }

        private void CreateQR(ShopHomeModel model, string shopLogo, string vshopUrl)
        {
            Image qrcode;
            //string logoFullPath = Server.MapPath(shopLogo);
            if (!Himall.Core.HimallIO.ExistFile(shopLogo))//|| !System.IO.File.Exists(logoFullPath)
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

            model.VShopQR = Convert.ToBase64String(arr);
            qrcode.Dispose();
        }

        public ActionResult Search(string sid, long cid = 0, string keywords = "", int pageNo = 1, decimal startPrice = 0, decimal endPrice = decimal.MaxValue)
        {

            int pageSize = 40;
            long shopId = 0;
            ShopInfo shopObj = null;

            endPrice = endPrice <= 0 || endPrice < startPrice ? decimal.MaxValue : endPrice;
            startPrice = startPrice < 0 ? 0 : startPrice;

            //shopId 不是数字
            if (!long.TryParse(sid, out shopId))
            {
                return RedirectToAction("Error404", "Error", new { area = "Web" });
                //404 页面
            }

            //店铺Id不存在
            shopObj = _iShopService.GetShop(shopId);
            if (null == shopObj)
            {
                return RedirectToAction("Error404", "Error", new { area = "Web" });
                //404 页面
            }
            #region 初始化Model

            ShopHomeModel model = new ShopHomeModel
            {
                HotAttentionProducts = new List<HotProductInfo>(),
                HotSaleProducts = new List<HotProductInfo>(),
                Floors = new List<ShopHomeFloor>(),
                Navignations = new List<BannerInfo>(),
                Shop = new ShopInfoModel(),
                ShopCategory = new List<CategoryJsonModel>(),
                Slides = new List<SlideAdInfo>(),
                Logo = ""
            };

            #endregion

            #region 导航和3个推荐商品

            //导航
            model.Navignations = _iNavigationService.GetSellerNavigations(shopObj.Id).ToList();

            //banner和3个推荐商品
            model.ImageAds = _iSlideAdsService.GetImageAds(shopObj.Id).OrderBy(item => item.Id).ToList();

            model.Slides = _iSlideAdsService.GetSlidAds(shopObj.Id, SlideAdInfo.SlideAdType.ShopHome).ToList();

            #endregion

            #region 店铺分类

            var categories = _iShopCategoryService.GetShopCategory(shopObj.Id).ToArray();
            foreach (var main in categories.Where(s => s.ParentCategoryId == 0))
            {
                var topC = new CategoryJsonModel()
                {
                    Name = main.Name,
                    Id = main.Id.ToString(),
                    SubCategory = new List<SecondLevelCategory>()
                };
                foreach (var secondItem in categories.Where(s => s.ParentCategoryId == main.Id))
                {
                    var secondC = new SecondLevelCategory()
                    {
                        Name = secondItem.Name,
                        Id = secondItem.Id.ToString(),
                    };

                    topC.SubCategory.Add(secondC);
                }
                model.ShopCategory.Add(topC);
            }

            #endregion

            #region 店铺信息

            var mark = ShopServiceMark.GetShopComprehensiveMark(shopObj.Id);
            model.Shop.Name = shopObj.ShopName;
            model.Shop.CompanyName = shopObj.CompanyName;
            model.Shop.Id = shopObj.Id;
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;
            model.Shop.Phone = shopObj.CompanyPhone;
            model.Shop.Address = _iRegionService.GetFullName(shopObj.CompanyRegionId);
            model.Logo = shopObj.Logo;

            #endregion

            #region 初始化查询Model并查询商品
            SearchProductQuery search = new SearchProductQuery()
            {
                StartPrice = startPrice,
                EndPrice = endPrice,
                ShopId = shopId,
                BrandId = 0,
                ShopCategoryId = cid,
                Keyword = keywords,
                OrderKey = 0,
                OrderType = true,
                PageSize = pageSize,
                PageNumber = pageNo
            };

            var productsModel = _iSearchProductService.SearchProduct(search);
            int total = productsModel.Total;
            var products = productsModel.Data;

            foreach (var pro in products)
            {
                pro.SaleCount = (int)_iProductService.GetProductVistInfo(pro.ProductId).SaleCounts;
            }
            model.Products = products;

            #endregion

            #region 热门销售

            var sale = _iProductService.GetHotSaleProduct(shopObj.Id, 5);
            if (sale != null)
            {
                foreach (var item in sale)
                {
                    model.HotSaleProducts.Add(new HotProductInfo
                    {
                        ImgPath = item.ImagePath,
                        Name = item.ProductName,
                        Price = item.MinSalePrice,
                        Id = item.Id,
                        SaleCount = (int)item.SaleCounts
                    });
                }
            }

            #endregion


            #region 热门关注

            var hot = _iProductService.GetHotConcernedProduct(shopObj.Id, 5).ToList();
            if (hot != null)
            {
                foreach (var item in hot)
                {
                    model.HotAttentionProducts.Add(new HotProductInfo
                    {
                        ImgPath = item.ImagePath,
                        Name = item.ProductName,
                        Price = item.MinSalePrice,
                        Id = item.Id,
                        SaleCount = (int)item.ConcernedCount
                    });
                }
            }
            #endregion

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = _iShopService.GetShopStatisticOrderComments(shopId);

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

            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = total
            };
            ViewBag.pageInfo = info;
            #endregion
            var categoryName = string.Empty;
            if (keywords == string.Empty)
            {
                if (cid != 0)
                {

                    var category = _iShopCategoryService.GetCategory(cid) ?? new ShopCategoryInfo() { };
                    categoryName = category.Name;
                }
            }
            ViewBag.CategoryName = categoryName;
            ViewBag.Keyword = keywords;
            ViewBag.cid = cid;
            ViewBag.BrowsedHistory = BrowseHistrory.GetBrowsingProducts(13, CurrentUser == null ? 0 : CurrentUser.Id);

            //补充当前店铺红包功能
            ViewBag.isShopPage = true;
            ViewBag.CurShopId = shopId;
            TempData["isShopPage"] = true;
            TempData["CurShopId"] = shopId;

            return View(model);

        }

        public ActionResult SearchAd(string sid, long cid = 0, string keywords = "", int pageNo = 1, decimal startPrice = 0, decimal endPrice = decimal.MaxValue)
        {

            int pageSize = 40;
            long shopId = 0;
            ShopInfo shopObj = null;

            endPrice = endPrice <= 0 || endPrice < startPrice ? decimal.MaxValue : endPrice;
            startPrice = startPrice < 0 ? 0 : startPrice;

            //shopId 不是数字
            if (!long.TryParse(sid, out shopId))
            {
                return RedirectToAction("Error404", "Error", new { area = "Web" });
                //404 页面
            }

            //店铺Id不存在
            shopObj = _iShopService.GetShop(shopId);
            if (null == shopObj)
            {
                return RedirectToAction("Error404", "Error", new { area = "Web" });
                //404 页面
            }
            #region 初始化Model

            ShopHomeModel model = new ShopHomeModel
            {
                HotAttentionProducts = new List<HotProductInfo>(),
                HotSaleProducts = new List<HotProductInfo>(),
                Floors = new List<ShopHomeFloor>(),
                Navignations = new List<BannerInfo>(),
                Shop = new ShopInfoModel(),
                ShopCategory = new List<CategoryJsonModel>(),
                Slides = new List<SlideAdInfo>(),
                Logo = ""
            };

            #endregion

            #region 导航和3个推荐商品

            //导航
            model.Navignations = _iNavigationService.GetSellerNavigations(shopObj.Id).ToList();

            //banner和3个推荐商品
            model.ImageAds = _iSlideAdsService.GetImageAds(shopObj.Id).OrderBy(item => item.Id).ToList();

            model.Slides = _iSlideAdsService.GetSlidAds(shopObj.Id, SlideAdInfo.SlideAdType.ShopHome).ToList();

            #endregion

            #region 店铺分类

            var categories = _iShopCategoryService.GetShopCategory(shopObj.Id).ToArray();
            foreach (var main in categories.Where(s => s.ParentCategoryId == 0))
            {
                var topC = new CategoryJsonModel()
                {
                    Name = main.Name,
                    Id = main.Id.ToString(),
                    SubCategory = new List<SecondLevelCategory>()
                };
                foreach (var secondItem in categories.Where(s => s.ParentCategoryId == main.Id))
                {
                    var secondC = new SecondLevelCategory()
                    {
                        Name = secondItem.Name,
                        Id = secondItem.Id.ToString(),
                    };

                    topC.SubCategory.Add(secondC);
                }
                model.ShopCategory.Add(topC);
            }

            #endregion

            #region 店铺信息

            var mark = ShopServiceMark.GetShopComprehensiveMark(shopObj.Id);
            model.Shop.Name = shopObj.ShopName;
            model.Shop.CompanyName = shopObj.CompanyName;
            model.Shop.Id = shopObj.Id;
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;
            model.Shop.Phone = shopObj.CompanyPhone;
            model.Shop.Address = _iRegionService.GetFullName(shopObj.CompanyRegionId);
            model.Logo = shopObj.Logo;
            #endregion

            SearchProductQuery query = new SearchProductQuery()
            {
                ShopId = long.Parse(sid),
                ShopCategoryId = cid,
                Keyword = keywords,
                StartPrice = startPrice,
                EndPrice = endPrice,
                PageNumber = pageNo,
                PageSize = pageSize
            };

            SearchProductResult result = _iSearchProductService.SearchProduct(query);
            
            model.Products = result.Data;

            //#endregion

            #region 热门销售

            var sale = _iProductService.GetHotSaleProduct(shopObj.Id, 5);
            if (sale != null)
            {
                foreach (var item in sale)
                {
                    model.HotSaleProducts.Add(new HotProductInfo
                    {
                        ImgPath = item.ImagePath,
                        Name = item.ProductName,
                        Price = item.MinSalePrice,
                        Id = item.Id,
                        SaleCount = (int)item.SaleCounts
                    });
                }
            }

            #endregion


            #region 热门关注

            var hot = _iProductService.GetHotConcernedProduct(shopObj.Id, 5).ToList();
            if (hot != null)
            {
                foreach (var item in hot)
                {
                    model.HotAttentionProducts.Add(new HotProductInfo
                    {
                        ImgPath = item.ImagePath,
                        Name = item.ProductName,
                        Price = item.MinSalePrice,
                        Id = item.Id,
                        SaleCount = (int)item.ConcernedCount
                    });
                }
            }
            #endregion

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = _iShopService.GetShopStatisticOrderComments(shopId);

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

            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = result.Total
            };
            ViewBag.pageInfo = info;
            #endregion
            var categoryName = string.Empty;
            if (keywords == string.Empty)
            {
                if (cid != 0)
                {

                    var category = _iShopCategoryService.GetCategory(cid) ?? new ShopCategoryInfo() { };
                    categoryName = category.Name;
                }
            }
            ViewBag.CategoryName = categoryName;
            ViewBag.Keyword = keywords;
            ViewBag.cid = cid;
            ViewBag.BrowsedHistory = BrowseHistrory.GetBrowsingProducts(13, CurrentUser == null ? 0 : CurrentUser.Id);

            //补充当前店铺红包功能
            ViewBag.isShopPage = true;
            ViewBag.CurShopId = shopId;
            TempData["isShopPage"] = true;
            TempData["CurShopId"] = shopId;
            //统计店铺访问人数
            StatisticApplication.StatisticShopVisitUserCount(shopId);          
            return View(model);

        }

        /// <summary>
        /// 获取店铺的红包列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PostShopCoupons(long id)
        {
            var datalist = _iCouponService.GetTopCoupon(id);
            var data = datalist.Select(d => new ShopCouponsListModel
            {
                Id = d.Id,
                CreateTime = d.CreateTime,
                ShopId = d.ShopId,
                ShopName = d.ShopName,
                Price = d.Price,
                PerMax = d.PerMax,
                OrderAmount = d.OrderAmount,
                Num = d.Num,
                StartTime = d.StartTime,
                EndTime = d.EndTime,
                CouponName = d.CouponName,
                ReceiveType = d.ReceiveType
            }).ToList();
            return Json(data);
        }

        /// <summary>
        /// 获取店铺的可用红包列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PostCanUseShopCoupons(long id)
        {
            if (CurrentUser != null)
            {
                var allcoulist = _iCouponService.GetAllUserCoupon(CurrentUser.Id);
                var datalist = allcoulist.Where(d => d.ShopId == id);
                var data = datalist.Select(d => new ShopCouponsListModel
                {
                    Id = d.CouponId,
                    CreateTime = d.CreateTime,
                    ShopId = d.ShopId,
                    ShopName = d.ShopName,
                    Price = d.Price,
                    PerMax = d.PerMax,
                    OrderAmount = d.OrderAmount,
                    Num = d.Num,
                    StartTime = d.StartTime,
                    EndTime = d.EndTime,
                    CouponName = d.CouponName
                }).ToList();
                return Json(data);
            }
            else
            {
                List<ShopCouponsListModel> data = new List<ShopCouponsListModel>();
                return Json(data);
            }
        }

        [HttpPost]
        public JsonResult AddFavorite(long shopId)
        {
            _iShopService.AddFavoriteShop(CurrentUser.Id, shopId);
            return Json(new { success = true });
        }

        /// <summary>
        /// 领取优惠券
        /// </summary>
        /// <param name="couponId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ReceiveCoupons(long couponId, long shopId)
        {
            var service = _iCouponService;
            var model = service.GetCouponInfo(shopId, couponId);
            if (CurrentUser == null)
            {
                return Json(new Result { success = false, msg = "请登录后领取", status = -1 });
            }
            if (model.EndTime < DateTime.Now.Date)
            {
                return Json(new Result { success = false, msg = "此优惠券已过期", status = -2 });
            }
            var max = service.GetUserReceiveCoupon(couponId, CurrentUser.Id);
            if (model.PerMax != 0 && max >= model.PerMax)
            {
                return Json(new Result { success = false, msg = "每人最多领取" + model.PerMax + "张该类型的优惠券", status = -3 });
            }
            if (model.Himall_CouponRecord.Count >= model.Num)
            {
                return Json(new Result { success = false, msg = "优惠券已领完", status = -3 });
            }

            if (model.ReceiveType == CouponInfo.CouponReceiveType.IntegralExchange)
            {
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                if (userInte.AvailableIntegrals < model.NeedIntegral)
                {
                    return Json(new Result { success = false, msg = "积分不足", status = -4 });
                }
            }
            CouponRecordInfo info = new CouponRecordInfo();
            info.UserId = CurrentUser.Id;
            info.UserName = CurrentUser.UserName;
            info.ShopId = shopId;
            info.CouponId = couponId;
            service.AddCouponRecord(info);
            return Json(new Result { success = true, msg = "领取成功", status = 1 });
        }

        public JsonResult ExistShopBranch(int shopId, int regionId, long[] productIds)
        {
            var query = new CommonModel.ShopBranchQuery();
            query.Status = CommonModel.ShopBranchStatus.Normal;
            query.ShopId = shopId;

            var region = RegionApplication.GetRegion(regionId, CommonModel.Region.RegionLevel.City);
            query.AddressPath = region.GetIdPath();
            query.ProductIds = productIds;
            var existShopBranch = ShopBranchApplication.Exists(query);

            return Json(existShopBranch, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取市级的所有子区域
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        public JsonResult GetCitySubRegions(int regionId)
        {
            var region = RegionApplication.GetRegion(regionId, CommonModel.Region.RegionLevel.County);
            var subRegion = RegionApplication.GetSubRegion(region.ParentId);

            return Json(new
            {
                regions = subRegion,
                selectId = region.Id
            }, true);
        }
        #endregion
    }
}