using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Web;
using Himall.Web.Areas.Web.Helper;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Areas.Mobile.Models;
using Himall.Application;
using Himall.Web.App_Code.Common;
using Himall.Core;
using Himall.CommonModel;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class ProductController : BaseMobileTemplatesController
    {
        private IShopService _iShopService;
        private IVShopService _iVShopService;
        private IProductService _iProductService;
        private ICashDepositsService _iCashDepositsService;
        private IFreightTemplateService _iFreightTemplateService;
        private IRegionService _iRegionService;
        private IDistributionService _iDistributionService;
        private IMessageService _iMessageService;
        private ITypeService _iTypeService;
        private const string SMSPLUGIN = "Himall.Plugin.Message.SMS";
        public ProductController(IShopService iShopService, IVShopService iVShopService, IProductService iProductService,
            ICashDepositsService iCashDepositsService, IFreightTemplateService iFreightTemplateService, IRegionService iRegionService
            , IDistributionService iDistributionService, ITypeService iTypeService, IMessageService iMessageService
            )
        {
            _iShopService = iShopService;
            _iVShopService = iVShopService;
            _iProductService = iProductService;
            _iCashDepositsService = iCashDepositsService;
            _iFreightTemplateService = iFreightTemplateService;
            _iRegionService = iRegionService;
            _iDistributionService = iDistributionService;
            _iTypeService = iTypeService;
            _iMessageService = iMessageService;
        }

        public JsonResult GetNeedRefreshProductInfo(long id = 0, long shopId = 0)
        {
            var productservice = _iProductService;
            var productId = id;
            if (productId == 0)
                return Json(new { data = false });
            var productInfo = productservice.GetNeedRefreshProductInfo(productId);
            if (productInfo == null)
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            }
            long vShopId;
            var vshopinfo = _iVShopService.GetVShopByShopId(productInfo.ShopId);
            if (vshopinfo == null)
                vShopId = -1;
            else
                vShopId = vshopinfo.Id;
            var skus = productservice.GetSKUs(productId);
            decimal min = 0, max = 0;
            var strPrice = string.Empty;
            var skusql = skus.Where(s => s.Stock >= 0);
            if (skusql.Count() > 0)
            {
                min = skusql.Min(s => s.SalePrice);
                max = skusql.Max(s => s.SalePrice);
                if (min == 0 && max == 0)
                {
                    strPrice = productInfo.MinSalePrice.ToString("f2");
                }
            }
            else if (max > min)
            {
                strPrice = string.Format("{0}-{1}", min.ToString("f2"), max.ToString("f2"));
            }
            else
            {
                strPrice = string.Format("{0}", min.ToString("f2"));
            }

            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var shopInfo = ShopApplication.GetShop(productInfo.ShopId);
            if (shopInfo.IsSelf)
            {
                strPrice = string.IsNullOrWhiteSpace(strPrice) ? (productInfo.MinSalePrice * discount).ToString("f2") : (Convert.ToDecimal(strPrice) * discount).ToString("f2");
            }
            else
            {
                strPrice = string.IsNullOrWhiteSpace(strPrice) ? productInfo.MinSalePrice.ToString("f2") : strPrice;

            }
            bool isFavorite;
            bool isFavoriteShop = false;
            if (CurrentUser == null)
            {
                isFavorite = false;
            }
            else
            {
                isFavorite = _iProductService.IsFavorite(productId, CurrentUser.Id);
                var favoriteShopIds = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();//获取已关注店铺
                isFavoriteShop = favoriteShopIds.Contains(shopId);
            }

            return Json(new
            {
                price = strPrice,
                saleStatus = (int)productInfo.SaleStatus,
                auditStatus = (int)productInfo.AuditStatus,
                isFavorite = isFavorite,
                isFavoriteShop = isFavoriteShop,
                freightTemplateId = productInfo.FreightTemplateId,
                vShopId = vShopId,
                stock = skus.Sum(a => a.Stock)
            });
        }

        /// <summary>
        /// 商品详情
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partnerid">合作者</param>
        /// <param name="nojumpfg">不跳转拼团</param>
        /// <returns></returns>
        public ActionResult Detail(string id = "", long partnerid = 0, int nojumpfg = 0)
        {
            var productservice = _iProductService;
            long gid = 0;
            #region 商品Id不合法
            if (long.TryParse(id, out gid))
            {
            }
            if (gid == 0)
            {
                //throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
                //跳转到出错页面
                return RedirectToAction("Error404", "Error", new
                {
                    area = "Web"
                });
            }
            if (!productservice.CheckProductIsExist(gid))
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            }
            #endregion
            var product = productservice.GetProduct(gid);
            if (product == null)
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            }
            #region 销售员
            if (partnerid > 0)
            {
                long curuserid = 0;
                if (CurrentUser != null)
                {
                    curuserid = CurrentUser.Id;
                }

                SaveDistributionUserLinkId(partnerid, product.ShopId, curuserid);
            }
            #endregion

            #region 拼团活动跳转
            if (nojumpfg != 1)
            {
                var fgactobj = FightGroupApplication.GetActiveByProductId(gid);
                if (fgactobj != null)
                {
                    if (fgactobj.ActiveStatus == CommonModel.FightGroupActiveStatus.Ongoing)  //只跳转进行中的活动
                    {
                        return RedirectToAction("Detail", "FightGroup", new { id = fgactobj.Id });
                    }
                }
            }
            #endregion

            //限时购商品跳转
            var ltmbuy = ServiceHelper.Create<ILimitTimeBuyService>().GetLimitTimeMarketItemByProductId(gid);
            if (ltmbuy != null)
            {
                return RedirectToAction("Detail", "LimitTimeBuy", new { id = ltmbuy.Id });
            }

            #region 限时购预热
            var iLimitService = ServiceHelper.Create<ILimitTimeBuyService>();
            var FlashSale = iLimitService.IsFlashSaleDoesNotStarted(gid);
            var FlashSaleConfig = iLimitService.GetConfig();

            if (FlashSale != null)
            {
                TimeSpan flashSaleTime = DateTime.Parse(FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                TimeSpan preheatTime = new TimeSpan(FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                if (preheatTime >= flashSaleTime)  //预热大于开始
                {
                    if (!FlashSaleConfig.IsNormalPurchase)
                        return RedirectToAction("Detail", "LimitTimeBuy", new { Id = FlashSale.Id });
                }
            }
            #endregion
            ProductManagerApplication.GetWAPHtml(gid);
            string urlHtml = "/Storage/Products/Statics/" + id + "-wap.html";
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            return File(urlHtml, "text/html");
        }

        public JsonResult GetWeiXinShareArgs()
        {
            var _weiXinShareArgs = Application.WXApiApplication.GetWeiXinShareArgs(this.HttpContext.Request.Url.AbsoluteUri);
            return Json(_weiXinShareArgs, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 商品详情(用户生成html)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partnerid">合作者</param>
        /// <param name="nojumpfg">不跳转拼团</param>
        /// <returns></returns>
        public ActionResult Details(long id = 0, long partnerid = 0, int nojumpfg = 0)
        {
            var shopService = _iShopService;
            var customerService = ServiceHelper.Create<ICustomerService>();
            string price = "";

            ProductDetailModel detailModel = new ProductDetailModel();
            ProductDetailModelForWeb model = new ProductDetailModelForWeb()
            {
                Product = new Model.ProductInfo(),
                Shop = new ShopInfoModel(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };

            ProductInfo product = null;
            ShopInfo shop = null;
            long gid = id;

            #region 商品Id不合法          
            if (gid == 0)
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
                //跳转到出错页面
                //return RedirectToAction("Error404", "Error", new
                //{
                //    area = "Web"
                //});
            }
            product = _iProductService.GetProduct(gid);

            if (product == null)
            {
                ////跳转到404页面
                //return RedirectToAction("Error404", "Error", new
                //{
                //    area = "Web"
                //});
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            }

            if (product.IsDeleted)
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
                ////跳转到404页面
                //return RedirectToAction("Error404", "Error", new
                //{
                //    area = "Web"
                //});
            }

            #endregion

            #region 销售员
            if (partnerid > 0)
            {
                long curuserid = 0;
                if (CurrentUser != null)
                {
                    curuserid = CurrentUser.Id;
                }
                SaveDistributionUserLinkId(partnerid, product.ShopId, curuserid);
            }
            #endregion

            #region 初始化商品和店铺
            //TODO:DZY[150729] 显示移动端描述
            model.ProductDescription = product.ProductDescriptionInfo.ShowMobileDescription;

            shop = _iShopService.GetShop(product.ShopId);
            var mark = ShopServiceMark.GetShopComprehensiveMark(shop.Id);
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;
            var comm = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(gid);
            model.Shop.Name = shop.ShopName;
            model.Shop.ProductMark = (comm == null || comm.Count() == 0) ? 0 : comm.Average(p => (decimal)p.ReviewMark);
            model.Shop.Id = product.ShopId;
            model.Shop.FreeFreight = shop.FreeFreight;
            detailModel.ProductNum = _iProductService.GetShopOnsaleProducts(product.ShopId);
            bool isFavorite;
            detailModel.FavoriteShopCount = _iShopService.GetShopFavoritesCount(product.ShopId);
            if (CurrentUser == null)
            {
                isFavorite = false;
                detailModel.IsFavoriteShop = false;
            }
            else
            {
                isFavorite = _iProductService.IsFavorite(product.Id, CurrentUser.Id);
                var favoriteShopIds = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();//获取已关注店铺
                detailModel.IsFavoriteShop = favoriteShopIds.Contains(product.ShopId);
            }
            detailModel.IsFavorite = isFavorite;
            #endregion

            #region 商品规格

            var typeservice = _iTypeService;
            ProductTypeInfo typeInfo = typeservice.GetType(product.TypeId);
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
                                    Img = Core.HimallIO.GetImagePath(sku.ShowPic)
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
                                    //Name = "选择规格",
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
            var shopStatisticOrderComments = _iShopService.GetShopStatisticOrderComments(product.ShopId);

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
            if (productAndDescription != null && productAndDescriptionPeer != null && !shop.IsSelf)
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
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null && !shop.IsSelf)
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
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null && !shop.IsSelf)
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

            #region 限时购预热
            var iLimitService = ServiceHelper.Create<ILimitTimeBuyService>();
            bool isPreheat = false;
            model.FlashSale = iLimitService.IsFlashSaleDoesNotStarted(gid);
            model.FlashSaleConfig = iLimitService.GetConfig();

            if (model.FlashSale != null)
            {
                TimeSpan flashSaleTime = DateTime.Parse(model.FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                TimeSpan preheatTime = new TimeSpan(model.FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                if (preheatTime >= flashSaleTime)  //预热大于开始
                {
                    isPreheat = true;
                    if (!model.FlashSaleConfig.IsNormalPurchase)
                        return RedirectToAction("Detail", "LimitTimeBuy", new { Id = model.FlashSale.Id });
                }
            }
            ViewBag.IsPreheat = isPreheat;
            #endregion

            model.Product = product;

            model.Favorites = product.Himall_Favorites.Count();
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
            var vshopinfo = _iVShopService.GetVShopByShopId(shop.Id);
            if (vshopinfo == null)
                vShopId = -1;
            else
                vShopId = vshopinfo.Id;
            detailModel.VShopId = vShopId;
            model.Shop.VShopId = vShopId;

            model.VShopLog = _iVShopService.GetVShopLog(model.Shop.VShopId);
            if (string.IsNullOrWhiteSpace(model.VShopLog))
            {
                //throw new Himall.Core.HimallException("店铺未开通微店功能");
                model.VShopLog = CurrentSiteSetting.WXLogo;
            }

            var customerServices = CustomerServiceApplication.GetMobileCustomerService(product.ShopId);
            var meiqia = CustomerServiceApplication.GetPreSaleByShopId(product.ShopId).FirstOrDefault(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            if (meiqia != null)
                customerServices.Insert(0, meiqia);
            ViewBag.CustomerServices = customerServices;

            ViewBag.DetailModel = detailModel;
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            return View(model);
        }
        public JsonResult GetProductColloCation(long productId)
        {
            var collocation = ServiceHelper.Create<ICollocationService>().GetCollocationListByProductId(productId);
            int result = 0;
            if (collocation != null)
            {
                foreach (var item in collocation)
                {
                    var pcount = item.Himall_Collocation.Himall_CollocationPoruducts
                                .Where(a => a.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && a.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited).Count();
                    var mainProduct = item.Himall_Collocation.Himall_CollocationPoruducts.FirstOrDefault(p => p.IsMain == true
                                && p.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                                && p.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                                && p.Himall_Products.IsDeleted == false);
                    if (mainProduct != null && pcount > 1)
                    {
                        result++;
                    }
                }
            }
            return Json(new { count = result });
        }
        public ActionResult ProductColloCation(long productId)
        {
            var collocation = ServiceHelper.Create<ICollocationService>().GetCollocationListByProductId(productId);
            List<CollocationProducts> products = null;
            List<ProductCollocationModel> data = new List<ProductCollocationModel>();
            if (collocation != null && collocation.Count > 0)
            {
                int i = 0;
                foreach (var item in collocation)
                {
                    i++;
                    var model = new ProductCollocationModel();
                    products = item.Himall_Collocation.Himall_CollocationPoruducts
                        .Where(a => a.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                        && a.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited)
                        .Select(a =>
                        new CollocationProducts()
                        {
                            DisplaySequence = a.DisplaySequence.Value,
                            IsMain = a.IsMain,
                            Stock = a.Himall_Products.SKUInfo.Sum(t => t.Stock),
                            MaxCollPrice = a.Himall_CollocationSkus.Max(x => x.Price),
                            MaxSalePrice = a.Himall_CollocationSkus.Max(x => x.SkuPirce).GetValueOrDefault(),
                            MinCollPrice = a.Himall_CollocationSkus.Min(x => x.Price),
                            MinSalePrice = a.Himall_CollocationSkus.Min(x => x.SkuPirce).GetValueOrDefault(),
                            ProductName = a.Himall_Products.ProductName,
                            ProductId = a.ProductId,
                            ColloPid = a.Id,
                            Image = Core.HimallIO.GetImagePath(a.Himall_Products.RelativePath)
                        }).OrderBy(a => a.DisplaySequence).ToList();
                    decimal cheap = 0;
                    if (products != null && products.Count > 1)
                    {
                        cheap = products.Sum(a => a.MaxSalePrice) - products.Sum(a => a.MinCollPrice);
                    }

                    var mainProduct = item.Himall_Collocation.Himall_CollocationPoruducts.FirstOrDefault(p => p.IsMain == true);

                    model.Id = item.Id;
                    model.Name = "组合购" + CollocationApplication.GetChineseNumber(i);
                    model.ProductId = mainProduct.ProductId;
                    model.ShopId = item.Himall_Collocation.ShopId;
                    model.Products = products;
                    model.Cheap = cheap;
                    data.Add(model);
                }
            }
            return View(data);
        }

        #region 累加浏览次数、 加入历史记录
        [HttpPost]
        public JsonResult LogProduct(long pid)
        {
            if (CurrentUser != null)
            {
                BrowseHistrory.AddBrowsingProduct(pid, CurrentUser.Id);
            }
            else
            {
                BrowseHistrory.AddBrowsingProduct(pid);
            }

            //_iProductService.LogProductVisti(pid);
            return Json(null);
        }

        #endregion
        //TODO:【2015-09-22】取红包数据

        [HttpPost]
        public JsonResult AddFavoriteProduct(long pid)
        {
            int status = 0;
            _iProductService.AddFavorite(pid, CurrentUser.Id, out status);
            return Json(new Result { success = true, msg = "成功关注" });
        }

        [HttpPost]
        public JsonResult DeleteFavoriteProduct(long pid)
        {
            _iProductService.DeleteFavorite(pid, CurrentUser.Id);
            return Json(new Result { success = true, msg = "已取消关注" });
        }

        public JsonResult GetSKUInfo(long pId)
        {
            var product = _iProductService.GetProduct(pId);

            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var shopInfo = ShopApplication.GetShop(product.ShopId);

            var skuArray = new List<ProductSKUModel>();
            foreach (var sku in product.SKUInfo.Where(s => s.Stock > 0))
            {
                decimal price = 1M;
                if (shopInfo.IsSelf)
                {
                    price = sku.SalePrice * discount;
                }
                else
                {
                    price = sku.SalePrice;
                }
                skuArray.Add(new ProductSKUModel
                {
                    Price = price,
                    SkuId = sku.Id,
                    Stock = (int)sku.Stock
                });
            }
            //foreach (var item in skuArray)
            //{
            //    var str = item.SkuId.Split('_');
            //    item.SkuId = string.Format("{0};{1};{2}", str[1], str[2], str[3]);
            //}
            return Json(new { skuArray = skuArray }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetCommentByProduct(long pId)
        {
            IEnumerable<ProductCommentInfo> result;
            var comments = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(pId);
            if (comments != null && comments.Count() > 0)
            {
                result = comments.OrderByDescending(c => c.ReviewMark).ToArray();

                var temp = result.OrderByDescending(a => a.ReviewDate).Take(3).ToArray();
                var data = from c in temp
                           select new
                           {
                               UserName = c.UserName,
                               ReviewContent = c.ReviewContent,
                               ReviewDate = c.ReviewDate.ToString("yyyy-MM-dd HH:mm:ss"),
                               ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                               ReplyDate = c.ReplyDate.HasValue ? c.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
                               ReviewMark = c.ReviewMark,
                               BuyDate = ""
                           };
                return Json(new
                {
                    successful = true,
                    comments = data,
                    goodComment = comments.ToArray().Where(c => c.ReviewMark >= 4).Count(),
                    badComment = comments.ToArray().Where(c => c.ReviewMark == 1).Count(),
                    comment = comments.ToArray().Where(c => c.ReviewMark == 2 || c.ReviewMark == 3).Count(),

                }, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductComment(long pId = 0, int commentType = 0)
        {
            var comments = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(pId);
            ViewBag.goodComment = comments.Count(c => c.ReviewMark >= 4);
            ViewBag.mediumComment = comments.Count(c => c.ReviewMark == 3);
            ViewBag.bedComment = comments.Count(c => c.ReviewMark <= 2);
            ViewBag.allComment = comments.Count();
            ViewBag.hasAppend = comments.Where(c => c.AppendDate.HasValue).Count();
            ViewBag.hasImages = comments.Where(c => c.Himall_ProductCommentsImages.Count > 0).Count();
            return View();
        }

        public JsonResult GetProductComment(long pId, int pageNo, int commentType = 0, int pageSize = 10)
        {
            IEnumerable<ProductCommentInfo> result;
            var comments = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(pId);
            switch (commentType)
            {
                case 1:
                    result = comments.OrderByDescending(c => c.ReviewMark).ToArray().Where(c => c.ReviewMark >= 4);
                    break;
                case 2:
                    result = comments.OrderByDescending(c => c.ReviewMark).ToArray().Where(c => c.ReviewMark == 3);
                    break;
                case 3:
                    result = comments.OrderByDescending(c => c.ReviewMark).ToArray().Where(c => c.ReviewMark <= 2);
                    break;
                case 4:
                    result = comments.Where(c => c.Himall_ProductCommentsImages.Count > 0);
                    break;
                case 5:
                    result = comments.Where(c => c.AppendDate.HasValue);
                    break;
                default:
                    result = comments.OrderByDescending(c => c.ReviewMark).ToArray();
                    break;
            }

            var productService = _iProductService;

            ProductCommentInfo[] temp = result.OrderByDescending(a => a.ReviewDate).Skip((pageNo - 1) * pageSize).Take(pageSize).ToArray();
            var data = temp.ToArray().Select(c => new
            {

                Sku = productService.GetSkuString(c.Himall_OrderItems.SkuId),
                UserName = c.UserName,
                ReviewContent = c.ReviewContent,
                AppendContent = c.AppendContent,
                AppendDate = c.AppendDate.HasValue ? c.AppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                ReplyAppendContent = c.ReplyAppendContent,
                ReplyAppendDate = c.ReplyAppendDate.HasValue ? c.ReplyAppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                FinshDate = c.Himall_OrderItems.OrderInfo.FinishDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                Images = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 0).Select(a => new { CommentImage = Himall.Core.HimallIO.GetImagePath(a.CommentImage) }).ToList(),
                AppendImages = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 1).Select(a => new { CommentImage = Himall.Core.HimallIO.GetImagePath(a.CommentImage) }).ToList(),
                ReviewDate = c.ReviewDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                ReplyDate = c.ReplyDate.HasValue ? c.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
                ReviewMark = c.ReviewMark,
                BuyDate = c.Himall_OrderItems.OrderInfo.OrderDate.ToString("yyyy-MM-dd HH:mm:ss")

            });

            return Json(data);
        }

        private IEnumerable<CouponInfo> GetCouponList(long shopid)
        {
            var service = ServiceHelper.Create<ICouponService>();
            var result = service.GetCouponList(shopid);
            var couponSetList = ServiceHelper.Create<IVShopService>().GetVShopCouponSetting(shopid).Where(a => a.PlatForm == Core.PlatformType.Wap).Select(item => item.CouponID);
            if (result.Count() > 0 && couponSetList.Count() > 0)
            {
                var couponList = result.ToArray().Where(item => couponSetList.Contains(item.Id));//取设置的优惠卷
                return couponList;
            }
            return new List<CouponInfo>();
        }

        /// <summary>
        /// 获取店铺优惠券
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ActionResult GetShopCoupons(long shopId)
        {
            var coupons = GetCouponList(shopId);
            return null;
        }


        [UnAuthorize]
        [HttpPost]
        public ActionResult GetProductActives(long shopId, long productId)
        {
            ProductActives actives = new ProductActives();
            var freeFreight = ServiceHelper.Create<IShopService>().GetShopFreeFreight(shopId);
            actives.freeFreight = freeFreight;
            var bonus = ServiceHelper.Create<IShopBonusService>().GetByShopId(shopId);
            if (bonus != null)
            {
                ProductBonusLableModel model = new ProductBonusLableModel();
                model.Count = bonus.Count;
                model.GrantPrice = bonus.GrantPrice;
                model.RandomAmountStart = bonus.RandomAmountStart;
                model.RandomAmountEnd = bonus.RandomAmountEnd;
                actives.ProductBonus = model;
            }
            var fullDiscount = FullDiscountApplication.GetOngoingActiveByProductId(productId, shopId);
            if (fullDiscount != null)
            {
                actives.FullDiscount = fullDiscount;
            }
            return Json(actives);
        }

        public ActionResult HistoryVisite(long userId)
        {
            var product = BrowseHistrory.GetBrowsingProducts(10, userId);
            return View(product);
        }

        [HttpPost]
        public ActionResult GetStock(string skuId)
        {
            var stock = _iProductService.GetSku(skuId).Stock;
            var product = _iProductService.GetSku(skuId).ProductInfo;
            var status = 0;
            if (product.AuditStatus == ProductInfo.ProductAuditStatus.Audited && product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)
            {
                status = 1;
            }
            return Json(new
            {
                Stock = stock,
                Status = status
            });
        }

        ////
        //private string GetSelected(long productId, long skuId, string skus,int skuType)
        //{
        //    string result = "";
        //    if (skus=="")
        //    {
        //        return result;
        //    }
        //    var groupSKU = skus.Split(',');
        //    for (int i = 0; i < groupSKU.Count(); i++)
        //    {
        //        var specs = groupSKU[i].Split('_');
        //        if (specs.Count() > 0)
        //        {
        //            long colorId = 0, sizeId = 0, versionId = 0,product=0;
        //            if (long.TryParse(specs[0], out product)) { }
        //            if (long.TryParse(specs[1], out colorId)) { }
        //            if (long.TryParse(specs[2], out sizeId)) { }
        //            if (long.TryParse(specs[3], out versionId)) { }
        //            if (colorId != 0 && skuType==1)
        //            {
        //                if (product == productId && colorId == skuId)
        //                {
        //                    result = "selected";
        //                }
        //            }
        //            else if (sizeId!= 0 && skuType==2)
        //            {
        //                if (product == productId && sizeId == skuId)
        //                {
        //                    result = "selected";
        //                }
        //            }
        //            else if (versionId != 0 && skuType == 3)
        //            {
        //                if (product == productId && versionId == skuId)
        //                {
        //                    result = "selected";
        //                }
        //            }
        //        }

        //    }
        //    return result;
        //}

        public ActionResult ShowSkuInfo(long id)
        {

            ProductInfo product = _iProductService.GetProduct(id);

            if (product == null)
            {
                throw new Himall.Core.HimallException("产品编号错误");
            }

            if (product.IsDeleted)
            {
                throw new Himall.Core.HimallException("产品编号错误");
            }

            ProductShowSkuInfoModel model = new ProductShowSkuInfoModel();
            model.MinSalePrice = product.MinSalePrice;
            model.ProductImagePath = product.RelativePath;
            model.MeasureUnit = product.MeasureUnit;
            model.MaxBuyCount = product.MaxBuyCount;

            #region 商品规格

            var typeservice = _iTypeService;
            ProductTypeInfo typeInfo = typeservice.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            model.ColorAlias = colorAlias;
            model.SizeAlias = sizeAlias;
            model.VersionAlias = versionAlias;
            if (product.SKUInfo != null && product.SKUInfo.Count() > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                //var skus = _iProductService.GetSKUs(id);
                foreach (var sku in product.SKUInfo.OrderBy(s => s.AutoId).ToList())
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
                                    Img = Himall.Core.HimallIO.GetImagePath(sku.ShowPic)
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
                                    //Name = "选择规格",
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

            return View(model);
        }

        [HttpPost]
        public JsonResult GetHasSku(long id)
        {
            bool result = false;
            var proser = _iProductService;
            result = proser.HasSKU(id);
            return Json(new { hassku = result });
        }
        /// <summary>
        /// 获取商品描述
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetProductDescription(long pid)
        {
            var ProductDescription = ServiceHelper.Create<IProductService>().GetProductDescription(pid);
            if (ProductDescription == null)
            {
                throw new Himall.Core.HimallException("错误的商品编号");
            }
            string DescriptionPrefix = "", DescriptiondSuffix = "";

            var iprodestempser = ServiceHelper.Create<IProductDescriptionTemplateService>();
            if (ProductDescription.DescriptionPrefixId != 0)
            {
                var desc = iprodestempser.GetTemplate(ProductDescription.DescriptionPrefixId, ProductDescription.ProductInfo.ShopId);
                DescriptionPrefix = desc == null ? "" : desc.MobileContent;
            }

            if (ProductDescription.DescriptiondSuffixId != 0)
            {
                var desc = iprodestempser.GetTemplate(ProductDescription.DescriptiondSuffixId, ProductDescription.ProductInfo.ShopId);
                DescriptiondSuffix = desc == null ? "" : desc.MobileContent;
            }
            return Json(new
            {
                prodes = ProductDescription.ShowMobileDescription,
                prodesPrefix = DescriptionPrefix,
                prodesSuffix = DescriptiondSuffix
            }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetHotProduct(long productId, int categoryId)
        {
            var relationProduct = ProductManagerApplication.GetRelationProductByProductId(productId);
            List<DTO.Product.Product> products;
            if (relationProduct == null || ProductManagerApplication.GetProductsByIds(relationProduct.RelationProductIds).Count == 0)
                products = ProductManagerApplication.GetHotSaleProductByCategoryId(categoryId, 10);
            else
                products = ProductManagerApplication.GetProductsByIds(relationProduct.RelationProductIds);

            foreach (var item in products)
            {
                item.ImagePath = item.GetImage(ImageSize.Size_220);
            }

            return Json(products, true);
        }

        public JsonResult CanBuy(long productId, int count)
        {
            if (CurrentUser == null)
                return Json(new { Result = true }, true);
            if (CurrentUser.Disabled)
            {
                return Json(new
                {
                    Result = false,
                    ResultType = -10,
                    Message = "用户被冻结"
                }, true);

            }
            int reason;
            var msg = new Dictionary<int, string>() { { 0, "" }, { 1, "商品已下架" }, { 2, "很抱歉，您查看的商品不存在，可能被转移。" }, { 3, "超出商品最大限购数" }, { 9, "商品无货" } };
            var result = ProductManagerApplication.CanBuy(CurrentUser.Id, productId, count, out reason);

            return Json(new
            {
                Result = result,
                ResultType = reason,
                Message = msg[reason]
            }, true);
        }
        #region 分销
        /// <summary>
        /// 获取商品的佣金信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetDistributionInfo(long id)
        {
            ProductGetDistributionInfoModel model = new ProductGetDistributionInfoModel { ProductId = id };
            var user = CurrentUser;
            model.ShareUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/m-wap/product/Detail/" + id.ToString();
            if (user != null && user.Id > 0)
            {
                model.UserId = user.Id;
                var prom = DistributionApplication.GetPromoterByUserId(model.UserId);
                if (prom != null)
                {
                    model.IsPromoter = true;
                    model.PromoterStatus = prom.Status;
                    if (prom.Status == PromoterInfo.PromoterStatus.Audited || prom.Status == PromoterInfo.PromoterStatus.NotAvailable)
                    {
                        model.ShareUrl += "?partnerid=" + model.UserId.ToString();
                    }
                }
            }

            var probroker = DistributionApplication.GetDistributionProductInfo(id);
            if (probroker != null)
            {
                model.IsDistribution = true;
                model.Brokerage = probroker.Commission;
            }

            model.WeiXinShareArgs = Application.WXApiApplication.GetWeiXinShareArgs(this.HttpContext.Request.UrlReferrer.AbsoluteUri);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 申请成为分销员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult ApplyDistribution(long id)
        {
            if (CurrentUser == null)
            {
                return Json(new Result { success = false, msg = "请登录" });
            }
            Result result = new Result();
            long curUserId = CurrentUser.Id;
            var setting = _iDistributionService.GetRecruitmentSetting();
            if (setting == null)
            {
                return Json(new Result() { success = false, msg = "平台未设置招募审核！" });
            }
            if (setting.MustRealName || setting.MustAddress)
            {
                return Json(new Result() { success = false, status = -1 });
            }
            string mobile = "";
            if (setting.MustMobile)
            {
                mobile = _iMessageService.GetDestination(curUserId, SMSPLUGIN, Himall.Model.MemberContactsInfo.UserTypes.General);
                if (string.IsNullOrEmpty(mobile))
                {
                    return Json(new Result() { success = false, status = -1 });
                }
            }
            var promoter = _iDistributionService.GetPromoterByUserId(curUserId);
            if (promoter != null && promoter.Status == PromoterInfo.PromoterStatus.Audited)
            {
                return Json(new Result() { success = true, msg = "你已经是销售员了！" });
            }
            Model.PromoterModel model = new Model.PromoterModel();
            model.Mobile = mobile;
            model.UserId = curUserId;
            _iDistributionService.ApplyForDistributor(model);
            promoter = _iDistributionService.GetPromoterByUserId(curUserId);
            result = new Result() { success = true, msg = "提交成功！", status = 1 };
            if (promoter != null)
            {
                if (promoter.Status == PromoterInfo.PromoterStatus.UnAudit)
                {
                    result = new Result() { success = false, msg = "提交成功！", status = 2 };
                }
            }
            return Json(result);
        }
        #endregion

        #region 页面调用块
        /// <summary>
        /// 显示发货地
        /// </summary>
        /// <param name="ftid">运费模板编号</param>
        /// <returns></returns>    
        public ActionResult ShowDepotAddress(long ftid)
        {
            FreightTemplateInfo template = _iFreightTemplateService.GetFreightTemplate(ftid);
            string productAddress = string.Empty;
            if (template != null && template.SourceAddress.HasValue)
            {
                var fullName = _iRegionService.GetFullName(template.SourceAddress.Value);
                if (fullName != null)
                {
                    var ass = fullName.Split(' ');
                    if (ass.Length >= 2)
                    {
                        productAddress = ass[0] + " " + ass[1];
                    }
                    else
                    {
                        productAddress = ass[0];
                    }
                }
            }

            template.DepotAddress = productAddress;
            return View(template);
        }
        /// <summary>
        /// 显示服务承诺
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <returns></returns>
		[ChildActionOnly]
        public ActionResult ShowServicePromise(long id, long shopId)
        {
            var model = _iCashDepositsService.GetCashDepositsObligation(id);
            int regionId = 0;
            if (CurrentUser != null)
            {
                var defaultShippingAddress = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
                if (defaultShippingAddress != null)
                    regionId = defaultShippingAddress.RegionId;
            }

            if (regionId == 0)
            {
                string curip = Himall.Core.Helper.WebHelper.GetIP();
                regionId = (int)RegionApplication.GetRegionByIPInTaobao(curip);
            }

            var region = RegionApplication.GetRegion(regionId, Region.RegionLevel.City);

            if (region != null)
            {
                var shopBranchQuery = new ShopBranchQuery();
                shopBranchQuery.ShopId = shopId;
                shopBranchQuery.Status = ShopBranchStatus.Normal;
                shopBranchQuery.ProductIds = new[] { id };
                shopBranchQuery.AddressPath = region.GetIdPath();
                model.CanSelfTake = ShopBranchApplication.Exists(shopBranchQuery);
            }

            ViewBag.ProductId = id;
            ViewBag.ShopId = shopId;
            return View(model);
        }
        /// <summary>
        /// 显示商品详情
        /// <para>显示模板，由js完成内容加载</para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult ShowProductDescription(long id)
        {
            return View(id);
        }


        /// <summary>
        /// 显示商品评论前两条
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <param name="top">取多少条</param>
        /// <returns></returns>      
        public ActionResult ProductCommentShow(long id, int top = 1, bool isshowtit = false)
        {
            ProductCommentShowModel model = new ProductCommentShowModel();
            model.ProductId = id;
            var product = _iProductService.GetProduct(id);
            model.CommentList = new List<ProductDetailCommentModel>();
            model.IsShowColumnTitle = isshowtit;
            model.IsShowCommentList = true;
            if (top < 1)
            {
                model.IsShowCommentList = false;
            }

            if (product == null)
            {
                //跳转到404页面
                throw new Core.HimallException("商品不存在");
            }
            if (product.IsDeleted)
            {
                //跳转到404页面
                throw new Core.HimallException("商品不存在");
            }
            var com = product.Himall_ProductComments.Where(item => !item.IsHidden.HasValue || item.IsHidden.Value == false);
            var comCount = com.Count();
            model.CommentCount = comCount;
            if (comCount > 0 && top > 0)
            {
                model.CommentList = com.OrderByDescending(a => a.ReviewDate).Take(top).Select(c => new ProductDetailCommentModel
                {
                    Sku = _iProductService.GetSkuString(c.Himall_OrderItems.SkuId),
                    UserName = c.UserName.Substring(0, 1) + "***" + c.UserName.Substring(c.UserName.Length - 1, 1),
                    ReviewContent = c.ReviewContent,
                    AppendContent = c.AppendContent,
                    AppendDate = c.AppendDate,
                    ReplyAppendContent = c.ReplyAppendContent,
                    ReplyAppendDate = c.ReplyAppendDate,
                    FinshDate = c.Himall_OrderItems.OrderInfo.FinishDate,
                    Images = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 0).Select(a => a.CommentImage).ToList(),
                    AppendImages = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 1).Select(a => a.CommentImage).ToList(),
                    ReviewDate = c.ReviewDate,
                    ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                    ReplyDate = c.ReplyDate,
                    ReviewMark = c.ReviewMark,
                    BuyDate = c.Himall_OrderItems.OrderInfo.OrderDate

                }).ToList();
                foreach (var citem in model.CommentList)
                {
                    if (citem.Images.Count > 0)
                    {
                        for (var _imgn = 0; _imgn < citem.Images.Count; _imgn++)
                        {
                            citem.Images[_imgn] = Himall.Core.HimallIO.GetRomoteImagePath(citem.Images[_imgn]);
                        }
                    }
                    if (citem.AppendImages.Count > 0)
                    {
                        for (var _imgn = 0; _imgn < citem.AppendImages.Count; _imgn++)
                        {
                            citem.AppendImages[_imgn] = Himall.Core.HimallIO.GetRomoteImagePath(citem.AppendImages[_imgn]);
                        }
                    }
                }
            }

            return View(model);
        }

        #endregion

        /// <summary>
        /// 异步请求是否门店授权
        /// </summary>
        /// <returns></returns>
        public JsonResult GetIsOpenStore()
        {
            var result = new
            {
                Success = true,
                IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore
            };
            return Json(result, true);
        }

        /// <summary>
        /// 获取该商品所在商家下距离用户最近的门店
        /// </summary>
        /// <param name="shopId">商家ID</param>
        /// <returns></returns>
        public JsonResult GetStroreInfo(long shopId, long productId, string fromLatLng = "")
        {
            if (shopId <= 0) return Json(new { Success = false, Message = "请传入合法商家ID" }, true);
            if (!(fromLatLng.Split(',').Length == 2)) return Json(new { Success = false, Message = "您当前定位信息异常" }, true);

            var query = new CommonModel.ShopBranchQuery()
            {
                ShopId = shopId,
                FromLatLng = fromLatLng,
                Status = CommonModel.ShopBranchStatus.Normal
            };
            int total = ShopBranchApplication.GetShopBranchsAll(query).Models.Where(p => (p.Latitude > 0 && p.Longitude > 0)).ToList().Count; //商家下门店总数
            query.ProductIds = new long[] { productId };
            var shopBranch = ShopBranchApplication.GetShopBranchsAll(query).Models.Where(p => (p.Latitude > 0 && p.Longitude > 0)).OrderBy(p => p.Distance).Take(1).FirstOrDefault<Himall.DTO.ShopBranch>();  //商家下有该产品的且距离用户最近的门店
            var result = new
            {
                Success = true,
                StoreInfo = shopBranch,
                Total = total
            };
            return Json(result, true);
        }
        /// <summary>
        /// 是否可允许自提门店
        /// </summary>
        /// <param name="shopId">商家ID</param>
        /// <returns></returns>
        public JsonResult GetIsSelfDelivery(long shopId, long productId, string fromLatLng = "")
        {
            if (shopId <= 0) return Json(new { Message = "请传入合法商家ID", IsSelfDelivery = 0 }, true);
            if (!(fromLatLng.Split(',').Length == 2)) return Json(new { Message = "请传入合法经纬度", IsSelfDelivery = 0 }, true);

            var query = new CommonModel.ShopBranchQuery()
            {
                ShopId = shopId,
                Status = CommonModel.ShopBranchStatus.Normal
            };
            string address = "", province = "", city = "", district = "", street = "";
            Common.ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (string.IsNullOrWhiteSpace(city)) return Json(new { Message = "无法获取当前城市", IsSelfDelivery = 0 }, true);

            Region cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
            if (cityInfo == null) return Json(new { Message = "获取当前城市异常", IsSelfDelivery = 0 }, true);
            query.CityId = cityInfo.Id;
            query.ProductIds = new long[] { productId };

            var shopBranch = ShopBranchApplication.GetShopBranchsAll(query).Models;//获取该商品所在商家下，且与用户同城内门店，且门店有该商品
            var skuInfos = ProductManagerApplication.GetSKU(productId);//获取该商品的sku
            //门店SKU内会有默认的SKU
            if (!skuInfos.Exists(p => p.Id == string.Format("{0}_0_0_0", productId))) skuInfos.Add(new DTO.SKU()
            {
                Id = string.Format("{0}_0_0_0", productId)
            });
            var shopBranchSkus = ShopBranchApplication.GetSkus(query.ShopId, shopBranch.Select(p => p.Id));//门店商品SKU

            //门店商品SKU，只要有一个sku有库存即可
            shopBranch.ForEach(p =>
                p.Enabled = skuInfos.Where(skuInfo => shopBranchSkus.Where(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock > 0 && sbSku.SkuId == skuInfo.Id).Count() > 0).Count() > 0
            );
            return Json(new { Message = "", IsSelfDelivery = shopBranch.Where(p => p.Enabled).Count() > 0 ? 1 : 0 }, true);//至少有一个能够自提的门店，才可显示图标
        }
    }
}