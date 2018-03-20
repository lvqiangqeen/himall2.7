using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;
using System.IO;
using Senparc.Weixin.MP.Helpers;
using Himall.Core;
using Senparc.Weixin.MP.CommonAPIs;
using Himall.Web.App_Code.Common;
using Himall.Web.Areas.Mobile.Models;
using Himall.Web.Areas.Web;
using Himall.Web.Areas.Web.Helper;
using Himall.Web.Areas.Web.Models;
using Himall.Application;
using Himall.CommonModel;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class DistributionMarketController : BaseMobileMemberController
    {
        private SiteSettingsInfo _siteSetting = null;
        private IDistributionService _iDistributionService;
        private ISiteSettingService _iSiteSettingService;
        private IShopService _iShopService;
        private ICategoryService _iCategoryService;
        private IProductService _iProductService;
        private long curUserId;
        private ISlideAdsService _iSlideAdsService;
        private IMemberService _iMemberService;
        private ITypeService _iTypeService;

        /// <summary>
        /// 历史搜索词
        /// </summary>
        private string MobileHistorySearchKeyName = "Himall-Mobhiskey";
        /// <summary>
        /// 保存词条数
        /// </summary>
        private int SaveKeyNumber = 20;

        public DistributionMarketController(
            IDistributionService iDistributionService,
            IShopService iShopService,
            ISlideAdsService iSlideAdsService
            , ICategoryService iCategoryService,
            ISiteSettingService iSiteSettingService,
            IProductService iProductService,
            IMemberService iMemberService, 
            ITypeService iTypeService
            )
        {
            _iDistributionService = iDistributionService;
            _iShopService = iShopService;
            _iCategoryService = iCategoryService;
            _iSiteSettingService = iSiteSettingService;
            _iProductService = iProductService;
            this._siteSetting = _iSiteSettingService.GetSiteSettings();
            _iSlideAdsService = iSlideAdsService;
            _iMemberService = iMemberService;
            _iTypeService = iTypeService;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (CurrentUser != null)
            {
                curUserId = CurrentUser.Id;
            }
        }

        /// <summary>
        /// 检测销售员信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public void CheckPromoter()
        {
            var _curuser = _iMemberService.GetMember(CurrentUser.Id);
            PromoterInfo info = _iDistributionService.GetPromoterByUserId(CurrentUser.Id);
            string result = "";
            if (info == null)
            {
                Response.Clear();
                Response.BufferOutput = true;
                result = @Url.Action("Apply", "Distribution");
                Response.Redirect(result);
                Response.End();
            }
            switch (info.Status)
            {
                case PromoterInfo.PromoterStatus.UnAudit:
                    result = @Url.Action("Apply", "Distribution");
                    break;
                case PromoterInfo.PromoterStatus.Refused:
                    result = @Url.Action("Apply", "Distribution");
                    break;
                case PromoterInfo.PromoterStatus.NotAvailable:
                    if (RouteData.Values["action"].ToString().ToLower() != "performance")
                    {
                        result = @Url.Action("Performance", "Distribution");
                    }
                    break;
            }
            if (!string.IsNullOrWhiteSpace(result))
            {
                Response.Clear();
                Response.BufferOutput = true;
                Response.Redirect(result);
                Response.End();
            }
        }

        [HttpPost]
        public ActionResult UpdateShareNum(long productId)
        {
            _iDistributionService.UpdateProductShareNum(productId);
            return Json(new Result() { success = true });
        }

        #region 分销市场商品
        public ActionResult Index()
        {

            //轮播图
            var slideImageSettings = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.DistributionHome).ToList();
            ViewBag.slideImage = slideImageSettings;

            CheckPromoter();
            //首页推荐商品
            var models = DistributionApplication.GetDistributionProducts(CurrentUser.Id);
            foreach(var product in models)
            {
                product.Image = HimallIO.GetRomoteProductSizeImage(product.Image, 1, (int)Himall.CommonModel.ImageSize.Size_100);
            }
            #region 二维码
            var curhttp = System.Web.HttpContext.Current;
            string url = curhttp.Request.Url.Scheme + "://" + curhttp.Request.Url.Authority; ;
            url = url + Url.Action("Index", "DistributionMarket");

            ViewBag.ShopQCodeUrl = url;

            var map = Core.Helper.QRCodeHelper.Create(url);
            string fileName = "/temp/" + curUserId + DateTime.Now.ToString("yyMMddHHmmssffffff") + ".jpg";
            map.Save(Server.MapPath(fileName));
            map.Dispose();

            ViewBag.ShopQCode = fileName;
            ViewBag.WeiXinDisIndex = false;
            if (!string.IsNullOrWhiteSpace(this._siteSetting.WeixinAppId) && !string.IsNullOrWhiteSpace(this._siteSetting.WeixinAppSecret) && (PlatformType == PlatformType.WeiXin))
            {
                ViewBag.WeiXinDisIndex = true;
            }
            #endregion
            return View(models);
        }

        /// <summary>
        /// 商品分类
        /// </summary>
        /// <returns></returns>
        public ActionResult DistributionMarketCategory()
        {
            var model = CategoryApplication.GetSubCategories();
            return View(model);
        }

        /// <summary>
        /// 热门商铺
        /// </summary>
        /// <returns></returns>
        public ActionResult DistributionMarketHotProduct(string skey, int? categoryId = null)
        {
            CheckPromoter();
            ViewBag.SearchKey = skey;
            if (categoryId.HasValue)
                ViewBag.categoryId = categoryId;
            else
                ViewBag.categoryId = 0;
            //List<CategoryInfo> model = _iCategoryService.GetCategoryByParentId(0).ToList();
            return View();
        }

        /// <summary>
        /// 热门商铺
        /// </summary>
        /// <returns></returns>
        public ActionResult DistributionMarketHotShop()
        {
            ViewBag.SearchKey = "";
            return View();
        }
        public ActionResult SearchProduct(string skey, int? categoryId = null)
        {
            CheckPromoter();
            ViewBag.SearchKey = skey;
            if (categoryId.HasValue)
                ViewBag.categoryId = categoryId;
            else
                ViewBag.categoryId = 0;
            List<CategoryInfo> model = _iCategoryService.GetCategoryByParentId(0).ToList();
            return View(model);
        }
        /// <summary>
        /// 获取分销商品列表
        /// </summary>
        /// <param name="skey"></param>
        /// <param name="page"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ProductList(string skey, int page, long categoryId = 0, long shopId = 0, int sort = 0)
        {
            //查询条件
            ProductBrokerageQuery query = new ProductBrokerageQuery();
            query.skey = skey;
            query.PageSize = 5;
            query.PageNo = page;
            query.ProductBrokerageState = ProductBrokerageInfo.ProductBrokerageStatus.Normal;
            query.OnlyShowNormal = true;
            if (categoryId != 0)
            {
                query.CategoryId = categoryId;
            }
            if (shopId != 0)
            {
                query.ShopId = shopId;
            }

            query.Sort = ProductBrokerageQuery.EnumProductSort.AgentNum;
            switch (sort)
            {
                case 1:
                    query.Sort = ProductBrokerageQuery.EnumProductSort.SalesNumber;
                    break;
                case 3:
                    query.Sort = ProductBrokerageQuery.EnumProductSort.Brokerage;
                    break;
                case 4:
                    query.Sort = ProductBrokerageQuery.EnumProductSort.PriceAsc;
                    break;
                case 5:
                    query.Sort = ProductBrokerageQuery.EnumProductSort.PriceDesc;
                    break;
                case 2:
                    query.Sort = ProductBrokerageQuery.EnumProductSort.AgentNum;
                    break;
                case 6:
                    query.Sort = ProductBrokerageQuery.EnumProductSort.MonthDesc;
                    break;
                case 7:
                    query.Sort = ProductBrokerageQuery.EnumProductSort.WeekDesc;
                    break;
            }

            ObsoletePageModel<ProductBrokerageInfo> datasql = _iDistributionService.GetDistributionProducts(query);

            List<ProductBrokerageInfo> datalist = new List<ProductBrokerageInfo>();
            List<DistributionProductListModel> result = new List<DistributionProductListModel>();
            if (datasql.Models != null)
            {
                datalist = datasql.Models.ToList();
                List<long> proids = datalist.Select(d => d.ProductId.Value).ToList();
                List<long> canAgentIds = _iDistributionService.GetCanAgentProductId(proids, curUserId).ToList();
                result = datalist.Select(d => new DistributionProductListModel
                {
                    ShopId = d.ShopId,
                    ProductId = d.ProductId,
                    ProductName = d.Product.ProductName,
                    ShortDescription=d.Product.ShortDescription,
                    Image = d.Product.GetImage(ImageSize.Size_350),
                    CategoryId = d.Product.CategoryId,
                    CategoryName = d.CategoryName,
                    DistributorRate = d.rate,
                    ProductBrokerageState = d.Status,
                    ProductSaleState = d.Product.SaleStatus,
                    SellPrice = d.Product.MinSalePrice,
                    ShowProductBrokerageState = d.Status.ToDescription(),
                    ShowProductSaleState = d.Product.SaleStatus.ToDescription(),
                    SaleNum = d.SaleNum,
                    AgentNum = d.AgentNum,
                    ForwardNum = d.ForwardNum,
                    isHasAgent = (!canAgentIds.Contains(d.ProductId.Value))
                }).ToList();
            }
            return Json(result);
        }
        /// <summary>
        /// 代理商品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AgentProduct(long id)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            List<long> proids = new List<long>();
            proids.Add(id);
            _iDistributionService.AddAgentProducts(proids, curUserId);
            result = new Result { success = true, msg = "代理商品成功" };
            return Json(result);
        }
        #endregion

        #region 商品详情

        public ActionResult ProductDetail(string id = "")
        {
            CheckPromoter();

            var shopService = ServiceHelper.Create<IVShopService>();
            string price = "";

            ProductDetailModel detailModel = new ProductDetailModel();
            DistributionProductDetailShowModel model = new DistributionProductDetailShowModel()
            {
                Product = new Model.ProductInfo(),
                Shop = new ShopInfoModel(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };

            ProductInfo product = null;
            ProductBrokerageInfo probroker = null;
            ShopInfo shop = null;
            long gid = 0;

            #region 商品Id不合法
            if (long.TryParse(id, out gid)) { }
            if (gid == 0)
            {
                //跳转到出错页面
                return RedirectToAction("Error404", "Error", new { area = "Web" });
            }
            #endregion



            #region 初始化商品和店铺
            probroker = _iDistributionService.GetDistributionProductInfo(gid);
            product = _iProductService.GetProduct(gid);

            if (probroker == null)
            {
                return RedirectToAction("Error404", "Error", new { area = "Web" });
            }

            if (product == null)
            {
                //跳转到404页面
                return RedirectToAction("Error404", "Error", new { area = "Web" });
            }
            model.ProductDescription = product.ProductDescriptionInfo.ShowMobileDescription;

            model.DistributionAgentNum = probroker.AgentNum.Value;
            model.DistributionSaleNum = probroker.SaleNum.Value;
            model.DistributionCommission = 0;
            model.ShopDistributionProductNum = _iDistributionService.GetShopDistributionProductCount(product.ShopId);
            decimal rate = probroker.rate;
            if (rate > 0)
            {
                model.DistributionCommission = (product.MinSalePrice * rate / 100);
                int _tmp = (int)(model.DistributionCommission * 100);
                //保留两位小数，但不四舍五入
                model.DistributionCommission = (decimal)(((decimal)_tmp) / (decimal)100);
            }

            //是否已代理
            List<long> proids = new List<long>();
            proids.Add(gid);
            List<long> canAgentIds = _iDistributionService.GetCanAgentProductId(proids, curUserId).ToList();
            model.DistributionIsAgent = true;
            if (canAgentIds.Count > 0)
            {
                model.DistributionIsAgent = false;
            }

            shop = ServiceHelper.Create<IShopService>().GetShop(product.ShopId);
            var mark = ShopServiceMark.GetShopComprehensiveMark(shop.Id);
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;
            var comm = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(gid);
            model.Shop.Name = shop.ShopName;
            model.Shop.ProductMark = (comm == null || comm.Count() == 0) ? 0 : comm.Average(p => (decimal)p.ReviewMark);
            model.Shop.Id = product.ShopId;
            model.Shop.FreeFreight = shop.FreeFreight;
            detailModel.ProductNum = ServiceHelper.Create<IProductService>().GetShopOnsaleProducts(product.ShopId);
            bool isFavorite;
            if (CurrentUser == null)
                isFavorite = false;
            else
                isFavorite = ServiceHelper.Create<IProductService>().IsFavorite(product.Id, CurrentUser.Id);
            detailModel.IsFavorite = isFavorite;
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
                                    Value = sku.Color,
                                    Img = sku.ShowPic
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
                min = product.SKUInfo.Where(s => s.Stock >= 0).Min(s => s.SalePrice);
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
            ViewBag.Price = string.IsNullOrWhiteSpace(price) ? product.MinSalePrice.ToString("f2") : price;
            #endregion

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = ServiceHelper.Create<IShopService>().GetShopStatisticOrderComments(product.ShopId);

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
                detailModel.ProductAndDescription = productAndDescription.CommentValue;
                detailModel.ProductAndDescriptionPeer = productAndDescriptionPeer.CommentValue;
                detailModel.ProductAndDescriptionMin = productAndDescriptionMin.CommentValue;
                detailModel.ProductAndDescriptionMax = productAndDescriptionMax.CommentValue;
            }
            else
            {
                detailModel.ProductAndDescription = defaultValue;
                detailModel.ProductAndDescriptionPeer = defaultValue;
                detailModel.ProductAndDescriptionMin = defaultValue;
                detailModel.ProductAndDescriptionMax = defaultValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null)
            {
                detailModel.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
                detailModel.SellerServiceAttitudePeer = sellerServiceAttitudePeer.CommentValue;
                detailModel.SellerServiceAttitudeMax = sellerServiceAttitudeMax.CommentValue;
                detailModel.SellerServiceAttitudeMin = sellerServiceAttitudeMin.CommentValue;
            }
            else
            {
                detailModel.SellerServiceAttitude = defaultValue;
                detailModel.SellerServiceAttitudePeer = defaultValue;
                detailModel.SellerServiceAttitudeMax = defaultValue;
                detailModel.SellerServiceAttitudeMin = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null)
            {
                detailModel.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
                detailModel.SellerDeliverySpeedPeer = sellerDeliverySpeedPeer.CommentValue;
                detailModel.SellerDeliverySpeedMax = sellerDeliverySpeedMax != null ? sellerDeliverySpeedMax.CommentValue : 0;
                detailModel.sellerDeliverySpeedMin = sellerDeliverySpeedMin != null ? sellerDeliverySpeedMin.CommentValue : 0;
            }
            else
            {
                detailModel.SellerDeliverySpeed = defaultValue;
                detailModel.SellerDeliverySpeedPeer = defaultValue;
                detailModel.SellerDeliverySpeedMax = defaultValue;
                detailModel.sellerDeliverySpeedMin = defaultValue;
            }
            #endregion


            model.Product = product;

            //var comments = ServiceHelper.Create<ICommentService>().GetComments(new CommentQuery
            //{
            //    ProductID = product.Id,
            //    PageNo = 1,
            //    PageSize = 10000
            //});
            //detailModel.CommentCount = comments.Total;

            var com = product.Himall_ProductComments.Where(item => !item.IsHidden.HasValue || item.IsHidden.Value == false);
            var comCount = com.Count();
            detailModel.CommentCount = comCount;

            var consultations = ServiceHelper.Create<IConsultationService>().GetConsultations(gid);

            //double total = product.Himall_ProductComments.Count();
            //double niceTotal = product.Himall_ProductComments.Count(item => item.ReviewMark >= 4);
            double total = (double)comCount;
            double niceTotal = com.Count(item => item.ReviewMark >= 4);
            detailModel.NicePercent = (int)((niceTotal / total) * 100);
            detailModel.Consultations = consultations.Count();
            long vShopId;
            if (shopService.GetVShopByShopId(shop.Id) == null)
                vShopId = -1;
            else
                vShopId = shopService.GetVShopByShopId(shop.Id).Id;
            detailModel.VShopId = vShopId;
            model.Shop.VShopId = vShopId;
            var bonus = ServiceHelper.Create<IShopBonusService>().GetByShopId(shop.Id);
            if (bonus != null)
            {
                detailModel.BonusCount = bonus.Count;
                detailModel.BonusGrantPrice = bonus.GrantPrice;
                detailModel.BonusRandomAmountStart = bonus.RandomAmountStart;
                detailModel.BonusRandomAmountEnd = bonus.RandomAmountEnd;
            }
            detailModel.CashDepositsObligation = ServiceHelper.Create<ICashDepositsService>().GetCashDepositsObligation(product.Id);
            var cashDepositModel = ServiceHelper.Create<ICashDepositsService>().GetCashDepositsObligation(product.Id);
            model.CashDepositsServer = cashDepositModel;
            model.VShopLog = shopService.GetVShopLog(model.Shop.VShopId);
            model.FavoriteShopCount = _iShopService.GetShopFavoritesCount(product.ShopId);

            ViewBag.DetailModel = detailModel;


            //分销信息

            return View(model);
        }
        #endregion

        #region 搜索相关
        /// <summary>
        /// 搜索框
        /// </summary>
        /// <param name="skey">关键词</param>
        /// <param name="stype">类型(0商品 1店铺)</param>
        /// <returns></returns>
        public ActionResult SearchBox(string skey, int stype = 0)
        {
            AddHistorySearchKey(skey, MobileHistorySearchKeyName);
            ViewBag.SearchBoxKey = skey;
            ViewBag.SearchBoxType = stype;
            ViewBag.HistorySearchKey = GetHistorySearchKey(MobileHistorySearchKeyName);
            return View();
        }
        /// <summary>
        /// 添加历史搜索词
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        private void AddHistorySearchKey(string key, string name)
        {
            List<string> datalist = GetHistorySearchKey(name);
            if (!string.IsNullOrWhiteSpace(key))
            {
                key = key.Replace(",", "");
                if (datalist.Count(d => d == key) < 1)
                {
                    datalist.Add(key);
                }
            }
            datalist = datalist.Take(SaveKeyNumber).ToList();
            if (datalist.Count > 0)
            {
                //中文转码
                for (var _n = 0; _n < datalist.Count; _n++)
                {
                    datalist[_n] = Server.UrlEncode(datalist[_n]);
                }
            }
            Core.Helper.WebHelper.SetCookie(name, string.Join(",", datalist.ToArray()));
        }

        private List<string> GetHistorySearchKey(string name)
        {
            List<string> result = new List<string>();
            string val = Core.Helper.WebHelper.GetCookie(name);
            string[] arrkey = val.Split(',');
            foreach (var item in arrkey)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    string key = Server.UrlDecode(item);
                    if (result.Count(d => d == key) < 1)
                    {
                        result.Add(key);
                    }
                }
            }
            result = result.Take(SaveKeyNumber).ToList();
            return result;
        }

        private void ClearHistorySearchKey(string name)
        {
            Core.Helper.WebHelper.SetCookie(name, "");
        }
        #endregion

        #region 店铺有关
        public ActionResult SearchShops(string skey)
        {
            CheckPromoter();
            ViewBag.SearchKey = skey;
            return View();
        }
        /// <summary>
        /// 店铺详情
        /// <para>销售员查看</para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ShopDetail(long id)
        {
            var shop = _iShopService.GetShop(id);
            if (shop == null)
            {
                throw new HimallException("错误的店铺编号");
            }
            CheckPromoter();
            var shareInfo = _iDistributionService.getShopDistributorSettingInfo(id);

            DistributionShopShowModel model = new DistributionShopShowModel();
            if (shareInfo != null)
            {
                model.ShopShareLogo = string.IsNullOrWhiteSpace(shareInfo.DistributorShareLogo) ? "" : Core.HimallIO.GetRomoteImagePath(shareInfo.DistributorShareLogo);
                model.ShopShareTitle = shareInfo.DistributorShareName;
                model.ShopShareDesc = shareInfo.DistributorShareContent;
            }
            model.ShopName = shop.ShopName;
            model.ShopId = id;
            model.UserId = curUserId;
            if (shop.Himall_VShop.Any())
            {
                model.ShopLogo = Core.HimallIO.GetRomoteImagePath(shop.Himall_VShop.FirstOrDefault().BackgroundImage);
            }
            if (CurrentUser != null)
            {
                model.isFavorite = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Any(d => d.ShopId == id);
            }
            return View(model);
        }

        public JsonResult AddFavorite(long shopId)
        {
            _iShopService.AddFavoriteShop(CurrentUser.Id, shopId);
            return Json(new Result { success = true, msg = "成功关注该微店." });
        }

        public JsonResult DeleteFavorite(long shopId)
        {
            _iShopService.CancelConcernShops(shopId, CurrentUser.Id);
            return Json(new Result { success = true, msg = "成功取消关注该微店." });
        }

        /// <summary>
        /// 获取分销店铺列表
        /// </summary>
        /// <param name="skey"></param>
        /// <param name="page"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ShopList(string skey, int page, long sort = 0)
        {
            //查询条件
            DistributionShopQuery query = new DistributionShopQuery();
            query.skey = skey;
            query.PageSize = 5;
            query.PageNo = page;
            query.Sort = DistributionShopQuery.EnumShopSort.Default;
            switch (sort)
            {
                case 2:
                    query.Sort = DistributionShopQuery.EnumShopSort.ProductNum;
                    break;
            }
            ObsoletePageModel<DistributionShopModel> datasql = _iDistributionService.GetShopDistributionList(query);

            List<DistributionShopModel> datalist = datasql.Models.ToList();
            return Json(datalist);
        }
        #endregion

        /// <summary>
        /// 获取平台分销模块设置
        /// </summary>
        /// <returns></returns>
        public ActionResult Setting()
        {
            var m = _iDistributionService.GetDistributionSetting();
            if (m != null && !string.IsNullOrWhiteSpace(m.DisBanner))
            {
                m.DisBanner = Core.HimallIO.GetRomoteImagePath(m.DisBanner);
            }
            return Json(new { success = true, data = m });
        }
    }
}