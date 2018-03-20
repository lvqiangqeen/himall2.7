using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.IServices;
using Himall.Web.Areas.Mobile.Models;
using Himall.Application;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class CouponController :BaseMobileMemberController
    {
        ICouponService _iCouponService;
            IVShopService _iVShopService;
            IShopService _iShopService;
            IShopBonusService _iShopBonusService;
            IMemberService _iMemberService;
        public CouponController(ICouponService iCouponService,
            IVShopService iVShopService,
            IShopService iShopService,
            IShopBonusService iShopBonusService,
            IMemberService iMemberService 
            )
        {
            _iCouponService = iCouponService;
            _iShopService = iShopService;
            _iVShopService = iVShopService;
            _iShopBonusService = iShopBonusService;
            _iMemberService = iMemberService;

        }

        // GET: Mobile/Coupon
        public ActionResult Get()
        {
            return View();
        }
        
        public ActionResult Management()
        {
            var service = _iCouponService;
            var vshop=_iVShopService;
            var userCouponList = service.GetUserCouponList(CurrentUser.Id);
            var shopBonus = _iShopBonusService.GetDetailByUserId( CurrentUser.Id );
            if( userCouponList == null && shopBonus == null )
            {
                throw new Himall.Core.HimallException( "没有领取记录!" );
            }
            else
            {
                var couponlist = userCouponList.ToArray().Select( a => new UserCouponInfo
                {
                    UserId = a.UserId ,
                    ShopId = a.ShopId ,
                    CouponId = a.CouponId ,
                    Price = a.Price ,
                    PerMax = a.PerMax ,
                    OrderAmount = a.OrderAmount ,
                    Num = a.Num ,
                    StartTime = a.StartTime ,
                    EndTime = a.EndTime ,
                    CreateTime = a.CreateTime ,
                    CouponName = a.CouponName ,
                    UseStatus = a.UseStatus ,
                    UseTime = a.UseTime ,
                    VShop = vshop.GetVShopByShopId( a.ShopId )
                } );

                int NoUseCount = couponlist.Count( item => ( item.EndTime > DateTime.Now && item.UseStatus == CouponRecordInfo.CounponStatuses.Unuse ) );
                int bonusNoUseCount = shopBonus.Count( p => p.State == ShopBonusReceiveInfo.ReceiveState.NotUse && p.Himall_ShopBonusGrant.Himall_ShopBonus.BonusDateEnd > DateTime.Now );
                ViewBag.NoUseCount = NoUseCount + bonusNoUseCount;
                ViewBag.UserCount = ( userCouponList.Count() + shopBonus.Count() ) - ViewBag.NoUseCount;
                ViewBag.ShopBonus = shopBonus;
                return View( couponlist );
               
            }
        }

        [HttpPost]
        public JsonResult AcceptCoupon(long vshopid, long couponid)
        {
            var couponService = _iCouponService;
            var couponInfo = couponService.GetCouponInfo(couponid);
            if (couponInfo.EndTime < DateTime.Now)
            {//已经失效
                return Json(new { status = 2, success = false, msg = "优惠券已经过期." });
            }
            CouponRecordQuery crQuery = new CouponRecordQuery();
            crQuery.CouponId = couponid;
            crQuery.UserId = CurrentUser.Id;
            ObsoletePageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
            if (couponInfo.PerMax!=0&&pageModel.Total >= couponInfo.PerMax)
            {//达到个人领取最大张数
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

        private IEnumerable<CouponInfo> GetCouponList(long shopid)
        {
            var service = _iCouponService;
            var result = service.GetCouponList(shopid);
            var couponSetList = _iVShopService.GetVShopCouponSetting(shopid).Where(a=>a.PlatForm== Core.PlatformType.Wap).Select(item => item.CouponID);
            if (result.Count() > 0 && couponSetList.Count() > 0)
            {
                var couponList = result.ToArray().Where(item => couponSetList.Contains(item.Id));//取设置的优惠券
                return couponList;
            }
            return new List<CouponInfo>();
        }

        public ActionResult ShopCouponList(long shopid)
        {
            var coupons = GetCouponList(shopid);
            VShopInfo vshop = _iVShopService.GetVShopByShopId(shopid);
            if (coupons != null)
            {
                ViewBag.CouponList = coupons.ToArray().Select(a => new UserCouponInfo
                {
                    ShopId = a.ShopId,
                    CouponId = a.Id,
                    Price = a.Price,
                    PerMax = a.PerMax,
                    OrderAmount = a.OrderAmount,
                    Num = a.Num,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    CreateTime = a.CreateTime,
                    CouponName = a.CouponName,
                    VShop = vshop,
                    ReceiveStatus= Receive(a.Id)
                }).Where(p=> p.ReceiveStatus != 2 && p.ReceiveStatus != 4);//优惠券已经过期、优惠券已领完，则不显示在店铺优惠券列表中
            }
            ViewBag.Shopid = shopid;
            ViewBag.VShopid = vshop != null ? vshop.Id : 0;
            var isFav = _iShopService.IsFavoriteShop(CurrentUser.Id, shopid);
            string favText;
            if (isFav)
            {
                favText = "已收藏";
            }
            else
            {
                favText = "收藏店铺";
            }
            ViewBag.FavText = favText;

            WXShopInfo wxinfo = _iVShopService.GetVShopSetting(shopid) ?? new WXShopInfo() { FollowUrl = string.Empty };
            ViewBag.FollowUrl = wxinfo.FollowUrl;
            return View();
        }
        private int Receive(long couponId)
        {
            if (CurrentUser != null && CurrentUser.Id > 0)//未登录不可领取
            {
                var couponService = _iCouponService;
                var couponInfo = couponService.GetCouponInfo(couponId);
                if (couponInfo.EndTime < DateTime.Now) return 2;//已经失效

                CouponRecordQuery crQuery = new CouponRecordQuery();
                crQuery.CouponId = couponId;
                crQuery.UserId = CurrentUser.Id;
                ObsoletePageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
                if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax) return 3;//达到个人领取最大张数

                crQuery = new CouponRecordQuery()
                {
                    CouponId = couponId
                };
                pageModel = couponService.GetCouponRecordList(crQuery);
                if (pageModel.Total >= couponInfo.Num) return 4;//达到领取最大张数

                if (couponInfo.ReceiveType == Himall.Model.CouponInfo.CouponReceiveType.IntegralExchange)
                {
                    var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                    if (userInte.AvailableIntegrals < couponInfo.NeedIntegral) return 5;//积分不足
                }

                return 1;//可正常领取
            }
            return 0;
        }
    }
}