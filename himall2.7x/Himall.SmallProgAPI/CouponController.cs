using Himall.Application;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.SmallProgAPI.Helper;
using Himall.SmallProgAPI.Model;

namespace Himall.SmallProgAPI
{
    public class CouponController : BaseApiController
    {
        /// <summary>
        /// 领取优惠券
        /// </summary>
        /// <returns></returns>
        public object GetUserCoupon(string openId, long couponId)
        {
            CheckUserLogin();
            bool status = true;
            string message = "";
            //long vshopId = vspId;// value.vshopId; 店铺Id
            //long couponId = couponId;// value.couponId; 优惠劵Id
            var couponService = ServiceProvider.Instance<ICouponService>.Create;
            var couponInfo = couponService.GetCouponInfo(couponId);
            if (couponInfo.EndTime < DateTime.Now)
            {//已经失效
                status = false;
                message="优惠券已经过期";
            }
            CouponRecordQuery crQuery = new CouponRecordQuery();
            crQuery.CouponId = couponId;
            crQuery.UserId = CurrentUser.Id;
            ObsoletePageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
            if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax)
            {//达到个人领取最大张数
                status = false;
                message="达到领取最大张数";
            }
            crQuery = new CouponRecordQuery()
            {
                CouponId = couponId
            };
            pageModel = couponService.GetCouponRecordList(crQuery);
            if (pageModel.Total >= couponInfo.Num)
            {//达到领取最大张数
                status = false;
                message="此优惠券已经领完了";
            }
            if (couponInfo.ReceiveType == Himall.Model.CouponInfo.CouponReceiveType.IntegralExchange)
            {
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
                if (userInte.AvailableIntegrals < couponInfo.NeedIntegral)
                {
                    //积分不足
                    status = false;
                    message= "积分不足 ";
                }
            }
            if (status)
            {
                CouponRecordInfo couponRecordInfo = new CouponRecordInfo()
                {
                    CouponId = couponId,
                    UserId = CurrentUser.Id,
                    UserName = CurrentUser.UserName,
                    ShopId = couponInfo.ShopId
                };
                couponService.AddCouponRecord(couponRecordInfo);
                return Json(new { Status = "OK", Message = "领取成功" });//执行成功
            }
            else
            {
                return Json(new { Status = "NO", Message = message });
            }
        }
        /// <summary>
        /// 获取用户优惠券列表
        /// </summary>
        /// <returns></returns>
        public object GetLoadCoupon(string openId = "", int pageSize = 10, int pageIndex = 1, int couponType = 1)
        {
            CheckUserLogin();
            CouponRecordQuery query = new CouponRecordQuery();
            query.UserId = CurrentUser.Id;
            query.PageNo = pageIndex;
            query.PageSize = pageSize;
            if (couponType == 1) //（1=未使用 2=已使用 3=已过期）
            {
                query.Status = 0;
            }
            else if (couponType == 2)
            {
                query.Status = 1;
            }
            else
            {
                query.Status = 2;
            }
            
            var model = ServiceProvider.Instance<ICouponService>.Create.GetCouponRecordList(query);
            var couponlist = new Object();
            var json = new object();
            if (model.Models != null)
            {
                //优惠券               
                if (model.Models != null)
                {
                    //CouponType   1=未使用 2=已使用 3=已过期
                    couponlist = model.Models.ToArray().Select(a => new
                    {
                        CouponId = a.Himall_Coupon.Id,
                        RedEnvelopeId = "",
                        CouponName = a.Himall_Coupon.CouponName,                       
                        ClaimCode = a.CounponSN,
                        UserId = a.UserId,
                        UserName = a.UserName,
                        ShopId = a.ShopId,
                        GetDate = a.CounponTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        Price = a.Himall_Coupon.Price,
                        StartTime = a.Himall_Coupon.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        ClosingTime = a.Himall_Coupon.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        //UsedTime = a.UsedTime,
                        //OrderId = a.OrderId,
                        OrderUseLimit =Math.Round(a.Himall_Coupon.OrderAmount, 2),
                        CanUseProducts = "",
                        UseWithGroup = false,//团购
                        UseWithPanicBuying = false,//抢购
                        UseWithFireGroup = false,//拼团
                        RowNumber = 1,
                        //VShop = GetVShop(a.ShopId),
                        ShopName = a.ShopName
                    });
                }
                else
                    couponlist = null;
                //代金红包
                //var userBonus = new object();
                //if (shopBonus != null)
                //{
                //    userBonus = shopBonus.ToArray().Select(item =>
                //    {
                //        var Price = item.Price.Value;
                //        var showOrderAmount = item.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice > 0 ? item.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice : item.Price.Value;
                //        if (item.Himall_ShopBonusGrant.Himall_ShopBonus.UseState != ShopBonusInfo.UseStateType.FilledSend)
                //            showOrderAmount = item.Price.Value;
                //        var Logo = string.Empty;
                //        long VShopId = 0;
                //        if (item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop != null && item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop.Count() > 0)
                //        {
                //            //Logo ="http://" + Url.Request.RequestUri.Host+ item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop.FirstOrDefault().Logo;
                //            Logo = Core.HimallIO.GetRomoteImagePath(item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop.FirstOrDefault().Logo);
                //            VShopId = item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop.FirstOrDefault().Id;
                //        }

                //        var State = (int)item.State;
                //        if (item.State != ShopBonusReceiveInfo.ReceiveState.Use && item.Himall_ShopBonusGrant.Himall_ShopBonus.DateEnd < DateTime.Now)
                //            State = (int)ShopBonusReceiveInfo.ReceiveState.Expired;
                //        var BonusDateEnd = item.Himall_ShopBonusGrant.Himall_ShopBonus.BonusDateEnd.ToString("yyyy-MM-dd");

                //        return new
                //        {
                //            Price = Price,
                //            ShowOrderAmount = showOrderAmount,
                //            Logo = Logo,
                //            VShopId = VShopId,
                //            State = State,
                //            BonusDateEnd = BonusDateEnd,
                //            ShopName = item.BaseShopName
                //        };
                //    });
                //}
                //else
                //    shopBonus = null;
                //优惠券
                //int NoUseCouponCount = 0;//未使用
                //int UseCouponCount = 0;//已使用
                //if (model.Models != null)
                //{
                //    NoUseCouponCount = model.Models.Count(item => (item.Himall_Coupon.EndTime > DateTime.Now && item.UseStatus == CouponRecordInfo.CounponStatuses.Unuse));
                //    UseCouponCount = model.Models.Count() - NoUseCouponCount;
                //}
                //红包
                //int NoUseBonusCount = 0;
                //int UseBonusCount = 0;
                //if (shopBonus != null)
                //{
                //    NoUseBonusCount = shopBonus.Count(r => r.State == ShopBonusReceiveInfo.ReceiveState.NotUse && r.Himall_ShopBonusGrant.Himall_ShopBonus.DateEnd > DateTime.Now);
                //    UseBonusCount = shopBonus.Count() - NoUseBonusCount;
                //}
                //int UseCount = UseCouponCount;// +UseBonusCount;
                //int NotUseCount = NoUseCouponCount;// +NoUseBonusCount;

                json = new
                    {
                        Status = "OK",
                        Data = couponlist
                    };
            }
            else
            {
                json = new
                     {
                         Status = "NO",
                         Data = "[]"
                     };
            }
            return json;
        }

        /// <summary>
        /// 获取系统可领取优惠券列表
        /// </summary>
        /// <returns></returns>
        public object GetLoadSiteCoupon(string openId = "", int pageSize = 10, int pageIndex = 1, int obtainWay = 1)
        {
            CheckUserLogin();
            int status = 0;
            CouponRecordQuery query = new CouponRecordQuery();
            query.UserId = CurrentUser.Id;
            query.PageNo = pageIndex;
            query.PageSize = pageSize;
            if (obtainWay == 1) //（1=未使用 2=已使用 3=已过期）
            {
                query.Status = 0;
            }
            else if (obtainWay == 2)
            {
                query.Status = 1;
            }
            else
            {
                query.Status = obtainWay; 
            }
            var record = ServiceProvider.Instance<ICouponService>.Create.GetCouponRecordList(query);
            var list = record.Models.ToArray().Select(
               item => new
               {
                   CouponId = item.Himall_Coupon.Id,
                   CouponName = item.Himall_Coupon.CouponName,
                   Price = Math.Round(item.Himall_Coupon.Price, 2),
                   SendCount = item.Himall_Coupon.Num,
                   UserLimitCount = item.Himall_Coupon.PerMax,
                   OrderUseLimit = Math.Round(item.Himall_Coupon.OrderAmount, 2),
                   StartTime = item.Himall_Coupon.StartTime.ToString("yyyy.MM.dd"),
                   ClosingTime = item.Himall_Coupon.EndTime.ToString("yyyy.MM.dd"),
                   ObtainWay = item.Himall_Coupon.ReceiveType,
                   NeedPoint = item.Himall_Coupon.NeedIntegral
               });
            var json = new
            {
                Status = "OK",
                total = record.Total,
                Data = list
            };
            return json;
        }
        /// <summary>
        /// 获取优惠券信息
        /// </summary>
        /// <returns></returns>
        public object GetCouponDetail(string openId, int couponId = 0)
        {
            //登录
            CheckUserLogin();
            var jsonStr = new object();
            if (couponId <= 0)
            {
                GetErrorJson("参数错误");
            }
            var couponService = ServiceProvider.Instance<ICouponService>.Create;
            CouponInfo coupon = couponService.GetCouponInfo(couponId);
            if (coupon == null)
            {
                jsonStr = new
                {
                    Status = "NO",
                    Message = "错误的优惠券编号"
                };
            }
            else
            {
                jsonStr = new
                {
                    Status = "OK",
                    Data = new
                    {
                        CouponId = coupon.Id,
                        CouponName = coupon.CouponName,
                        Price = coupon.Price,
                        SendCount = coupon.Num,
                        UserLimitCount = coupon.PerMax,
                        OrderUseLimit = Math.Round(coupon.OrderAmount,2),
                        StartTime = coupon.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        ClosingTime = coupon.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        CanUseProducts = "",
                        ObtainWay = coupon.ReceiveType,
                        NeedPoint = coupon.NeedIntegral,
                        UseWithGroup=false,
                        UseWithPanicBuying=false,
                        UseWithFireGroup=false
                    }
                };

            }
            return jsonStr;
        }
        private IEnumerable<ShopBonusReceiveInfo> GetBonusList()
        {
            var service = ServiceProvider.Instance<IShopBonusService>.Create;

            return service.GetDetailByUserId(CurrentUser.Id);
        }

        private object GetVShop(long shopId)
        {
            var vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shopId);
            if (vshop == null)
                return null;
            else
                return new { VShopId = vshop.Id, VShopLogo = Core.HimallIO.GetRomoteImagePath(vshop.Logo) };

        }

        object GetErrorJson(string errorMsg)
        {
            var message = new
            {
                Status = "false",
                Message = errorMsg
            };
            return message;
        }
    }
}
