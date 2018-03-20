using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;

using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using Himall.API.Model;
using Himall.API.Model.ParamsModel;
using Himall.CommonModel;
using Himall.Application;

namespace Himall.API
{
    public class CouponController : BaseApiController
    {
        public object GetShopCouponList(long shopId)
        {
            var coupons = GetCouponList(shopId);
            if (coupons != null)
            {
                VShopInfo vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shopId);
                if (vshop == null)
                {
                    return Json(new { Success = "false", ErrorMsg = "该店铺未开通微店，请到PC端去领取" });
                }
                var userCoupon = coupons.Where(d=>d.Himall_CouponSetting.Any(c=>c.PlatForm== Core.PlatformType.Wap)).ToArray().Select(a => new
                {
                    ShopId = a.ShopId,
                    CouponId = a.Id,
                    Price = a.Price,
                    PerMax = a.PerMax,
                    OrderAmount = a.OrderAmount,
                    Num = a.Num,
                    StartTime = a.StartTime.ToString(),
                    EndTime = a.EndTime.ToString(),
                    CreateTime = a.CreateTime.ToString(),
                    CouponName = a.CouponName,
                    //VShopLogo = "http://" + Url.Request.RequestUri.Host + vshop.Logo,
                    VShopLogo = Core.HimallIO.GetRomoteImagePath(vshop.Logo),
                    VShopId = a.Himall_Shops.Himall_VShop.FirstOrDefault() == null ? 0 : a.Himall_Shops.Himall_VShop.FirstOrDefault().Id,
                    ShopName = a.ShopName,
                    Receive = Receive(a.ShopId, a.Id)
                });
                var data = userCoupon.Where(p => p.Receive != 2 && p.Receive != 4);//优惠券已经过期、优惠券已领完，则不显示在店铺优惠券列表中
                return Json(new { Success = "true", Coupon = data });
            }
            else

                return Json(new { Success = "false", ErrorMsg = "该店铺没有可领优惠券" });
        }


        public object GetUserCounponList()
        {
            CheckUserLogin();
            var service = ServiceProvider.Instance<ICouponService>.Create;
            var vshop = ServiceProvider.Instance<IVShopService>.Create;
            var userCouponList = service.GetUserCouponList(CurrentUser.Id);
            var shopBonus = GetBonusList();
            if (userCouponList != null || shopBonus != null)
            {
                //优惠券
                var couponlist = new Object();
                if (userCouponList != null)
                {
                    couponlist = userCouponList.ToArray().Select(a => new
                    {
                        UserId = a.UserId,
                        ShopId = a.ShopId,
                        CouponId = a.CouponId,
                        Price = a.Price,
                        PerMax = a.PerMax,
                        OrderAmount = a.OrderAmount,
                        Num = a.Num,
                        StartTime = a.StartTime.ToString(),
                        EndTime = a.EndTime.ToString(),
                        CreateTime = a.CreateTime.ToString(),
                        CouponName = a.CouponName,
                        UseStatus = a.UseStatus,
                        UseTime = a.UseTime.HasValue ? a.UseTime.ToString() : null,
                        VShop = GetVShop(a.ShopId),
                        ShopName = a.ShopName
                    });
                }
                else
                    couponlist = null;
                //代金红包
                var userBonus = new object();
                if (shopBonus != null)
                {
                    userBonus = shopBonus.ToArray().Select(item =>
                   {
                       var Price = item.Price.Value;
                       var showOrderAmount = item.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice > 0 ? item.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice : item.Price.Value;
                       if (item.Himall_ShopBonusGrant.Himall_ShopBonus.UseState != ShopBonusInfo.UseStateType.FilledSend)
                           showOrderAmount = item.Price.Value;
                       var Logo = string.Empty;
                       long VShopId = 0;
                       if (item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop != null && item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop.Count() > 0)
                       {
                           //Logo ="http://" + Url.Request.RequestUri.Host+ item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop.FirstOrDefault().Logo;
                           Logo = Core.HimallIO.GetRomoteImagePath(item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop.FirstOrDefault().Logo);
                           VShopId = item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop.FirstOrDefault().Id;
                       }

                       var State = (int)item.State;
                       if (item.State != ShopBonusReceiveInfo.ReceiveState.Use && item.Himall_ShopBonusGrant.Himall_ShopBonus.DateEnd < DateTime.Now)
                           State = (int)ShopBonusReceiveInfo.ReceiveState.Expired;
                       var BonusDateEnd = item.Himall_ShopBonusGrant.Himall_ShopBonus.BonusDateEnd.ToString("yyyy-MM-dd");

                       return new
                       {
                           Price = Price,
                           ShowOrderAmount = showOrderAmount,
                           Logo = Logo,
                           VShopId = VShopId,
                           State = State,
                           BonusDateEnd = BonusDateEnd,
                           ShopName = item.BaseShopName
                       };
                   });
                }
                else
                    shopBonus = null;
                //优惠券
                int NoUseCouponCount = 0;
                int UseCouponCount = 0;
                if (userCouponList != null)
                {
                    NoUseCouponCount = userCouponList.Count(item => (item.EndTime > DateTime.Now && item.UseStatus == CouponRecordInfo.CounponStatuses.Unuse));
                    UseCouponCount = userCouponList.Count() - NoUseCouponCount;
                }
                //红包
                int NoUseBonusCount = 0;
                int UseBonusCount = 0;
                if (shopBonus != null)
                {
                    NoUseBonusCount = shopBonus.Count(r => r.State == ShopBonusReceiveInfo.ReceiveState.NotUse && r.Himall_ShopBonusGrant.Himall_ShopBonus.DateEnd > DateTime.Now);
                    UseBonusCount = shopBonus.Count() - NoUseBonusCount;
                }

                int UseCount = UseCouponCount + UseBonusCount;
                int NotUseCount = NoUseCouponCount + NoUseBonusCount;

                return Json(new { Success = "true", NoUseCount = NotUseCount, UserCount = UseCount, Coupon = couponlist, Bonus = userBonus });
            }
            else
            {
                throw new Himall.Core.HimallException("没有领取记录!");
            }
        }

        private object GetVShop(long shopId)
        {
            var vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shopId);
            if (vshop == null)
                return null;
            else
                //return new { VShopId = vshop.Id, VShopLogo = "http://" + Url.Request.RequestUri.Host + vshop.Logo };
                return new { VShopId = vshop.Id, VShopLogo = Core.HimallIO.GetRomoteImagePath(vshop.Logo) };

        }
        //领取优惠券
        public object PostAcceptCoupon(CouponAcceptCouponModel value)
        {
            CheckUserLogin();
            long vshopId = value.vshopId;
            long couponId = value.couponId;
            var couponService = ServiceProvider.Instance<ICouponService>.Create;
            var couponInfo = couponService.GetCouponInfo(couponId);
            if (couponInfo.EndTime < DateTime.Now)
            {//已经失效
                return Json(new { Status = 2, Success = "false", ErrorMsg = "优惠券已经过期." });
            }
            CouponRecordQuery crQuery = new CouponRecordQuery();
            crQuery.CouponId = couponId;
            crQuery.UserId = CurrentUser.Id;
            ObsoletePageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
            if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax)
            {//达到个人领取最大张数
                return Json(new { Status = 3, Success = "false", ErrorMsg = "达到个人领取最大张数，不能再领取." });
            }
            crQuery = new CouponRecordQuery()
            {
                CouponId = couponId
            };
            pageModel = couponService.GetCouponRecordList(crQuery);
            if (pageModel.Total >= couponInfo.Num)
            {//达到领取最大张数
                return Json(new { Status = 4, Success = "false", ErrorMsg = "此优惠券已经领完了." });
            }
            if (couponInfo.ReceiveType == Himall.Model.CouponInfo.CouponReceiveType.IntegralExchange)
            {
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
                if (userInte.AvailableIntegrals < couponInfo.NeedIntegral)
                {
                    //积分不足
                    return Json(new { Status = 5, Success = "false", ErrorMsg = "积分不足 " + couponInfo.NeedIntegral.ToString() });
                }
            }
            CouponRecordInfo couponRecordInfo = new CouponRecordInfo()
            {
                CouponId = couponId,
                UserId = CurrentUser.Id,
                UserName = CurrentUser.UserName,
                ShopId = couponInfo.ShopId
            };
            couponService.AddCouponRecord(couponRecordInfo);
            return Json(new { Status = 1, Success = "true" });//执行成功
        }


        /// <summary>
        /// 取积分优惠券
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public QueryPageModel<CouponGetIntegralCouponModel> GetIntegralCoupon(int page = 1, int pagesize = 10)
        {
            var _iCouponService = ServiceProvider.Instance<ICouponService>.Create;
            IVShopService _iVShopService = ServiceProvider.Instance<IVShopService>.Create;
            ObsoletePageModel<CouponInfo> coupons = _iCouponService.GetIntegralCoupons(page, pagesize);
            Mapper.CreateMap<CouponInfo, CouponGetIntegralCouponModel>();
            QueryPageModel<CouponGetIntegralCouponModel> result = new QueryPageModel<CouponGetIntegralCouponModel>();
            result.Total = coupons.Total;
            if (result.Total > 0)
            {
                var datalist = coupons.Models.ToList();
                var objlist = new List<CouponGetIntegralCouponModel>();
                foreach (var item in datalist)
                {
                    var tmp = Mapper.Map<CouponGetIntegralCouponModel>(item);
                    var vshopobj = _iVShopService.GetVShopByShopId(tmp.ShopId);
                    if (vshopobj != null)
                    {
                        tmp.VShopId = vshopobj.Id;
                        //优惠价封面为空时，取微店Logo，微店Logo为空时，取商城微信Logo
                        if (string.IsNullOrWhiteSpace(tmp.ShowIntegralCover))
                        {
                            if (!string.IsNullOrWhiteSpace(vshopobj.WXLogo))
                            {
                                tmp.ShowIntegralCover = Core.HimallIO.GetRomoteImagePath(vshopobj.WXLogo);
                            }
                            else
                            {
                                var siteset = SiteSettingApplication.GetSiteSettings();
                                tmp.ShowIntegralCover = Core.HimallIO.GetRomoteImagePath(siteset.WXLogo);
                            }
                        }
                    }
                    objlist.Add(tmp);
                }
                result.Models = objlist.ToList();
            }
            return result;
        }
        private IEnumerable<CouponInfo> GetCouponList(long shopid)
        {
            var service = ServiceProvider.Instance<ICouponService>.Create;
            var result = service.GetCouponList(shopid);
            var couponSetList = ServiceProvider.Instance<IVShopService>.Create.GetVShopCouponSetting(shopid).Select(item => item.CouponID);
            if (result.Count() > 0 && couponSetList.Count() > 0)
            {
                var couponList = result.ToArray().Where(item => couponSetList.Contains(item.Id));//取设置的优惠券
                return couponList;
            }
            return null;
        }

        private IEnumerable<ShopBonusReceiveInfo> GetBonusList()
        {
            var service = ServiceProvider.Instance<IShopBonusService>.Create;
            return service.GetDetailByUserId(CurrentUser.Id);
        }
        /// <summary>
        /// 是否可领取优惠券
        /// </summary>
        /// <param name="vshopId"></param>
        /// <param name="couponId"></param>
        /// <returns></returns>
        private int Receive(long vshopId, long couponId)
        {
            if (CurrentUser != null && CurrentUser.Id > 0)//未登录不可领取
            {
                var couponService = ServiceProvider.Instance<ICouponService>.Create;
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
                    var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
                    if (userInte.AvailableIntegrals < couponInfo.NeedIntegral) return 5;//积分不足
                }

                return 1;//可正常领取
            }
            return 0;
        }
    }
}
