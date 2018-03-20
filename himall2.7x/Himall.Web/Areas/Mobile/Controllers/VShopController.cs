using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Mobile.Models;
using Himall.Web.Areas.Web.Helper;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Helpers;
using System.IO;
using Himall.Application;
using Himall.CommonModel;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class VShopController : BaseMobileTemplatesController
    {
        private SiteSettingsInfo _siteSetting = null;
        private string wxlogo = "/images/defaultwxlogo.png";
        private WXCardLogInfo.CouponTypeEnum ThisCouponType = WXCardLogInfo.CouponTypeEnum.Coupon;
        private IWXCardService _iWXCardService;
        private IVShopService _iVShopService;
        private IShopService _iShopService;
        private ITemplateSettingsService _iTemplateSettingsService;
        private IProductService _iProductService;
        private ICustomerService _iCustomerService;
        private IShopBonusService _iShopBonusService;

        public VShopController(IWXCardService iWXCardService,
            IVShopService iVShopService,
             IShopService iShopService,
             ITemplateSettingsService iTemplateSettingsService,
            IProductService iProductService,
            ICustomerService iCustomerService
            , IShopBonusService iShopBonusService
            )
        {
            this._siteSetting = CurrentSiteSetting;
            this._iWXCardService = iWXCardService;
            _iVShopService = iVShopService;
            _iShopService = iShopService;
            _iTemplateSettingsService = iTemplateSettingsService;
            _iProductService = iProductService;
            _iCustomerService = iCustomerService;
            _iShopBonusService = iShopBonusService;
        }

        [HttpGet]
        public ActionResult List()
        {
            return View();
        }

        [HttpPost]
        public JsonResult List(int page, int pageSize)
        {
            int total;
            var vshops = _iVShopService.GetVShops(page, pageSize, out total, VShopInfo.VshopStates.Normal).ToArray();
            long[] favoriteShopIds = new long[] { };
            if (CurrentUser != null)
                favoriteShopIds = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();
            var model = vshops.Select(item =>
            {
                int productCount = _iShopService.GetShopProductCount(item.ShopId);
                int FavoritesCount = _iShopService.GetShopFavoritesCount(item.ShopId);
                return new
                {
                    id = item.Id,
                    image = HimallIO.GetImagePath(item.BackgroundImage),
                    tags = item.Tags,
                    name = item.Name,
                    shopId = item.ShopId,
                    favorite = favoriteShopIds.Contains(item.ShopId),
                    productCount = productCount,
                    FavoritesCount = FavoritesCount
                };
            });
            return Json(model);
        }

        [ActionName("Index")]
        public ActionResult Main()
        {
            var service = _iVShopService;
            var topShop = service.GetTopShop();
            bool isFavorite = false;
            if (topShop != null)
            {
                var queryModel = new ProductQuery()
                {
                    PageSize = 3,
                    PageNo = 1,
                    ShopId = topShop.ShopId
                };
                queryModel.AuditStatus = new Model.ProductInfo.ProductAuditStatus[] { ProductInfo.ProductAuditStatus.Audited };
                queryModel.SaleStatus = Model.ProductInfo.ProductSaleStatus.OnSale;
                var products = _iProductService.GetProducts(queryModel).Models.ToList();
                var topShopProducts = products.Select(item => new Himall.Web.Areas.Mobile.Models.ProductItem()
                {
                    Id = item.Id,
                    ImageUrl = item.GetImage(ImageSize.Size_350),
                    MarketPrice = item.MarketPrice,
                    Name = item.ProductName,
                    SalePrice = item.MinSalePrice
                });
                ViewBag.TopShopProducts = topShopProducts;//主推店铺的商品
                if (CurrentUser != null)
                {
                    var favoriteShopIds = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();//获取已关注店铺
                    isFavorite = favoriteShopIds.Contains(topShop.ShopId);
                }
                int productCount = _iShopService.GetShopProductCount(topShop.ShopId);
                int FavoritesCount = _iShopService.GetShopFavoritesCount(topShop.ShopId);
                ViewBag.ProductCount = productCount;
                ViewBag.FavoritesCount = FavoritesCount;
                if (!string.IsNullOrEmpty(topShop.Tags))
                {
                    var array = topShop.Tags.Split(new string[] { ";", "；" }, StringSplitOptions.RemoveEmptyEntries);
                    string wxTag = string.Empty;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (i < 2)
                            wxTag += " " + array[i] + " ·";
                    }
                    wxTag = wxTag.TrimStart().Trim('·');
                    ViewBag.Tags = wxTag;
                }
            }


            ViewBag.IsFavorite = isFavorite;
            return View(topShop);
        }

        [HttpPost]
        public JsonResult GetHotShops(int page, int pageSize)
        {
            int total;
            var hotShops = _iVShopService.GetHotShops(page, pageSize, out total).ToArray();//获取热门微店
            var homeProductService = ServiceHelper.Create<IMobileHomeProductsService>();
            long[] favoriteShopIds = new long[] { };
            if (CurrentUser != null)
                favoriteShopIds = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();
            var model = hotShops.Select(item =>
                {
                    int productCount = _iShopService.GetShopProductCount(item.ShopId);
                    int FavoritesCount = _iShopService.GetShopFavoritesCount(item.ShopId);
                    var queryModel = new ProductQuery()
                    {
                        PageSize = 3,
                        PageNo = 1,
                        ShopId = item.ShopId
                    };
                    queryModel.AuditStatus = new Model.ProductInfo.ProductAuditStatus[] { ProductInfo.ProductAuditStatus.Audited };
                    queryModel.SaleStatus = Model.ProductInfo.ProductSaleStatus.OnSale;
                    var products = _iProductService.GetProducts(queryModel).Models.ToList();
                    var tags = string.Empty;
                    if (!string.IsNullOrEmpty(item.Tags))
                    {
                        var array = item.Tags.Split(new string[] { ";", "；" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (i < 2)
                                tags += " " + array[i] + " ·";
                        }
                        tags = tags.TrimStart().Trim('·');
                    }


                    return new
                    {
                        id = item.Id,
                        name = item.Name,
                        logo = HimallIO.GetImagePath(item.Logo),
                        products = products.Select(t => new
                        {
                            id = t.Id,
                            name = t.ProductName,
                            image = t.GetImage(ImageSize.Size_220),
                            salePrice = t.MinSalePrice,
                        }),
                        favorite = favoriteShopIds.Contains(item.ShopId),
                        shopId = item.ShopId,
                        productCount = productCount,
                        FavoritesCount = FavoritesCount,
                        Tags = tags
                    };
                }
            );

            return Json(model);
        }

        public ActionResult Detail(long id, int? couponid, int? shop, bool sv = false, int ispv = 0, string tn = "")
        {
            var vshop = _iVShopService.GetVShop(id);
            string crrentTemplateName = "t1";
            _iShopService.CheckInitTemplate(vshop.ShopId);
            var curr = _iTemplateSettingsService.GetCurrentTemplate(vshop.ShopId);
            if (null != curr)
            {
                crrentTemplateName = curr.CurrentTemplateName;
            }
            if (ispv == 1)
            {
                if (!string.IsNullOrWhiteSpace(tn))
                {
                    crrentTemplateName = tn;
                }
            }
            ViewBag.VshopId = id;
            ViewBag.ShopId = vshop.ShopId;
            ViewBag.Title = vshop.HomePageTitle;

            //var customerServices = CustomerServiceApplication.GetMobileCustomerService(vshop.ShopId);
            //var meiqia = CustomerServiceApplication.GetPreSaleByShopId(vshop.ShopId).FirstOrDefault(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            //if (meiqia != null)
            //	customerServices.Insert(0, meiqia);
            //ViewBag.CustomerServices = customerServices;

            ViewBag.ShowAside = 1;
            //统计店铺访问人数
            StatisticApplication.StatisticShopVisitUserCount(vshop.ShopId);
            return View(string.Format("~/Areas/SellerAdmin/Templates/vshop/{0}/{1}/Skin-HomePage.cshtml", vshop.ShopId, crrentTemplateName));
        }
        public JsonResult LoadProductsFromCache(long shopid, long page)
        {
            var html = TemplateSettingsApplication.GetShopGoodTagFromCache(shopid, page);
            return Json(new { htmlTag = html }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 未开通微店提醒
        /// </summary>
        /// <returns></returns>
        public ActionResult NoOpenVShopTips()
        {
            return View();
        }

        #region 优惠券
        private IEnumerable<CouponInfo> GetCouponList(long shopid)
        {
            var service = ServiceHelper.Create<ICouponService>();
            var result = service.GetCouponList(shopid);
            var couponSetList = _iVShopService.GetVShopCouponSetting(shopid).Select(item => item.CouponID);
            if (result.Count() > 0 && couponSetList.Count() > 0)
            {
                var couponList = result.ToArray().Where(item => couponSetList.Contains(item.Id));//取设置的优惠券
                return couponList;
            }
            return null;
        }

        public ActionResult CouponInfo(long id, int? accept)
        {
            VshopCouponInfoModel result = new VshopCouponInfoModel();
            var couponService = ServiceHelper.Create<ICouponService>();
            var couponInfo = couponService.GetCouponInfo(id) ?? new CouponInfo() { };
            if (couponInfo.EndTime < DateTime.Now)
            {
                //已经失效
                result.CouponStatus = Himall.Model.CouponInfo.CouponReceiveStatus.HasExpired;
            }

            if (CurrentUser != null)
            {
                CouponRecordQuery crQuery = new CouponRecordQuery();
                crQuery.CouponId = id;
                crQuery.UserId = CurrentUser.Id;
                ObsoletePageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
                if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax)
                {
                    //达到个人领取最大张数
                    result.CouponStatus = Himall.Model.CouponInfo.CouponReceiveStatus.HasLimitOver;
                }
                crQuery = new CouponRecordQuery()
                {
                    CouponId = id,
                    PageNo = 1,
                    PageSize = 9999
                };
                pageModel = couponService.GetCouponRecordList(crQuery);
                if (pageModel.Total >= couponInfo.Num)
                {
                    //达到领取最大张数
                    result.CouponStatus = Himall.Model.CouponInfo.CouponReceiveStatus.HasReceiveOver;
                }
                if (couponInfo.ReceiveType == Himall.Model.CouponInfo.CouponReceiveType.IntegralExchange)
                {
                    var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                    if (userInte.AvailableIntegrals < couponInfo.NeedIntegral)
                    {
                        result.CouponStatus = Himall.Model.CouponInfo.CouponReceiveStatus.IntegralLess;
                    }
                }
                var isFav = _iShopService.IsFavoriteShop(CurrentUser.Id, couponInfo.ShopId);
                if (isFav)
                {
                    result.IsFavoriteShop = true;
                }
            }
            result.CouponId = id;
            if (accept.HasValue)
                result.AcceptId = accept.Value;

            var vshop = _iVShopService.GetVShopByShopId(couponInfo.ShopId);
            string curwxlogo = wxlogo;
            if (vshop != null)
            {
                result.VShopid = vshop.Id;
                if (!string.IsNullOrWhiteSpace(vshop.WXLogo))
                {
                    curwxlogo = vshop.WXLogo;
                }
                if (string.IsNullOrWhiteSpace(wxlogo))
                {
                    if (!string.IsNullOrWhiteSpace(this._siteSetting.WXLogo))
                    {
                        curwxlogo = this._siteSetting.WXLogo;
                    }
                }
            }
            ViewBag.ShopLogo = curwxlogo;
            var vshopSetting = _iVShopService.GetVShopSetting(couponInfo.ShopId);
            if (vshopSetting != null)
            {
                result.FollowUrl = vshopSetting.FollowUrl;
            }
            result.ShopId = couponInfo.ShopId;
            result.CouponData = couponInfo;
            //补充ViewBag
            ViewBag.ShopId = result.ShopId;
            ViewBag.FollowUrl = result.FollowUrl;
            ViewBag.FavText = result.IsFavoriteShop ? "已收藏" : "收藏店铺";
            ViewBag.VShopid = result.VShopid;
            return View(result);
        }
        [HttpPost]
        public JsonResult AcceptCoupon(long vshopid, long couponid)
        {
            if (CurrentUser == null)
            {
                return Json(new { status = 1, success = false, msg = "未登录." });
            }
            var couponService = ServiceHelper.Create<ICouponService>();
            var couponInfo = couponService.GetCouponInfo(couponid);
            if (couponInfo.EndTime < DateTime.Now)
            {//已经失效
                return Json(new { status = 2, success = false, msg = "优惠券已经过期." });
            }
            CouponRecordQuery crQuery = new CouponRecordQuery();
            crQuery.CouponId = couponid;
            crQuery.UserId = CurrentUser.Id;
            ObsoletePageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
            if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax)
            {
                //达到个人领取最大张数
                return Json(new { status = 3, success = false, msg = "达到个人领取最大张数，不能再领取." });
            }
            crQuery = new CouponRecordQuery()
            {
                CouponId = couponid
            };
            pageModel = couponService.GetCouponRecordList(crQuery);
            if (pageModel.Total >= couponInfo.Num)
            {//达到领取最大张数
                return Json(new { status = 4, success = false, msg = "此优惠券已经领完了." });
            }
            if (couponInfo.ReceiveType == Himall.Model.CouponInfo.CouponReceiveType.IntegralExchange)
            {
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                if (userInte.AvailableIntegrals < couponInfo.NeedIntegral)
                {
                    //积分不足
                    return Json(new { status = 5, success = false, msg = "积分不足 " + couponInfo.NeedIntegral.ToString() });
                }
            }
            CouponRecordInfo couponRecordInfo = new CouponRecordInfo()
            {
                CouponId = couponid,
                UserId = CurrentUser.Id,
                UserName = CurrentUser.UserName,
                ShopId = couponInfo.ShopId
            };
            couponService.AddCouponRecord(couponRecordInfo);
            return Json(new { status = 0, success = true, msg = "领取成功", crid = couponRecordInfo.Id });//执行成功
        }
        public ActionResult GetCouponSuccess(long id)
        {
            VshopCouponInfoModel result = new VshopCouponInfoModel();
            var couponser = ServiceHelper.Create<ICouponService>();
            var couponRecordInfo = couponser.GetCouponRecordById(id);
            if (couponRecordInfo == null) throw new HimallException("错误的优惠券编号");
            var couponInfo = couponser.GetCouponInfo(couponRecordInfo.ShopId, couponRecordInfo.CouponId);
            if (couponInfo == null) throw new HimallException("错误的优惠券编号");
            result.CouponData = couponInfo;
            result.CouponId = couponInfo.Id;
            result.CouponRecordId = couponRecordInfo.Id;
            result.ShopId = couponInfo.ShopId;
            result.IsShowSyncWeiXin = false;

            if (CurrentUser != null)
            {
                var isFav = _iShopService.IsFavoriteShop(CurrentUser.Id, couponInfo.ShopId);
                if (isFav)
                {
                    result.IsFavoriteShop = true;
                }
            }
            result.CouponId = id;

            #region 同步微信前信息准备
            if (couponInfo.IsSyncWeiXin == 1 && this.PlatformType == PlatformType.WeiXin)
            {
                result.WXJSInfo = _iWXCardService.GetSyncWeiXin(couponInfo.Id, couponRecordInfo.Id, ThisCouponType, Request.Url.AbsoluteUri);
                if (result.WXJSInfo != null)
                {
                    result.IsShowSyncWeiXin = true;
                    //result.WXJSCardInfo = ser_wxcard.GetJSWeiXinCard(couponRecordInfo.CouponId, couponRecordInfo.Id, ThisCouponType);    //同步方式有重复留的Bug
                }
            }
            #endregion

            string curwxlogo = wxlogo;
            var vshop = _iVShopService.GetVShopByShopId(couponInfo.ShopId);
            if (vshop != null)
            {
                result.VShopid = vshop.Id;
                if (!string.IsNullOrWhiteSpace(vshop.WXLogo))
                {
                    curwxlogo = vshop.WXLogo;
                }
                if (string.IsNullOrWhiteSpace(wxlogo))
                {
                    if (!string.IsNullOrWhiteSpace(this._siteSetting.WXLogo))
                    {
                        curwxlogo = this._siteSetting.WXLogo;
                    }
                }
            }
            ViewBag.ShopLogo = curwxlogo;
            //补充ViewBag
            ViewBag.ShopId = result.ShopId;
            ViewBag.FollowUrl = result.FollowUrl;
            ViewBag.FavText = result.IsFavoriteShop ? "已收藏" : "收藏店铺";
            ViewBag.VShopid = result.VShopid;
            return View(result);
        }
        [HttpPost]
        public JsonResult GetWXCardData(long id)
        {
            WXJSCardModel result = new WXJSCardModel();
            bool isdataok = true;
            var couponser = ServiceHelper.Create<ICouponService>();
            CouponRecordInfo couponRecordInfo = null;
            if (isdataok)
            {
                couponRecordInfo = couponser.GetCouponRecordById(id);
                if (couponRecordInfo == null)
                {
                    isdataok = false;
                }
            }
            CouponInfo couponInfo = null;
            if (isdataok)
            {
                couponInfo = couponser.GetCouponInfo(couponRecordInfo.ShopId, couponRecordInfo.CouponId);
                if (couponInfo == null)
                {
                    isdataok = false;
                }
            }
            #region 同步微信前信息准备
            if (isdataok)
            {
                if (couponInfo.IsSyncWeiXin == 1 && this.PlatformType == PlatformType.WeiXin)
                {
                    result = _iWXCardService.GetJSWeiXinCard(couponRecordInfo.CouponId, couponRecordInfo.Id, ThisCouponType);
                }
            }
            #endregion
            return Json(result);
        }
        #endregion

        public JsonResult AddFavorite(long shopId)
        {
            if (CurrentUser == null)
                return Json(new Result { success = false, msg = "请先登录." });
            _iShopService.AddFavoriteShop(CurrentUser.Id, shopId);
            return Json(new Result { success = true, msg = "成功关注该微店." });
        }

        public JsonResult DeleteFavorite(long shopId)
        {
            _iShopService.CancelConcernShops(shopId, CurrentUser.Id);
            return Json(new Result { success = true, msg = "成功取消关注该微店." });
        }

        public ActionResult Introduce(long id)
        {
            var vshop = _iVShopService.GetVShop(id);
            string qrCodeImagePath = string.Empty;
            if (vshop != null)
            {
                string vshopUrl = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/vshop/detail/" + id;

                Image map;
                if (!string.IsNullOrWhiteSpace(vshop.Logo) && HimallIO.ExistFile(vshop.Logo))
                    map = Core.Helper.QRCodeHelper.Create(vshopUrl, HimallIO.GetImagePath(vshop.Logo));
                else
                    map = Core.Helper.QRCodeHelper.Create(vshopUrl);

                MemoryStream ms = new MemoryStream();
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                //  将图片内存流转成base64,图片以DataURI形式显示  
                string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
                ms.Dispose();
                qrCodeImagePath = strUrl;
            }
            ViewBag.QRCode = qrCodeImagePath;
            bool isFavorite;
            if (CurrentUser == null)
                isFavorite = false;
            else
                isFavorite = _iShopService.IsFavoriteShop(CurrentUser.Id, vshop.ShopId);
            ViewBag.IsFavorite = isFavorite;
            var mark = ShopServiceMark.GetShopComprehensiveMark(vshop.ShopId);
            ViewBag.shopMark = mark.ComprehensiveMark.ToString();

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = _iShopService.GetShopStatisticOrderComments(vshop.ShopId);

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

            return View(vshop);
        }

        [HttpPost]
        public JsonResult ProductList(long shopId, int pageNo, int pageSize)
        {
            var productList = ServiceHelper.Create<IMobileHomeProductsService>().GetMobileHomePageProducts(shopId, Himall.Core.PlatformType.WeiXin).OrderBy(item => item.Sequence).ThenByDescending(o => o.Id).Skip((pageNo - 1) * pageSize).Take(pageSize);
            var result = productList.ToArray().Select(item => new
            {
                Id = item.ProductId,
                ImageUrl = item.Himall_Products.GetImage(ImageSize.Size_350),
                Name = item.Himall_Products.ProductName,
                MarketPrice = item.Himall_Products.MarketPrice,
                SalePrice = item.Himall_Products.MinSalePrice.ToString("F2")
            });
            return Json(result);
        }

        public ActionResult Search(string keywords = "", /* 搜索关键字 */
        string exp_keywords = "", /* 渐进搜索关键字 */
        long cid = 0,  /* 分类ID */
        long b_id = 0, /* 品牌ID */
        string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
        int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
        int orderType = 1, /* 排序方式（1：升序，2：降序） */
        int pageNo = 1, /*页码*/
        int pageSize = 6, /*每页显示数据量*/
        long vshopId = 0,//店铺ID
        long shopCid = 0//店铺分类 
        )
        {
            int total;
            long shopId = -1;
            if (vshopId > 0)
            {
                var vshop = _iVShopService.GetVShop(vshopId);
                if (vshop != null)
                    shopId = vshop.ShopId;
            }
            if (!string.IsNullOrWhiteSpace(keywords))
                keywords = keywords.Trim();

            ProductSearch model = new ProductSearch()
            {
                shopId = shopId,
                BrandId = b_id,
                CategoryId = cid,
                Ex_Keyword = exp_keywords,
                Keyword = keywords,
                OrderKey = orderKey,
                ShopCategoryId = shopCid,
                OrderType = orderType == 1,
                AttrIds = new System.Collections.Generic.List<string>(),
                PageNumber = pageNo,
                PageSize = pageSize,
            };

            var productsResult = ServiceHelper.Create<IProductService>().SearchProduct(model);
            total = productsResult.Total;
            var products = productsResult.Models.ToArray();


            decimal discount = 1M;
            long selfShopId = 0;
            var selfshop = _iShopService.GetSelfShop();
            if (selfshop != null) selfShopId = selfshop.Id;
            if (CurrentUser != null) discount = CurrentUser.MemberDiscount;

            var limit = LimitTimeApplication.GetLimitProducts();
            var fight = FightGroupApplication.GetFightGroupPrice();
            var commentService = ServiceHelper.Create<ICommentService>();
            var productsModel = products.Select(item =>
                new ProductItem()
                {
                    Id = item.Id,
                    ImageUrl = item.GetImage(ImageSize.Size_350),
                    //SalePrice = (item.ShopId == selfshop.Id ? item.MinSalePrice * discount : item.MinSalePrice),
                    SalePrice = GetProductPrice(item, limit, fight, discount, selfShopId),
                    Name = item.ProductName,
                    CommentsCount = commentService.GetCommentsByProductId(item.Id).Count()
                }
            );



            var bizCategories = ServiceHelper.Create<IShopCategoryService>().GetShopCategory(shopId);
            var shopCategories = GetSubCategories(bizCategories, 0, 0);

            ViewBag.ShopCategories = shopCategories;
            ViewBag.Total = total;
            ViewBag.Keywords = keywords;
            ViewBag.VShopId = vshopId;
            if (shopId > 0)
            {
                //统计店铺访问人数
                StatisticApplication.StatisticShopVisitUserCount(shopId);
            }
            return View(productsModel);
        }

        /// <summary>
        /// 商品价格
        /// </summary>
        /// <returns></returns>
        private decimal GetProductPrice(ProductInfo item, List<FlashSalePrice> limit, List<FightGroupPrice> fight, decimal discount, long selfShopId)
        {
            decimal price = item.MinSalePrice;//原价

            if (item.ShopId == selfShopId) price = price * discount;//自营店，会员价

            var isLimit = limit.Where(r => r.ProductId == item.Id).FirstOrDefault();
            var isFight = fight.Where(r => r.ProductId == item.Id).FirstOrDefault();

            if (isLimit != null) price = isLimit.MinPrice;//限时购价
            if (isFight != null) price = isFight.ActivePrice;//团购价

            return price;
        }

        /// <summary>
        ///  商品搜索页面
        /// </summary>
        /// <param name="keywords">搜索关键字</param>
        /// <param name="exp_keywords">渐进搜索关键字</param>
        /// <param name="cid">分类ID</param>
        /// <param name="b_id">品牌ID</param>
        /// <param name="a_id">属性ID, 表现形式：attrId_attrValueId</param>
        /// <param name="orderKey">序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间）</param>
        /// <param name="orderType">排序方式（1：升序，2：降序）</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页显示数据量</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Search(
            string keywords = "", /* 搜索关键字 */
            string exp_keywords = "", /* 渐进搜索关键字 */
            long cid = 0,  /* 分类ID */
            long b_id = 0, /* 品牌ID */
            string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
            int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
            int orderType = 1, /* 排序方式（1：升序，2：降序） */
            int pageNo = 1, /*页码*/
            int pageSize = 6,/*每页显示数据量*/
           long vshopId = 0,//微店ID
           long shopCid = 0,
           string t = ""/*无意义参数，为了重载*/
            )
        {
            int total;
            long shopId = -1;
            if (vshopId > 0)
            {
                var vshop = _iVShopService.GetVShop(vshopId);
                if (vshop != null)
                    shopId = vshop.ShopId;
            }
            if (!string.IsNullOrWhiteSpace(keywords))
                keywords = keywords.Trim();
            ProductSearch model = new ProductSearch()
            {
                shopId = shopId,
                BrandId = b_id,
                CategoryId = cid,
                ShopCategoryId = shopCid,
                Ex_Keyword = exp_keywords,
                Keyword = keywords,
                OrderKey = orderKey,
                OrderType = orderType == 1,
                AttrIds = new System.Collections.Generic.List<string>(),
                PageNumber = pageNo,
                PageSize = pageSize
            };

            var productsResult = ServiceHelper.Create<IProductService>().SearchProduct(model);
            total = productsResult.Total;
            var products = productsResult.Models.ToArray();
            var selfshop = _iShopService.GetSelfShop();
            decimal discount = 1m;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var commentService = ServiceHelper.Create<ICommentService>();
            var resultModel = products.Select(item => new
            {
                id = item.Id,
                name = item.ProductName,
                price = (item.ShopId == selfshop.Id ? item.MinSalePrice * discount : item.MinSalePrice).ToString("F2"),
                commentsCount = commentService.GetCommentsByProductId(item.Id).Count(),
                img = item.GetImage(ImageSize.Size_350)
            });
            return Json(resultModel);
        }

        public ActionResult Category(long vShopId)
        {
            var vshopInfo = _iVShopService.GetVShop(vShopId);
            var bizCategories = ServiceHelper.Create<IShopCategoryService>().GetShopCategory(vshopInfo.ShopId).ToList();
            var shopCategories = GetSubCategories(bizCategories, 0, 0);
            ViewBag.VShopId = vShopId;
            return View(shopCategories);
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
                });
            return categories;
        }
        [HttpPost]
        public JsonResult GetVShopIdByShopId(long shopId)
        {
            var vshop = _iVShopService.GetVShopByShopId(shopId);
            return Json(new Result { success = true, msg = vshop.Id.ToString() });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="couponid"></param>
        /// <param name="shop"></param>
        /// <param name="sv"></param>
        /// <param name="ispv"></param>
        /// <param name="tn"></param>
        /// <returns></returns>
        public ActionResult VShopHeader(long id)
        {
            var vshopService = ServiceHelper.Create<IVShopService>();
            var vshop = vshopService.GetVShop(id);
            if (vshop == null)
            {
                throw new HimallException("错误的微店Id");
            }
            //轮播图
            var slideImgs = ServiceHelper.Create<ISlideAdsService>().GetSlidAds(vshop.ShopId, Model.SlideAdInfo.SlideAdType.VShopHome).ToList();


            var homeProducts = ServiceHelper.Create<IMobileHomeProductsService>().GetMobileHomePageProducts(vshop.ShopId, Himall.Core.PlatformType.WeiXin).OrderBy(item => item.Sequence).ThenByDescending(o => o.Id).Take(8);
            var products = homeProducts.ToArray().Select(item => new ProductItem()
            {
                Id = item.ProductId,
                ImageUrl = item.Himall_Products.GetImage(ImageSize.Size_350),
                Name = item.Himall_Products.ProductName,
                MarketPrice = item.Himall_Products.MarketPrice,
                SalePrice = item.Himall_Products.MinSalePrice
            });
            var banner = ServiceHelper.Create<INavigationService>().GetSellerNavigations(vshop.ShopId, Core.PlatformType.WeiXin).ToList();

            //var couponInfo = GetCouponList(vshop.ShopId);无效的代码，先注释掉

            ViewBag.SlideAds = slideImgs.ToArray().Select(item => new HomeSlideAdsModel() { ImageUrl = item.ImageUrl, Url = item.Url });

            ViewBag.Banner = banner;
            ViewBag.Products = products;
            if (CurrentUser == null)
                ViewBag.IsFavorite = false;
            else
                ViewBag.IsFavorite = ServiceHelper.Create<IShopService>().IsFavoriteShop(CurrentUser.Id, vshop.ShopId);
            //快速关注
            var vshopSetting = ServiceHelper.Create<IVShopService>().GetVShopSetting(vshop.ShopId);
            if (vshopSetting != null)
                ViewBag.FollowUrl = vshopSetting.FollowUrl;

            ViewBag.VshopId = id;
            ViewBag.ShopId = vshop.ShopId;
            return View("~/Areas/Mobile/Templates/Default/Views/Shared/_VShopHeader.cshtml", vshop);
        }
        /// <summary>
        /// 获取模板节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetTemplateItem(string id, long shopid, string tn = "")
        {
            string result = "";
            if (string.IsNullOrWhiteSpace(tn))
            {
                tn = "t1";
                var curr = _iTemplateSettingsService.GetCurrentTemplate(shopid);
                if (null != curr)
                {
                    tn = curr.CurrentTemplateName;
                }
            }
            result = VTemplateHelper.GetTemplateItemById(id, tn, VTemplateClientTypes.SellerWapIndex, shopid);
            return result;
        }

        #region 页面调用块
        /// <summary>
        /// 显示营销信息
        /// <para>优惠券，满额免</para>
        /// </summary>
        /// <param name="id">店铺编号</param>
        /// <param name="showcoupon">是否显示优惠券</param>
        /// <param name="showfreefreight">是否显示满额免</param>
        /// <param name="showfullsend">是否显示满就送</param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult ShowPromotion(long id, bool showcoupon = true, bool showfreefreight = true, bool showfullsend = true)
        {
            VShopShowPromotionModel model = new VShopShowPromotionModel();
            model.ShopId = id;
            var shop = _iShopService.GetShop(id);
            if (shop == null)
            {
                throw new HimallException("错误的店铺编号");
            }
            if (showcoupon)
            {
                model.CouponCount = ServiceHelper.Create<ICouponService>().GetTopCoupon(id, 10, PlatformType.Wap).Count();
            }

            if (showfreefreight)
            {
                model.FreeFreight = shop.FreeFreight;
            }
            model.BonusCount = 0;
            if (showfullsend)
            {
                var bonus = ServiceHelper.Create<IShopBonusService>().GetByShopId(id);
                if (bonus != null)
                {
                    model.BonusCount = bonus.Count;
                    model.BonusGrantPrice = bonus.GrantPrice;
                    model.BonusRandomAmountStart = bonus.RandomAmountStart;
                    model.BonusRandomAmountEnd = bonus.RandomAmountEnd;
                }
            }
            return View(model);
        }

        /// <summary>
        /// 移动端优惠券静态化
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetPromotions(long id)
        {
            VShopShowPromotionModel model = new VShopShowPromotionModel();
            model.ShopId = id;
            //model.CouponCount = ServiceHelper.Create<ICouponService>().GetTopCoupon(id, 10, PlatformType.Wap).Count();
            model.CouponCount = GetCouponCount(id);
            return Json(model);
        }

        /// <summary>
        /// 店铺评分
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult ShowShopScore(long id)
        {
            VShopShowShopScoreModel model = new VShopShowShopScoreModel();
            model.ShopId = id;
            var shop = _iShopService.GetShop(id);
            if (shop == null)
            {
                throw new HimallException("错误的店铺信息");
            }

            model.ShopName = shop.ShopName;

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
                model.ProductAndDescription = productAndDescription.CommentValue;
                model.ProductAndDescriptionPeer = productAndDescriptionPeer.CommentValue;
                model.ProductAndDescriptionMin = productAndDescriptionMin.CommentValue;
                model.ProductAndDescriptionMax = productAndDescriptionMax.CommentValue;
            }
            else
            {
                model.ProductAndDescription = defaultValue;
                model.ProductAndDescriptionPeer = defaultValue;
                model.ProductAndDescriptionMin = defaultValue;
                model.ProductAndDescriptionMax = defaultValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null)
            {
                model.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
                model.SellerServiceAttitudePeer = sellerServiceAttitudePeer.CommentValue;
                model.SellerServiceAttitudeMax = sellerServiceAttitudeMax.CommentValue;
                model.SellerServiceAttitudeMin = sellerServiceAttitudeMin.CommentValue;
            }
            else
            {
                model.SellerServiceAttitude = defaultValue;
                model.SellerServiceAttitudePeer = defaultValue;
                model.SellerServiceAttitudeMax = defaultValue;
                model.SellerServiceAttitudeMin = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null)
            {
                model.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
                model.SellerDeliverySpeedPeer = sellerDeliverySpeedPeer.CommentValue;
                model.SellerDeliverySpeedMax = sellerDeliverySpeedMax != null ? sellerDeliverySpeedMax.CommentValue : 0;
                model.sellerDeliverySpeedMin = sellerDeliverySpeedMin != null ? sellerDeliverySpeedMin.CommentValue : 0;
            }
            else
            {
                model.SellerDeliverySpeed = defaultValue;
                model.SellerDeliverySpeedPeer = defaultValue;
                model.SellerDeliverySpeedMax = defaultValue;
                model.sellerDeliverySpeedMin = defaultValue;
            }
            #endregion

            model.ProductNum = _iProductService.GetShopOnsaleProducts(id);
            model.IsFavoriteShop = false;
            model.FavoriteShopCount = _iShopService.GetShopFavoritesCount(id);
            if (CurrentUser != null)
            {
                model.IsFavoriteShop = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Any(d => d.ShopId == id);
            }

            long vShopId;
            var vshopinfo = _iVShopService.GetVShopByShopId(shop.Id);
            if (vshopinfo == null)
            {
                vShopId = -1;
            }
            else
            {
                vShopId = vshopinfo.Id;
            }
            model.VShopId = vShopId;
            model.VShopLog = _iVShopService.GetVShopLog(vShopId);
            if (string.IsNullOrWhiteSpace(model.VShopLog))
            {
                model.VShopLog = CurrentSiteSetting.WXLogo;
            }
            if (!string.IsNullOrWhiteSpace(model.VShopLog))
            {
                model.VShopLog = Himall.Core.HimallIO.GetImagePath(model.VShopLog);
            }

            return View(model);
        }
        #endregion
        #region 获取优惠券数
        internal int GetCouponCount(long shopId)
        {
            var service = ServiceHelper.Create<ICouponService>();
            //var result = service.GetCouponList(shopid);
            //var couponSetList = _iVShopService.GetVShopCouponSetting(shopid).Where(a => a.PlatForm == Core.PlatformType.Wap).Select(item => item.CouponID);
            //if (result.Count() > 0 && couponSetList.Count() > 0)
            //{
            //    var couponList = result.ToArray().Where(item => couponSetList.Contains(item.Id)).Select(p => new
            //    {
            //        Receive = Receive(p.Id)
            //    });
            //    return couponList.Where(p => p.Receive != 2 && p.Receive != 4).Count();//排除过期和已领完的
            //}
            //return 0;
            return service.GetUserCouponCount(shopId);
        }

        //private int Receive(long couponId)
        //{
        //    if (CurrentUser != null && CurrentUser.Id > 0)//未登录不可领取
        //    {
        //        var couponService = ServiceHelper.Create<ICouponService>();
        //        var couponInfo = couponService.GetCouponInfo(couponId);
        //        if (couponInfo.EndTime < DateTime.Now) return 2;//已经失效

        //        CouponRecordQuery crQuery = new CouponRecordQuery();
        //        crQuery.CouponId = couponId;
        //        crQuery.UserId = CurrentUser.Id;
        //        ObsoletePageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
        //        if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax) return 3;//达到个人领取最大张数

        //        crQuery = new CouponRecordQuery()
        //        {
        //            CouponId = couponId
        //        };
        //        pageModel = couponService.GetCouponRecordList(crQuery);
        //        if (pageModel.Total >= couponInfo.Num) return 4;//达到领取最大张数

        //        if (couponInfo.ReceiveType == Himall.Model.CouponInfo.CouponReceiveType.IntegralExchange)
        //        {
        //            var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
        //            if (userInte.AvailableIntegrals < couponInfo.NeedIntegral) return 5;//积分不足
        //        }

        //        return 1;//可正常领取
        //    }
        //    return 0;
        //} 
        #endregion
    }
}