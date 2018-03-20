using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Himall.Web;
using Himall.Model;
using Himall.IServices;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;

namespace Himall.Web.Areas.Web.Controllers
{
    public class SideController : BaseWebController
    {
        private IMemberIntegralService _iMemberIntegralService;
        private IProductService _iProductService;
        private IShopBonusService _iShopBonusService;
        private ICouponService _iCouponService;
        public SideController(IMemberIntegralService iMemberIntegralService,
            IProductService iProductService,
            IShopBonusService iShopBonusService,
            ICouponService iCouponService
            )
        {

            _iMemberIntegralService = iMemberIntegralService;
            _iProductService = iProductService;
            _iShopBonusService = iShopBonusService;
            _iCouponService = iCouponService;
        }

        /// <summary>
        /// 侧边我的资产
        /// </summary>
        /// <returns></returns>
        public ActionResult MyAsset()
        {
            MyAssetViewModel result = new Models.MyAssetViewModel();
            result.MyCouponCount = 0;
            result.isLogin = CurrentUser != null;
            ViewBag.isLogin = result.isLogin ? "true" : "false";
            //用户积分
            result.MyMemberIntegral = result.isLogin ? _iMemberIntegralService.GetMemberIntegral(CurrentUser.Id).AvailableIntegrals : 0;
            //关注商品
            var concern = result.isLogin ? _iProductService.GetUserAllConcern(CurrentUser.Id) : new List<FavoriteInfo>();
            result.MyConcernsProducts = concern.Take(10).ToList();
            //优惠卷
            var coupons = result.isLogin ?_iCouponService.GetAllUserCoupon(CurrentUser.Id).ToList() : new List<UserCouponInfo>();
            coupons = coupons == null ? new List<UserCouponInfo>() : coupons;
            result.MyCoupons = coupons;
            result.MyCouponCount += result.MyCoupons.Count();

            //红包
            var shopBonus = result.isLogin ? _iShopBonusService.GetCanUseDetailByUserId(CurrentUser.Id) : new List<ShopBonusReceiveInfo>();
            shopBonus = shopBonus == null ? new List<ShopBonusReceiveInfo>() : shopBonus;
            result.MyShopBonus = shopBonus;
            result.MyCouponCount += result.MyShopBonus.Count();

            //浏览的商品
            var browsingPro = result.isLogin ? BrowseHistrory.GetBrowsingProducts(10, CurrentUser == null ? 0 : CurrentUser.Id) : new List<ProductBrowsedHistoryModel>();
            result.MyBrowsingProducts = browsingPro;
            return View(result);
        }
    }
}