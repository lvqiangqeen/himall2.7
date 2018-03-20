using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Models;
using Senparc.Weixin.MP.CommonAPIs;
using Himall.Web.App_Code.Common;
using Himall.Application;
using Himall.Core;

namespace Himall.Web.Areas.Web.Controllers
{
    public class LimitTimeBuyController : BaseWebController
    {
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private ISlideAdsService _iSlideAdsService;
        private IShopService _iShopService;
        private IProductService _iProductService;
        private IProductDescriptionTemplateService _iProductDescriptionTemplateService;
        private IShopCategoryService _iShopCategoryService;
        private ICommentService _iCommentService;
        private IConsultationService _iConsultationService;
        private ICouponService _iCouponService;
        private ICashDepositsService _iCashDepositsService;
        private ISiteSettingService _iSiteSettingService;
        private ITypeService _iTypeService;
        public LimitTimeBuyController(
            ILimitTimeBuyService iLimitTimeBuyService,
            ISlideAdsService iSlideAdsService,
            IShopService iShopService,
            IProductService iProductService,
            IProductDescriptionTemplateService iProductDescriptionTemplateService,
            IShopCategoryService iShopCategoryService,
            ICommentService iCommentService,
            IConsultationService iConsultationService,
            ICouponService iCouponService,
            ICashDepositsService iCashDepositsService,
            ISiteSettingService iSiteSettingService, ITypeService iTypeService
            )
        {
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iSlideAdsService = iSlideAdsService;
            _iShopService = iShopService;
            _iProductService = iProductService;
            _iProductDescriptionTemplateService = iProductDescriptionTemplateService;
            _iShopCategoryService = iShopCategoryService;
            _iCommentService = iCommentService;
            _iConsultationService = iConsultationService;
            _iCouponService = iCouponService;
            _iCashDepositsService = iCashDepositsService;
            _iSiteSettingService = iSiteSettingService;
            _iTypeService = iTypeService;

        }
        // GET: Web/LimitTimeBuy
        public ActionResult Home(
            string keywords = "", /* 搜索关键字 */
            string catename = "",/* 分类名*/
            int orderKey = 5, /* 排序项（1：默认，2：销量，3：价格，4 : 结束时间,5:状态） */
            int orderType = 1, /* 排序方式（1：升序，2：降序） */
            int isStart = 0,     /*是否开始( 1 : 开始 , 2 : 未开始 )*/
            int pageNo = 1, /*页码*/
            int pageSize = 60 /*每页显示数据量*/
            )
        {
            #region 初始化查询Model
            FlashSaleQuery model = new FlashSaleQuery()
            {
                ItemName = keywords,
                OrderKey = orderKey,
                OrderType = orderType,
                CategoryName = catename,
                IsStart = isStart,
                IsPreheat=true,
                PageNo = pageNo,
                PageSize = pageSize,
                AuditStatus = FlashSaleInfo.FlashSaleStatus.Ongoing,
                CheckProductStatus = true
            };

            #endregion

            #region ViewBag

            List<SelectListItem> CateSelItem = new List<SelectListItem>();
            var cateArray = _iLimitTimeBuyService.GetServiceCategories();
            foreach (var cate in cateArray)
            {
                CateSelItem.Add(new SelectListItem { Selected = false, Text = cate, Value = cate });
            }
            if (!string.IsNullOrWhiteSpace(catename))
            {
                var _tmp = CateSelItem.FirstOrDefault(c => c.Text.Equals(catename));
                if (_tmp != null)
                {
                    _tmp.Selected = true;
                }
            }

            ViewBag.Cate = CateSelItem;
            ViewBag.keywords = keywords;
            ViewBag.orderKey = orderKey;
            ViewBag.orderType = orderType;
            ViewBag.catename = catename;
            ViewBag.Logined = (null != CurrentUser) ? 1 : 0;
            ViewBag.isStart = isStart;
            ViewBag.Slide = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.PlatformLimitTime);

            #endregion


            #region 查询商品

            //var itemsModel = _iLimitTimeBuyService.GetItemList(model);
            var itemsModel = _iLimitTimeBuyService.GetAll(model);
            int total = itemsModel.Total;
            var items = itemsModel.Models.ToArray();


            if (itemsModel.Total == 0)
            {
                ViewBag.keywords = keywords;
                return View();
            }


            #endregion


            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = model.PageNo,
                ItemsPerPage = pageSize,
                TotalItems = total
            };
            ViewBag.pageInfo = info;
            #endregion

            return View(items ?? new FlashSaleInfo[] { });
        }

        private bool WXIsConfig(string appId, string appSecret)
        {
            return (!string.IsNullOrWhiteSpace(appId)) && (!string.IsNullOrWhiteSpace(appSecret));
        }

        public ActionResult Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Error404", "Error", new { area = "Web" });
            string price = "";

            #region 定义Model和变量

            LimitTimeProductDetailModel model = new LimitTimeProductDetailModel
            {
                MainId = long.Parse(id),
                HotAttentionProducts = new List<HotProductInfo>(),
                HotSaleProducts = new List<HotProductInfo>(),
                Product = new Model.ProductInfo(),
                Shop = new ShopInfoModel(),
                ShopCategory = new List<CategoryJsonModel>(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };

            FlashSaleModel market = null;
            ShopInfo shop = null;

            long gid = 0, mid = 0;

            #endregion


            #region 商品Id不合法
            if (long.TryParse(id, out mid)) { }
            if (mid == 0)
            {
                //跳转到出错页面
                return RedirectToAction("Error404", "Error", new { area = "Web" });
            }
            #endregion


            #region 初始化商品和店铺

            market = _iLimitTimeBuyService.Get(mid);

            switch (market.Status)
            {
                case FlashSaleInfo.FlashSaleStatus.Ended:
                    return RedirectToAction("Detail", "Product", new { id = market.ProductId });
                    break;
                case FlashSaleInfo.FlashSaleStatus.Cancelled:
                    return RedirectToAction("Detail", "Product", new { id = market.ProductId });
                    break;
            }
            if (market.Status != FlashSaleInfo.FlashSaleStatus.Ongoing)
            {
                return RedirectToAction("Home");
            }
            model.FlashSale = market;
            if (market == null || market.Id == 0 || market.Status != FlashSaleInfo.FlashSaleStatus.Ongoing)
            {
                //可能参数是商品ID
                market = market == null ? _iLimitTimeBuyService.GetFlaseSaleByProductId(mid) : market;
                if (market == null)
                {
                    //跳转到404页面
                    return RedirectToAction("Error404", "Error", new { area = "Mobile" });
                }
                if (market.Status != FlashSaleInfo.FlashSaleStatus.Ongoing)
                {
                    return RedirectToAction("Detail", "Product", new { id = market.ProductId });
                }
            }

            model.MaxSaleCount = market.LimitCountOfThePeople;
            model.Title = market.Title;

            shop = _iShopService.GetShop(market.ShopId);
            model.Shop.Name = shop.ShopName;
            #endregion

            #region 商品描述
            var product = _iProductService.GetProduct(market.ProductId);
            gid = market.ProductId;
            //product.MarketPrice = market.MinPrice;
            //product.SaleCounts = market.SaleCount;

            var brandModel = ServiceHelper.Create<IBrandService>().GetBrand(product.BrandId);
            product.BrandName = brandModel == null ? "" : brandModel.Name;

            model.Product = product;
            model.ProductDescription = product.ProductDescriptionInfo.Description;
            if (product.ProductDescriptionInfo.DescriptionPrefixId != 0)
            {
                var desc = _iProductDescriptionTemplateService
                    .GetTemplate(product.ProductDescriptionInfo.DescriptionPrefixId, product.ShopId);
                model.DescriptionPrefix = desc == null ? "" : desc.Content;
            }

            if (product.ProductDescriptionInfo.DescriptiondSuffixId != 0)
            {
                var desc = _iProductDescriptionTemplateService
                    .GetTemplate(product.ProductDescriptionInfo.DescriptiondSuffixId, product.ShopId);
                model.DescriptiondSuffix = desc == null ? "" : desc.Content;
            }

            #endregion

            #region 店铺

            var categories = _iShopCategoryService.GetShopCategory(product.ShopId);
            List<ShopCategoryInfo> allcate = categories.ToList();
            foreach (var main in allcate.Where(s => s.ParentCategoryId == 0))
            {
                var topC = new CategoryJsonModel()
                {
                    Name = main.Name,
                    Id = main.Id.ToString(),
                    SubCategory = new List<SecondLevelCategory>()
                };
                foreach (var secondItem in allcate.Where(s => s.ParentCategoryId == main.Id))
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
            model.CashDeposits = _iCashDepositsService.GetCashDepositsObligation(product.Id);

            #endregion

            #region 热门销售

            var sale = _iProductService.GetHotSaleProduct(shop.Id, 5);
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

            var hot = _iProductService.GetHotConcernedProduct(shop.Id, 5);
            if (hot != null)
            {
                foreach (var item in hot.ToArray())
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

            #region 商品规格

            ProductTypeInfo typeInfo = _iTypeService.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            model.ColorAlias = colorAlias;
            model.SizeAlias = sizeAlias;
            model.VersionAlias = versionAlias;
            if (product.SKUInfo != null && product.SKUInfo.Count() > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0)
                    {
                        if (long.TryParse(specs[1], out colorId)) { }
                        if (colorId != 0)
                        {
                            if (!model.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = product.SKUInfo.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                model.Color.Add(new ProductSKU
                                {
                                    //Name = "选择颜色",
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1)
                    {
                        if (long.TryParse(specs[2], out sizeId)) { }
                        if (sizeId != 0)
                        {
                            if (!model.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                var ss = product.SKUInfo.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                model.Size.Add(new ProductSKU
                                {
                                    //Name = "选择尺码",
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

                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0)
                        {
                            if (!model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = product.SKUInfo.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.Stock);
                                model.Version.Add(new ProductSKU
                                {
                                    //Name = "选择版本",
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
                decimal min = 0, max = 0;
                if (product.SKUInfo.FirstOrDefault(s => s.Stock >= 0) != null)
                    min = product.SKUInfo.Where(s => s.Stock >= 0).Min(s => s.SalePrice);
                if (product.SKUInfo.FirstOrDefault(s => s.Stock >= 0) != null)
                    max = product.SKUInfo.Where(s => s.Stock >= 0).Max(s => s.SalePrice);
                if (min == 0 && max == 0)
                {
                    price = product.MinSalePrice.ToString("f2");
                }
                else if (max > min)
                {
                    price = string.Format("{0}-{1}", min.ToString("f2"), max.ToString("f2"));
                }
                else
                {
                    price = string.Format("{0}", min.ToString("f2"));
                }

            }
            model.Price = string.IsNullOrWhiteSpace(price) ? product.MinSalePrice.ToString("f2") : price;
            #endregion

            #region 商品属性
            List<TypeAttributesModel> ProductAttrs = new List<TypeAttributesModel>();
            var prodAttrs = _iProductService.GetProductAttribute(product.Id).ToList();
            foreach (var attr in prodAttrs)
            {
                if (!ProductAttrs.Any(p => p.AttrId == attr.AttributeId))
                {
                    TypeAttributesModel attrModel = new TypeAttributesModel()
                    {
                        AttrId = attr.AttributeId,
                        AttrValues = new List<TypeAttrValue>(),
                        Name = attr.AttributesInfo.Name
                    };
                    foreach (var attrV in attr.AttributesInfo.AttributeValueInfo)
                    {
                        if (prodAttrs.Any(p => p.ValueId == attrV.Id))
                        {
                            attrModel.AttrValues.Add(new TypeAttrValue
                            {
                                Id = attrV.Id.ToString(),
                                Name = attrV.Value
                            });
                        }
                    }
                    ProductAttrs.Add(attrModel);
                }
                else
                {
                    var attrTemp = ProductAttrs.FirstOrDefault(p => p.AttrId == attr.AttributeId);

                    if (!attrTemp.AttrValues.Any(p => p.Id == attr.ValueId.ToString()))
                    {
                        attrTemp.AttrValues.Add(new TypeAttrValue
                        {
                            Id = attr.ValueId.ToString(),
                            Name = attr.AttributesInfo.AttributeValueInfo.FirstOrDefault(a => a.Id == attr.ValueId).Value
                        });
                    }
                }
            }
            model.ProductAttrs = ProductAttrs;
            #endregion

            #region 获取评论、咨询数量
            //var comments = _iCommentService.GetComments( new CommentQuery
            //{
            //    ProductID = product.Id ,
            //    PageNo = 1 ,
            //    PageSize = 10000
            //} );
            //model.CommentCount = comments.Total;

            var com = product.Himall_ProductComments.Where(item => !item.IsHidden.HasValue || item.IsHidden.Value == false);
            var comCount = com.Count();
            model.CommentCount = comCount;

            var consultations = _iConsultationService.GetConsultations(gid);
            model.Consultations = consultations.Count();

            #endregion

            #region 累加浏览次数、 加入历史记录
            if (CurrentUser != null)
            {
                BrowseHistrory.AddBrowsingProduct(product.Id, CurrentUser.Id);
            }
            else
            {
                BrowseHistrory.AddBrowsingProduct(product.Id);
            }

            //_iProductService.LogProductVisti(gid);
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            #endregion

            #region 红包
            var bonus = ServiceHelper.Create<IShopBonusService>().GetByShopId(product.ShopId);
            if (bonus != null)
            {
                model.GrantPrice = bonus.GrantPrice;
            }
            else
            {
                model.GrantPrice = 0;
            }
            #endregion

            #region 获取店铺的评价统计

            var shopStatisticOrderComments = _iShopService.GetShopStatisticOrderComments(shop.Id);

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

            #region 客服
            model.Service = ServiceHelper.Create<ICustomerService>().GetCustomerService(shop.Id).Where(c => c.Type == CustomerServiceInfo.ServiceType.PreSale && c.TerminalType== CustomerServiceInfo.ServiceTerminalType.PC).OrderBy(m => m.Tool);
            #endregion

            #region 开团提醒场景二维码
             var siteSetting = _iSiteSettingService.GetSiteSettings();
            if (DateTime.Parse(model.FlashSale.BeginDate) > DateTime.Now && WXIsConfig(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret))
            {
                var token = AccessTokenContainer.TryGetToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
                SceneHelper helper = new SceneHelper();
                Himall.Model.SceneModel scene = new SceneModel(QR_SCENE_Type.FlashSaleRemind, model.FlashSale.Id);
                int sceneId = helper.SetModel(scene);
                var ticket = Senparc.Weixin.MP.AdvancedAPIs.QrCode.QrCodeApi.Create(token, 86400, sceneId).ticket;
                ViewBag.ticket = ticket;
            }
            #endregion

            model.Logined = (null != CurrentUser) ? 1 : 0;
            model.EnabledBuy = product.AuditStatus == ProductInfo.ProductAuditStatus.Audited && DateTime.Parse(market.BeginDate) <= DateTime.Now && DateTime.Parse(market.EndDate) > DateTime.Now && product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale;
            if (market.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && DateTime.Parse(market.BeginDate) < DateTime.Now && DateTime.Parse(market.EndDate) > DateTime.Now)
            {
                TimeSpan end = new TimeSpan(DateTime.Parse(market.EndDate).Ticks);
                TimeSpan start = new TimeSpan(DateTime.Now.Ticks);
                TimeSpan ts = end.Subtract(start);
                model.Second = ts.TotalSeconds < 0 ? 0 : ts.TotalSeconds;
            }
            else if (market.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && DateTime.Parse(market.BeginDate) > DateTime.Now)
            {
                TimeSpan end = new TimeSpan(DateTime.Parse(market.BeginDate).Ticks);
                TimeSpan start = new TimeSpan(DateTime.Now.Ticks);
                TimeSpan ts = end.Subtract(start);
                model.Second = ts.TotalSeconds < 0 ? 0 : ts.TotalSeconds;
            }

            //补充当前店铺红包功能
            ViewBag.isShopPage = true;
            ViewBag.CurShopId = product.ShopId;
            TempData["isShopPage"] = true;
            TempData["CurShopId"] = product.ShopId;

            return View(model);
        }

        [HttpPost]
        public ActionResult GetSkus(long id)
        {
            var model = _iLimitTimeBuyService.Get(id);
            if (model != null)
            {
                return Json(model);
            }
            return Json(null);
        }

        [HttpPost]
        public JsonResult CheckLimitTimeBuy(string skuIds, string counts)
        {
            var skuIdsArr = skuIds.Split(',');
            var pCountsArr = counts.TrimEnd(',').Split(',').Select(t => int.Parse(t));
            var productService = _iProductService;
            int index = 0;
            var products = skuIdsArr.Select(item =>
            {
                var sku = productService.GetSku(item);
                var count = pCountsArr.ElementAt(index++);
                return new CartItemModel()
                {
                    id = sku.ProductInfo.Id,
                    count = count
                };
            }).ToList().FirstOrDefault();

            int exist = _iLimitTimeBuyService.GetMarketSaleCountForUserId(products.id, CurrentUser.Id);
            int MaxSaleCount = 0;
            var model = _iLimitTimeBuyService.GetLimitTimeMarketItemByProductId(products.id);
            if (model != null)
            {
                MaxSaleCount = model.LimitCountOfThePeople;
            }
            return Json(new { success = MaxSaleCount >= exist + products.count, maxSaleCount = MaxSaleCount, remain = MaxSaleCount - exist });
        }
    }
}