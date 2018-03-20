using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class CouponController : BaseSellerController
    {
        private ICouponService _iCouponService;
        private IMarketService _iMarketService;
        private IShopService _iShopService;
        public CouponController(ICouponService iCouponService, IMarketService iMarketService,IShopService iShopService )
        {
            _iCouponService = iCouponService;
            _iMarketService = iMarketService;
            _iShopService = iShopService;
        }
        // GET: SellerAdmin/Coupon
        public ActionResult Management()
        {
            //处理错误同步结果
            _iCouponService.ClearErrorWeiXinCardSync();

            var settings = _iMarketService.GetServiceSetting(MarketType.Coupon);
            if (settings == null)
                return View("Nosetting");

            ViewBag.Market = _iCouponService.GetCouponService(CurrentSellerManager.ShopId);
            return View();
        }

        public JsonResult Cancel(long couponId)
        {
            var shopId = CurrentSellerManager.ShopId;
            _iCouponService.CancelCoupon(couponId, shopId);
            return Json(new Result() { success = true, msg = "操作成功！" });
        }

        #region 添加修改优惠券
        public ActionResult Edit(long Id)
        {
            CouponInfo model = new CouponInfo();
            long shopId = CurrentSellerManager.ShopId;
            var couponser = _iCouponService;
            model = couponser.GetCouponInfo(shopId, Id);
            if (model == null)
            {
                throw new HimallException("错误的优惠券编号。");
            }
            if (model.IsSyncWeiXin == 1 && model.WXAuditStatus != (int)WXCardLogInfo.AuditStatusEnum.Audited)
            {
                throw new HimallException("同步微信优惠券未审核通过时不可修改。");
            }
            model.FormIsSyncWeiXin = model.IsSyncWeiXin == 1;
            
            model.CanVshopIndex = CurrentSellerManager.VShopId > 0;
            ViewBag.EndTime = _iMarketService.GetMarketService(CurrentSellerManager.ShopId, MarketType.Coupon).MarketServiceRecordInfo.Max(item => item.EndTime).ToString("yyyy-MM-dd");
            ViewBag.CanAddIntegralCoupon = couponser.CanAddIntegralCoupon(shopId, model.Id);
            return View(model);
        }
        public ActionResult Add()
        {
            CouponInfo model = new CouponInfo();
            long shopId = CurrentSellerManager.ShopId;
            var couponser = _iCouponService;
            model = new CouponInfo();
            model.StartTime = DateTime.Now;
            model.EndTime = model.StartTime.AddMonths(1);
            model.ReceiveType = CouponInfo.CouponReceiveType.ShopIndex;
           
            model.CanVshopIndex = CurrentSellerManager.VShopId > 0;
            model.Himall_CouponSetting.Clear();
            if (model.CanVshopIndex)
            {
                model.Himall_CouponSetting.Add(new CouponSettingInfo() { Display = 1, PlatForm = Himall.Core.PlatformType.Wap });
            }
            model.Himall_CouponSetting.Add(new CouponSettingInfo() { Display = 1, PlatForm = Himall.Core.PlatformType.PC });
            model.FormIsSyncWeiXin = false;
            model.ShopId = shopId;
            ViewBag.EndTime = _iMarketService.GetMarketService(CurrentSellerManager.ShopId, MarketType.Coupon).MarketServiceRecordInfo.Max(item => item.EndTime).ToString("yyyy-MM-dd");
            ViewBag.CanAddIntegralCoupon = couponser.CanAddIntegralCoupon(shopId);
            return View(model);
        }

        [HttpPost]
        public JsonResult Edit(CouponInfo info)
        {
            bool isAdd = false;
            if (info.Id == 0) isAdd = true;
            var couponser = _iCouponService;
            var shopId = CurrentSellerManager.ShopId;
            info.ShopId = shopId;
            var shopName = _iShopService.GetShop(shopId).ShopName;
            info.ShopName = shopName;

            if (isAdd)
            {
                info.CreateTime = DateTime.Now;
                if (info.StartTime >= info.EndTime)
                {
                    return Json(new Result() { success = false, msg = "开始时间必须小于结束时间" });
                }
                info.IsSyncWeiXin = 0;
                if (info.FormIsSyncWeiXin)
                {
                    info.IsSyncWeiXin = 1;

                    if (string.IsNullOrWhiteSpace(info.FormWXColor))
                    {
                        return Json(new Result() { success = false, msg = "错误的卡券颜色" });
                    }
                    if (string.IsNullOrWhiteSpace(info.FormWXCTit))
                    {
                        return Json(new Result() { success = false, msg = "请填写卡券标题" });
                    }

                    if (!WXCardLogInfo.WXCardColors.Contains(info.FormWXColor))
                    {
                        return Json(new Result() { success = false, msg = "错误的卡券颜色" });
                    }
                    //判断字符长度
                    var enc = System.Text.Encoding.Default;
                    if (enc.GetBytes(info.FormWXCTit).Count() > 18)
                    {
                        return Json(new Result() { success = false, msg = "卡券标题不得超过9个汉字" });
                    }
                    if (!string.IsNullOrWhiteSpace(info.FormWXCSubTit))
                    {
                        if (enc.GetBytes(info.FormWXCSubTit).Count() > 36)
                        {
                            return Json(new Result() { success = false, msg = "卡券副标题不得超过18个汉字" });
                        }
                    }
                }
            }
            var couponsetting = Request.Form["chkShow"];

            info.CanVshopIndex = CurrentSellerManager.VShopId > 0;

            switch (info.ReceiveType)
            {
                case Himall.Model.CouponInfo.CouponReceiveType.IntegralExchange:
                    if (!couponser.CanAddIntegralCoupon(shopId, info.Id))
                    {
                        return Json(new Result() { success = false, msg = "当前已有积分优惠券，每商家只可以推广一张积分优惠券！" });
                    }
                    info.Himall_CouponSetting.Clear();
                    if (info.EndIntegralExchange == null)
                    {
                        return Json(new Result() { success = false, msg = "错误的兑换截止时间" });
                    }
                    if (info.EndIntegralExchange > info.EndTime.AddDays(1).Date)
                    {
                        return Json(new Result() { success = false, msg = "错误的兑换截止时间" });
                    }
                    if (info.NeedIntegral < 10)
                    {
                        return Json(new Result() { success = false, msg = "积分最少10分起兑" });
                    }
                    break;
                case Himall.Model.CouponInfo.CouponReceiveType.DirectHair:
                    info.Himall_CouponSetting.Clear();
                    break;
                default:
                    if (!string.IsNullOrEmpty(couponsetting))
                    {
                        info.Himall_CouponSetting.Clear();
                        var t = couponsetting.Split(',');
                        if (t.Contains("WAP"))
                        {
                            //if (info.CanVshopIndex)
                            //{
                                info.Himall_CouponSetting.Add(new CouponSettingInfo() { Display = 1, PlatForm = Himall.Core.PlatformType.Wap });
                            //}
                        }
                        if (t.Contains("PC"))
                        {
                            info.Himall_CouponSetting.Add(new CouponSettingInfo() { Display = 1, PlatForm = Himall.Core.PlatformType.PC });
                        }
                    }
                    else
                    {
                        return Json(new Result() { success = false, msg = "必须选择一个推广类型" });
                    }
                    break;
            }

            #region 转移图片
            string path = Server.MapPath(string.Format(@"/Storage/Shop/{0}/Coupon/{1}", shopId, info.Id));
            #endregion

            if (isAdd)
            {
                couponser.AddCoupon(info);
            }
            else
            {
                couponser.EditCoupon(info);
            }
            return Json(new { success = true });
        }
        #endregion

        public ActionResult Receivers(long Id)
        {
            ViewBag.Id = Id;
            return View();
        }

        public ActionResult Detail(long Id)
        {
            var model = _iCouponService.GetCouponInfo(CurrentSellerManager.ShopId, Id);
            if (model != null)
            {
                if (model.IsSyncWeiXin == 1 && model.WXAuditStatus != (int)WXCardLogInfo.AuditStatusEnum.Audited)
                {
                    throw new HimallException("同步微信优惠券未审核通过时不可修改。");
                }
            }
            string host = Request.Url.Host;
            host += Request.Url.Port != 80 ? ":" + Request.Url.Port.ToString() : "";
            ViewBag.Url = String.Format("http://{0}/m-wap/vshop/CouponInfo/{1}", host, model.Id);
            var map = Core.Helper.QRCodeHelper.Create(ViewBag.Url);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            //  显示  
            ViewBag.Image = strUrl;
            ViewBag.EndTime = _iMarketService.GetMarketService(CurrentSellerManager.ShopId, MarketType.Coupon).MarketServiceRecordInfo.Max(item => item.EndTime).ToString("yyyy-MM-dd");
            return View(model);
        }


        [HttpPost]
        public ActionResult GetReceivers(long Id, int page, int rows)
        {
            CouponRecordQuery query = new CouponRecordQuery();
            query.CouponId = Id;
            query.ShopId = CurrentSellerManager.ShopId;
            query.PageNo = page;
            query.PageSize = rows;
            var record = _iCouponService.GetCouponRecordList(query);
            var list = record.Models.ToArray().Select(
               item => new
               {
                   Id = item.Id,
                   Price = Math.Round(item.Himall_Coupon.Price, 2),
                   CreateTime = item.Himall_Coupon.CreateTime.ToString("yyyy-MM-dd"),
                   CouponSN = item.CounponSN,
                   UsedTime = item.UsedTime.HasValue ? item.UsedTime.Value.ToString("yyyy-MM-dd") : "",
                   ReceviceTime = item.CounponTime.ToString("yyyy-MM-dd"),
                   Recever = item.UserName,
                   OrderId = item.OrderId,
                   Status = item.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse ? (item.Himall_Coupon.EndTime < DateTime.Now.Date ? "已过期" : item.CounponStatus.ToDescription()) : item.CounponStatus.ToDescription(),
               }
               );
            var model = new { rows = list, total = record.Total };
            return Json(model);
        }

        public ActionResult BuyService()
        {
            var market = _iCouponService.GetCouponService(CurrentSellerManager.ShopId);
            ViewBag.Market = market;
            string endDate = null;
            var now = DateTime.Now.Date;
            if (market != null && market.MarketServiceRecordInfo.Max(item => item.EndTime) < now)
            {
                endDate = "您的优惠券服务已经过期，您可以续费。";
            }
            else if (market != null && market.MarketServiceRecordInfo.Max(item => item.EndTime) >= now)
            {
                var maxtime = market.MarketServiceRecordInfo.Max(item => item.EndTime);
                endDate = string.Format("{0} 年 {1} 月 {2} 日", maxtime.Year, maxtime.Month, maxtime.Day);
            }
            ViewBag.EndDate = endDate;
            ViewBag.Price = _iMarketService.GetServiceSetting(MarketType.Coupon).Price;
            return View();
        }
        [HttpPost]
        [UnAuthorize]
        public JsonResult BuyService(int month)
        {
            Result result = new Result();
            var service = _iMarketService;
            service.OrderMarketService(month, CurrentSellerManager.ShopId, MarketType.Coupon);
            result.success = true;
            result.msg = "购买服务成功";
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetItemList(int page, int rows, string couponName)
        {
            var service = _iCouponService;
            var result = service.GetCouponList(new CouponQuery { CouponName = couponName, ShopId = CurrentSellerManager.ShopId, IsShowAll = true, PageSize = rows, PageNo = page });
            var list = result.Models.ToList().Select(
                item =>
                {
                    int Status = 0;
                    if (item.StartTime <= DateTime.Now && item.EndTime > DateTime.Now)
                        Status = 2;
                    else
                        if (item.StartTime > DateTime.Now)
                            Status = 1;
                        else
                            Status = 0;

                    return new
                        {
                            Id = item.Id,
                            StartTime = item.StartTime.ToString("yyyy/MM/dd"),
                            EndTime = item.EndTime.ToString("yyyy/MM/dd"),
                            Price = Math.Round(item.Price, 2),
                            CouponName = item.CouponName,
                            PerMax = item.PerMax == 0 ? "不限张" : item.PerMax.ToString() + "张/人",
                            OrderAmount = item.OrderAmount == 0 ? "不限制" : "满" + item.OrderAmount + "使用",
                            Num = item.Num,
                            ReceviceNum = item.Himall_CouponRecord.Count(),
                            RecevicePeople = item.Himall_CouponRecord.GroupBy(a => a.UserId).Count(),
                            Used = item.Himall_CouponRecord.Count(a => a.CounponStatus == CouponRecordInfo.CounponStatuses.Used),
                            IsSyncWeiXin = item.IsSyncWeiXin,
                            WXAuditStatus = (item.IsSyncWeiXin != 1 ? (int)WXCardLogInfo.AuditStatusEnum.Audited : item.WXAuditStatus),
                            Status = Status,
                            CreateTime = item.CreateTime
                        };
                }
                ).OrderByDescending(r => r.Status).ThenByDescending(r => r.CreateTime);
            var model = new { rows = list, total = result.Total };
            return Json(model);
        }
    }
}