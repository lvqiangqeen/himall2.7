using Himall.Core;
using System.Linq.Expressions;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using MySql.Data.MySqlClient;
using Dapper;

namespace Himall.Service
{
    public class CouponService : ServiceBase, ICouponService
    {
        /// <summary>
        /// 优惠券类型
        /// </summary>
        private WXCardLogInfo.CouponTypeEnum ThisCouponType = WXCardLogInfo.CouponTypeEnum.Coupon;
        /// <summary>
        /// 微信卡券服务
        /// </summary>
        private IWXCardService ser_wxcard;
        public CouponService()
        {
            ser_wxcard = Himall.ServiceProvider.Instance<IWXCardService>.Create;
        }
        public CouponRecordInfo AddCouponRecord(CouponRecordInfo info)
        {
            var shopName = Context.ShopInfo.FindById(info.ShopId).ShopName;
            var coupondata = Context.CouponInfo.FirstOrDefault(d => d.Id == info.CouponId);

            if (coupondata.IsSyncWeiXin == 1 && coupondata.WXAuditStatus != (int)WXCardLogInfo.AuditStatusEnum.Audited)
            {
                throw new HimallException("优惠券状态错误，不可领取");
            }

            //扣积分
            if (coupondata.ReceiveType == Himall.Model.CouponInfo.CouponReceiveType.IntegralExchange)
            {
                MemberIntegralRecord mirinfo = new MemberIntegralRecord();
                mirinfo.UserName = info.UserName;
                mirinfo.MemberId = info.UserId;
                mirinfo.RecordDate = DateTime.Now;
                mirinfo.TypeId = MemberIntegral.IntegralType.Exchange;
                mirinfo.Integral = coupondata.NeedIntegral;
                mirinfo.ReMark = "兑换优惠券:面值" + coupondata.Price.ToString("f2");
                var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Exchange, mirinfo.Integral);
                ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(mirinfo, memberIntegral);
            }

            info.CounponStatus = CouponRecordInfo.CounponStatuses.Unuse;
            info.CounponSN = Guid.NewGuid().ToString().Replace("-", "");
            info.UsedTime = null;
            info.CounponTime = DateTime.Now;
            info.ShopName = shopName;
            info.OrderId = null;
            var item = Context.CouponRecordInfo.Add(info);
            Context.SaveChanges();
            return item;
        }
        public void AddSendmessagerecordCouponSN(List<SendmessagerecordCouponSNInfo> items)
        {
            if (items != null && items.Count > 0)
            {
                Context.SendmessagerecordCouponSNInfo.AddRange(items);
                Context.SaveChanges();
            }
        }
        public void UseCoupon(long userId, IEnumerable<long> Ids, IEnumerable<OrderInfo> orders)
        {
            var date = DateTime.Now.Date;
            var coupons = Context.CouponRecordInfo.Include("Himall_Coupon").Where(a => a.UserId == userId && Ids.Contains(a.Id) && a.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse
                        && a.Himall_Coupon.EndTime > date).DistinctBy(a => a.ShopId).ToList();
            //微信卡券操作
            foreach (var info in coupons)
            {
                info.CounponStatus = CouponRecordInfo.CounponStatuses.Used;
                info.UsedTime = DateTime.Now;
                info.OrderId = orders.FirstOrDefault(a => a.ShopId == info.ShopId && a.ProductTotalAmount >= info.Himall_Coupon.OrderAmount).Id;
                ser_wxcard.Consume(info.Id, ThisCouponType);
            }
            Context.SaveChanges();
        }

        public ActiveMarketServiceInfo GetCouponService(long shopId)
        {
            if (shopId <= 0)
            {
                throw new HimallException("ShopId不能识别");
            }
            var market = Context.ActiveMarketServiceInfo.FirstOrDefault(m => m.ShopId == shopId && m.TypeId == MarketType.Coupon);
            return market;
        }


        public ObsoletePageModel<CouponInfo> GetCouponList(CouponQuery query)
        {
            if (query.ShopId.HasValue)
            {
                if (query.ShopId <= 0)
                {
                    throw new HimallException("ShopId不能识别");
                }
            }
            int total = 0;
            int auditsuccess = (int)WXCardLogInfo.AuditStatusEnum.Audited;
            IQueryable<CouponInfo> coupon = Context.CouponInfo.AsQueryable();
            if (query.ShopId.HasValue)
            {
                coupon = coupon.Where(d => d.ShopId == query.ShopId);
            }
            if (query.IsShowAll != true)
            {
                coupon = coupon.Where(d => d.WXAuditStatus == auditsuccess);
            }
            if (query.ShowPlatform.HasValue)
            {
                coupon = coupon.Where(d => d.Himall_CouponSetting.Any(s => s.PlatForm == query.ShowPlatform.Value));
            }
            if (query.IsOnlyShowNormal == true)
            {
                DateTime curMindate = DateTime.Now.Date;
                DateTime curMaxdate = curMindate.AddDays(1).Date;
                coupon = coupon.Where(d => d.EndTime >= curMaxdate && d.StartTime <= curMindate);
            }
            if (!string.IsNullOrWhiteSpace(query.CouponName))
            {
                coupon = coupon.Where(d => d.CouponName.Contains(query.CouponName));
            }
            coupon = coupon.GetPage(out total, d => d.OrderByDescending(o => o.EndTime), query.PageNo, query.PageSize);
            ObsoletePageModel<CouponInfo> pageModel = new ObsoletePageModel<CouponInfo>() { Models = coupon, Total = total };
            return pageModel;
        }
        public IQueryable<CouponInfo> GetCouponList(long shopid)
        {
            int auditsuccess = (int)WXCardLogInfo.AuditStatusEnum.Audited;
            return Context.CouponInfo.Where(item => item.ShopId == shopid && item.WXAuditStatus == auditsuccess);
        }
        /// <summary>
        /// 取用户领取的所有优惠卷信息
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public IQueryable<UserCouponInfo> GetUserCouponList(long userid)
        {
            IQueryable<UserCouponInfo> list = Context.CouponInfo.Join(Context.CouponRecordInfo.Where(item => item.UserId == userid), a => a.Id, b => b.CouponId, (a, b) => new UserCouponInfo
            {
                UserId = b.UserId,
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
                UseStatus = b.CounponStatus,
                UseTime = b.UsedTime,
                ShopName = a.ShopName
            });
            return list;
        }


        public List<UserCouponInfo> GetAllUserCoupon(long userid)
        {
            var date = DateTime.Now;

            var query = Context.CouponRecordInfo.Where(item => item.UserId == userid &&
                item.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse
                && item.Himall_Coupon.EndTime > date);
            var list = query.Select(b => new UserCouponInfo()
            {
                UserId = b.UserId,
                ShopId = b.ShopId,
                CouponId = b.CouponId,
                Price = b.Himall_Coupon.Price,//
                PerMax = b.Himall_Coupon.PerMax,
                OrderAmount = b.Himall_Coupon.OrderAmount,//
                Num = b.Himall_Coupon.Num,
                StartTime = b.Himall_Coupon.StartTime,
                EndTime = b.Himall_Coupon.EndTime,
                ShopName = b.Himall_Coupon.ShopName,
                CreateTime = b.Himall_Coupon.CreateTime,
                CouponName = b.Himall_Coupon.CouponName,
                UseStatus = b.CounponStatus,
                UseTime = b.UsedTime
            }).ToList();

            return list;

            //IQueryable<UserCouponInfo> list = context.CouponInfo.Join(context.CouponRecordInfo.Where(item => item.UserId == userid &&item.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse), a => a.Id, b => b.CouponId, (a, b) => new UserCouponInfo
            //{
            //    UserId = b.UserId,
            //    ShopId = a.ShopId,
            //    CouponId = a.Id,
            //    Price = a.Price,//
            //    PerMax = a.PerMax,
            //    OrderAmount = a.OrderAmount,//
            //    Num = a.Num,
            //    StartTime = a.StartTime,
            //    EndTime = a.EndTime,
            //    ShopName=a.ShopName,
            //    CreateTime = a.CreateTime,
            //    CouponName = a.CouponName,
            //    UseStatus = b.CounponStatus,
            //    UseTime = b.UsedTime
            //});
            //return list.ToList();
        }
        public List<CouponRecordInfo> GetAllCoupon(long userid)
        {
            return Context.CouponRecordInfo.Where(p => p.UserId == userid).ToList();
        }

        public ObsoletePageModel<CouponRecordInfo> GetCouponRecordList(CouponRecordQuery query)
        {
            int total = 0;
            var date = DateTime.Now;
            IQueryable<CouponRecordInfo> coupons = Context.CouponRecordInfo.AsQueryable();
            if (query.CouponId.HasValue)
            {
                coupons = coupons.Where(d => d.CouponId == query.CouponId);
            }
            if (query.UserId.HasValue)
            {
                coupons = coupons.Where(d => d.UserId == query.UserId.Value);
            }
            if (query.ShopId.HasValue)
            {
                coupons = coupons.Where(d => d.ShopId == query.ShopId.Value);
            }
            if (!string.IsNullOrWhiteSpace(query.UserName))
            {
                coupons = coupons.Where(d => d.UserName.Contains(query.UserName));
            }

            switch (query.Status)
            {
                case 0:
                    coupons = coupons.Where(item => item.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse && item.Himall_Coupon.EndTime > date);
                    break;
                case 1:
                    coupons = coupons.Where(item => item.CounponStatus == CouponRecordInfo.CounponStatuses.Used);
                    break;
                case 2:
                    coupons = coupons.Where(item => item.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse && item.Himall_Coupon.EndTime <= date);
                    break;
            }
            coupons = coupons.GetPage(out total, query.PageNo, query.PageSize);
            ObsoletePageModel<CouponRecordInfo> pageModel = new ObsoletePageModel<CouponRecordInfo>() { Models = coupons, Total = total };
            return pageModel;
        }

        public CouponRecordInfo GetCouponRecordById(long id)
        {
            CouponRecordInfo result = Context.CouponRecordInfo.FirstOrDefault(d => d.Id == id);
            //数据补充
            if (result.WXCodeId.HasValue)
            {
                result.WXCardCodeInfo = Context.WXCardCodeLogInfo.FirstOrDefault(d => d.Id == result.WXCodeId.Value);
            }
            return result;
        }


        public CouponInfo GetCouponInfo(long shopId, long couponId)
        {
            CouponInfo result = Context.CouponInfo.FirstOrDefault(a => a.ShopId == shopId && a.Id == couponId);
            if (result != null)
            {
                if (result.IsSyncWeiXin == 1)
                {
                    result.WXCardInfo = Context.WXCardLogInfo.FirstOrDefault(a => a.Id == result.CardLogId);
                }
            }
            return result;
        }

        public CouponInfo GetCouponInfo(long couponId)
        {
            CouponInfo result = Context.CouponInfo.FirstOrDefault(a => a.Id == couponId);
            if (result != null)
            {
                if (result.IsSyncWeiXin == 1)
                {
                    result.WXCardInfo = Context.WXCardLogInfo.FirstOrDefault(a => a.Id == result.CardLogId);
                }
            }
            return result;
        }

        public List<CouponInfo> GetCouponInfo(long[] couponId)
        {
            List<CouponInfo> result = Context.CouponInfo.Where(a => couponId.Contains(a.Id)).ToList();
            return result;
        }

        /// <summary>
        /// 添加或修改条件判断
        /// </summary>
        /// <param name="info"></param>
        private void CanAddOrEditCoupon(CouponInfo info)
        {
            var ids = Context.CouponInfo.Where(a => a.EndTime > DateTime.Now && a.ShopId == info.ShopId && a.ReceiveType != CouponInfo.CouponReceiveType.IntegralExchange && a.ReceiveType != CouponInfo.CouponReceiveType.DirectHair).Select(a => a.Id).ToList();
            var setting = Context.CouponSettingInfo.Where(a => ids.Contains(a.CouponID)).ToList();
            if (info.Himall_CouponSetting != null && info.Himall_CouponSetting.Count > 0)
            {
                int maxnum = 5;
                if (setting.Count(a => a.PlatForm == Core.PlatformType.Wap) >= maxnum && !ids.Any(d => d == info.Id) && info.Himall_CouponSetting.Any(d => d.PlatForm == PlatformType.Wap))
                {
                    throw new HimallException("最多添加5个移动端优惠券");
                }
                if (setting.Count(a => a.PlatForm == Core.PlatformType.PC) >= maxnum && !ids.Any(d => d == info.Id) && info.Himall_CouponSetting.Any(d => d.PlatForm == PlatformType.PC))
                {
                    throw new HimallException("最多添加5个PC端个优惠券");
                }
            }

        }

        public void AddCoupon(CouponInfo info)
        {
            CanAddOrEditCoupon(info);
            var coupon = Context.ActiveMarketServiceInfo.FirstOrDefault(a => a.TypeId == MarketType.Coupon && a.ShopId == info.ShopId);
            if (coupon == null)
            {
                throw new HimallException("您没有订购此服务");
            }
            if (coupon.MarketServiceRecordInfo.Max(item => item.EndTime.Date) < info.EndTime)
            {
                throw new HimallException("结束日期已经超过购买优惠券服务的日期");
            }
            info.WXAuditStatus = 1;
            if (info.IsSyncWeiXin == 1)
            {
                info.WXAuditStatus = 0;
            }
            Context.CouponInfo.Add(info);
            Context.SaveChanges();

            #region 同步微信
            if (info.IsSyncWeiXin == 1)
            {
                WXCardLogInfo wxdata = new WXCardLogInfo();
                wxdata.CardColor = info.FormWXColor;
                wxdata.CardTitle = info.FormWXCTit;
                wxdata.CardSubTitle = info.FormWXCSubTit;
                wxdata.CouponType = ThisCouponType;
                wxdata.CouponId = info.Id;
                wxdata.ShopId = info.ShopId;
                wxdata.ShopName = info.ShopName;
                wxdata.ReduceCost = (int)(info.Price * 100);
                wxdata.LeastCost = (int)(info.OrderAmount * 100);
                wxdata.Quantity = info.Num;
                wxdata.GetLimit = info.PerMax;
                wxdata.DefaultDetail = info.Price.ToString("F2") + "元优惠券1张";
                wxdata.BeginTime = info.StartTime.Date;
                wxdata.EndTime = info.EndTime.AddDays(1).AddMinutes(-1);
                if (ser_wxcard.Add(wxdata))
                {
                    info.CardLogId = wxdata.Id;
                    Context.SaveChanges();
                }
                else
                {
                    Context.CouponInfo.Remove(info);
                    Context.SaveChanges();
                    throw new HimallException("同步微信卡券失败，请检查参数是否有错！");
                }
            }
            #endregion

            SaveCover(info);
        }

        public void EditCoupon(CouponInfo info)
        {
            CanAddOrEditCoupon(info);
            var model = Context.CouponInfo.FirstOrDefault(a => a.ShopId == info.ShopId && a.Id == info.Id);
            if (model == null)
            {
                throw new HimallException("错误的优惠券信息");
            }
            //计算库存量
            int nqnum = Context.CouponRecordInfo.Count(d => d.CouponId == model.Id);
            if (info.Num < nqnum)
            {
                throw new HimallException("错误的发放总量，已领取数已超过发放总量");
            }
            Context.CouponSettingInfo.RemoveRange(model.Himall_CouponSetting);
            if (info.Himall_CouponSetting != null && info.Himall_CouponSetting.Count > 0)
            {
                model.Himall_CouponSetting = info.Himall_CouponSetting;
            }
            model.CouponName = info.CouponName;
            model.PerMax = info.PerMax;
            model.Num = info.Num;
            model.ReceiveType = info.ReceiveType;
            model.EndIntegralExchange = info.EndIntegralExchange;
            model.NeedIntegral = info.NeedIntegral;
            model.IntegralCover = info.IntegralCover;
            //model.EndIntegralExchange = info.EndIntegralExchange;
            if (model.IsSyncWeiXin == 1 && model.CardLogId.HasValue)
            {
                var carddata = Context.WXCardLogInfo.FirstOrDefault(d => d.Id == model.CardLogId.Value);
                if (carddata != null)
                {
                    int wxstock = model.Num - nqnum;
                    //同步微信限领
                    ser_wxcard.EditGetLimit(model.PerMax, carddata.CardId);
                    //同步微信库存
                    ser_wxcard.EditStock(wxstock, carddata.CardId);
                }
            }
            Context.SaveChanges();
            SaveCover(model);
        }


        public IEnumerable<CouponInfo> GetTopCoupon(long shopId, int top = 8, PlatformType type = Core.PlatformType.PC)
        {
            var Date = DateTime.Now;
            int auditsuccess = (int)WXCardLogInfo.AuditStatusEnum.Audited;
            return Context.CouponInfo.Where(a => a.ShopId == shopId && a.EndTime >= Date && a.WXAuditStatus == auditsuccess && a.Himall_CouponSetting.Any(b => b.PlatForm == type)).OrderByDescending(a => a.Price).Take(top);
        }

        //使某个优惠券失效
        public void CancelCoupon(long couponId, long shopId)
        {
            var coupon = Context.CouponInfo.FirstOrDefault(a => a.ShopId == shopId && a.Id == couponId);
            if (coupon == null)
            {
                throw new HimallException("找不到相对应的优惠券！");
            }
            coupon.EndTime = DateTime.Now.Date.AddDays(-1);
            Context.SaveChanges();
            //同步微信
            if (coupon.IsSyncWeiXin == 1 && coupon.CardLogId != null)
            {
                ser_wxcard.Delete(coupon.CardLogId.Value);
                //TODO:DZY 处理过期投放??
            }
        }

        public int GetUserReceiveCoupon(long couponId, long userId)
        {
            return Context.CouponRecordInfo.Count(a => a.CouponId == couponId && a.UserId == userId);
        }


        public List<CouponRecordInfo> GetUserCoupon(long shopId, long userId, decimal totalPrice)
        {
            var date = DateTime.Now;
            return Context.CouponRecordInfo.Where(item => item.ShopId == shopId
                 && item.UserId == userId
                 && item.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse
                 && item.Himall_Coupon.StartTime <= date
                 && item.Himall_Coupon.EndTime > date
                 && item.Himall_Coupon.OrderAmount <= totalPrice).OrderByDescending(item => item.Himall_Coupon.Price).ToList();
            // item.Himall_Coupon.Price < totalPrice
            //不需要大于优惠券的价格
        }


        public IEnumerable<CouponRecordInfo> GetOrderCoupons(long userId, IEnumerable<long> Ids)
        {
            var date = DateTime.Now;
            return Context.CouponRecordInfo.Where(a => a.UserId == userId && Ids.Contains(a.Id) && a.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse &&
                a.Himall_Coupon.StartTime <= date && a.Himall_Coupon.EndTime > date).DistinctBy(a => a.ShopId);
        }


        public CouponRecordInfo GetCouponRecordInfo(long userId, long orderId)
        {
            CouponRecordInfo result = Context.CouponRecordInfo.FirstOrDefault(a => a.UserId == userId && a.OrderId == orderId);
            if (result != null)
            {
                //数据补充
                if (result.WXCodeId.HasValue)
                {
                    result.WXCardCodeInfo = Context.WXCardCodeLogInfo.FirstOrDefault(d => d.Id == result.WXCodeId.Value);
                }
            }
            return result;
        }
        /// <summary>
        /// 是否可以添加积分兑换红包
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public bool CanAddIntegralCoupon(long shopid, long id = 0)
        {
            bool result = false;
            DateTime CurDay = DateTime.Now.Date;
            DateTime CurTime = DateTime.Now;
            var sql = Context.CouponInfo.Where(d => d.ShopId == shopid && d.ReceiveType == CouponInfo.CouponReceiveType.IntegralExchange && d.EndIntegralExchange >= CurTime && d.EndTime >= CurDay);
            if (id > 0)
            {
                sql = sql.Where(d => d.Id != id);
            }
            result = sql.Count() < 1;
            return result;
        }
        /// <summary>
        /// 取积分优惠券列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ObsoletePageModel<CouponInfo> GetIntegralCoupons(int page, int pageSize)
        {
            ObsoletePageModel<CouponInfo> result = new ObsoletePageModel<CouponInfo>();
            DateTime CurDay = DateTime.Now;
            DateTime CurTime = DateTime.Now;
            int auditsuccess = (int)WXCardLogInfo.AuditStatusEnum.Audited;
            var sql = Context.CouponInfo.Where(d => d.ReceiveType == CouponInfo.CouponReceiveType.IntegralExchange
            && d.EndIntegralExchange >= CurTime && d.EndTime >= CurDay && d.StartTime <= CurDay
            && d.WXAuditStatus == auditsuccess && d.Num > 0);
            int total = 0;
            sql = sql.GetPage(out total, page, pageSize, d => d.OrderByDescending(o => o.CreateTime));
            result.Models = sql;
            result.Total = total;
            return result;
        }

        /// <summary>
        /// 同步微信卡券审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cardid"></param>
        /// <param name="auditstatus">审核状态</param>
        public void SyncWeixinCardAudit(long id, string cardid, WXCardLogInfo.AuditStatusEnum auditstatus)
        {
            var coupon = Context.CouponInfo.FirstOrDefault(d => d.Id == id);
            if (coupon != null)
            {
                coupon.WXAuditStatus = (int)auditstatus;
                Context.SaveChanges();
            }
        }

        /// <summary>
        /// 处理错误的卡券同步信息
        /// </summary>
        public void ClearErrorWeiXinCardSync()
        {
            DateTime overtime = DateTime.Now.AddDays(-2).Date;
            int wxaudstate = (int)WXCardLogInfo.AuditStatusEnum.Auditin;
            var datalist = Context.CouponInfo.Where(d => d.CreateTime < overtime && d.IsSyncWeiXin == 1 && d.WXAuditStatus == wxaudstate).ToList();
            if (datalist.Count > 0)
            {
                List<long?> cardids = datalist.Select(d => d.CardLogId).ToList();
                var cardlist = Context.WXCardLogInfo.Where(d => cardids.Contains(d.Id)).ToList();
                if (cardlist.Count > 0)
                {
                    Context.WXCardLogInfo.RemoveRange(cardlist);
                }
                foreach (var item in datalist)
                {
                    item.WXAuditStatus = (int)WXCardLogInfo.AuditStatusEnum.AuditNot;
                    item.EndTime = DateTime.Now.AddDays(-1);
                }
                Context.SaveChanges();
            }
        }

        /// <summary>
        /// 转移封面图片
        /// </summary>
        /// <param name="model"></param>
        public void SaveCover(CouponInfo model)
        {
            string image = model.IntegralCover;
            string path = string.Format(@"/Storage/Shop/{0}/Coupon/", model.ShopId);
            var ext = ".png";
            string filename = model.Id.ToString() + ext;
            var savepath = Path.Combine(path, filename);
            if (image != null && image.Contains("/temp/"))
            {
                string temp = image.Substring(image.LastIndexOf("/temp"));
                Core.HimallIO.CopyFile(temp, savepath, true);
                model.IntegralCover = savepath;
                Context.SaveChanges();
            }
        }



        public ObsoletePageModel<CouponInfo> GetCouponByName(string text, DateTime endDate, int ReceiveType, int page, int pageSize)
        {

            int total = 0;
            IQueryable<CouponInfo> coupon = Context.CouponInfo.AsQueryable();

            if (!text.Equals(""))
                coupon = coupon.Where(d => d.CouponName.Contains(text));

            if (endDate > DateTime.Parse("2000-01-01"))
                coupon = coupon.Where(d => d.EndTime > endDate);
            else
                coupon = coupon.Where(d => d.EndTime > DateTime.Now);//查询未结束

            if (ReceiveType >= 0)
                coupon = coupon.Where(d => d.ReceiveType == (Model.CouponInfo.CouponReceiveType)ReceiveType);

            coupon = coupon.Where(d => d.Num > d.Himall_CouponRecord.Count());//查询库存剩余
            coupon = coupon.GetPage(out total, d => d.OrderByDescending(o => o.EndTime), page, pageSize);
            ObsoletePageModel<CouponInfo> pageModel = new ObsoletePageModel<CouponInfo>() { Models = coupon, Total = total };
            return pageModel;
        }


        /// <summary>
        /// 获取指定优惠券会员领取情况统计
        /// </summary>
        /// <param name="couponIds">优惠券ID数组</param>
        /// <returns></returns>
        public List<CouponRecordInfo> GetCouponRecordTotal(long[] couponIds)
        {
            var model = Context.CouponRecordInfo.Where(d => couponIds.Contains(d.CouponId)).ToList();
            return model;
        }
        /// <summary>
        /// 获取能够使用的优惠券数量[排除过期、已领完]
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public int GetUserCouponCount(long shopId)
        {
            DynamicParameters parms = new DynamicParameters();
            parms.Add("@ShopId", shopId);
            parms.Add("@PlatForm", Core.PlatformType.Wap);
            StringBuilder countsql = new StringBuilder(2048);
            countsql.Append(" select count(1) from ( ");
            countsql.Append(" select A.Id,A.ShopId,A.CouponName,A.Num, ");
            countsql.Append(" (select count(1) from Himall_CouponRecord where CouponID=A.Id)AS hasNum from Himall_Coupon A ");
            countsql.Append(" left join Himall_CouponSetting S on A.Id=S.CouponID  ");
            countsql.Append(" where A.ShopId=@ShopId and A.EndTime>CURDATE() and S.PlatForm=@PlatForm and (S.Display=1 OR S.Display is NULL) ");
            countsql.Append(" order by A.Id desc )T where T.hasNum<T.Num ");/*此SQL还需优化*/
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                return Core.Helper.TypeHelper.ObjectToInt(conn.ExecuteScalar(string.Concat(countsql), parms));
            }
        }
    }
}
