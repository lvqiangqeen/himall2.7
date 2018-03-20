using System;
using System.Collections.Generic;
using System.Linq;
using Himall.IServices;
using Himall.Model;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.DTO;

namespace Himall.Application
{
    /// <summary>
    /// 优惠券业务实现
    /// </summary>
    public class CouponApplication
    {

        private static ICouponSendByRegisterService _ICouponSendByRegisterService = ObjectContainer.Current.Resolve<ICouponSendByRegisterService>();
        private static ICouponService _iCouponService = ObjectContainer.Current.Resolve<ICouponService>();
        private static IMemberService _iMemberService = ObjectContainer.Current.Resolve<IMemberService>();
        private static IWXMsgTemplateService _iWXMsgTemplateService = ObjectContainer.Current.Resolve<IWXMsgTemplateService>();
        private static IMessageService _iMessageService = ObjectContainer.Current.Resolve<IMessageService>();
        /// <summary>
        /// 优惠券设置
        /// </summary>
        /// <param name="mCouponRegister"></param>
        public static void SetCouponSendByRegister(Himall.DTO.CouponSendByRegisterModel mCouponSendByRegisterModel)
        {
            List<Himall.Model.CouponSendByRegisterDetailedInfo> lmCouponSendByRegisterDetailed = new List<Model.CouponSendByRegisterDetailedInfo>();
            foreach (var item in mCouponSendByRegisterModel.CouponIds)
            {
                lmCouponSendByRegisterDetailed.Add(new Himall.Model.CouponSendByRegisterDetailedInfo() { CouponId = item.Id });
            }
            var model = new Himall.Model.CouponSendByRegisterInfo
            {
                Himall_CouponSendByRegisterDetailed = lmCouponSendByRegisterDetailed,
                Link = mCouponSendByRegisterModel.Link,
                Status = mCouponSendByRegisterModel.Status,
                Id = mCouponSendByRegisterModel.Id
            };
            if (model.Id <= 0)
            {
                _ICouponSendByRegisterService.AddCouponSendByRegister(model);
            }
            else
            {
                _ICouponSendByRegisterService.UpdateCouponSendByRegister(model);
            }
        }

        /// <summary>
        /// 获取优惠券设置
        /// </summary>
        /// <returns></returns>
        public static Himall.DTO.CouponSendByRegisterModel GetCouponSendByRegister()
        {
            var vModel = new Himall.DTO.CouponSendByRegisterModel();
            var model = _ICouponSendByRegisterService.GetCouponSendByRegister();
            if (model != null)
            {
                vModel.Id = model.Id;
                vModel.Link = model.Link;
                vModel.Status = model.Status;

                int total = 0;
                decimal price = 0;
                var lmCoupon = new List<Himall.DTO.CouponModel>();
                foreach (var item in model.Himall_CouponSendByRegisterDetailed)
                {
                    int inventory = item.Himall_Coupon.Num - item.Himall_Coupon.Himall_CouponRecord.Count();//优惠券剩余量
                    if (inventory > 0 && item.Himall_Coupon.EndTime > DateTime.Now)
                    {
                        total += inventory;
                        price += item.Himall_Coupon.Price;
                        lmCoupon.Add(new Himall.DTO.CouponModel
                        {
                            Id = item.CouponId,
                            CouponName = item.Himall_Coupon.CouponName,
                            inventory = inventory,
                            Num = item.Himall_Coupon.Num,
                            useNum = item.Himall_Coupon.Himall_CouponRecord.Count(),
                            Price = item.Himall_Coupon.Price,
                            ShopId = item.Himall_Coupon.ShopId,
                            ShopName = item.Himall_Coupon.ShopName,
                            EndTime = item.Himall_Coupon.EndTime,
                            StartTime = item.Himall_Coupon.StartTime,
                            OrderAmount = item.Himall_Coupon.OrderAmount == 0 ? "不限制" : "满" + item.Himall_Coupon.OrderAmount
                        });
                    }
                }
                vModel.CouponIds = lmCoupon;
                vModel.total = total;
                vModel.price = price;
                if (vModel.CouponIds.Count.Equals(0))
                {
                    vModel.Status = Himall.CommonModel.CouponSendByRegisterStatus.Shut;
                }
            }
            return vModel;
        }

        /// <summary>
        /// 注册成功赠送优惠券
        /// </summary>
        /// <param name="id">会员ID</param>
        /// <param name="userName">会员登录名</param>
        /// <returns>返回赠送张数</returns>
        public static int RegisterSendCoupon(long id, string userName)
        {
            int result = 0;
            var model = GetCouponSendByRegister();
            if (model.Status.Equals(Himall.CommonModel.CouponSendByRegisterStatus.Open) && model.total > 0)//如果活动开启，且优惠券有剩余
            {
                foreach (var item in model.CouponIds)
                {
                    if (item.inventory > 0)
                    {
                        CouponRecordInfo info = new CouponRecordInfo();
                        info.UserId = id;
                        info.UserName = userName;
                        info.ShopId = item.ShopId;
                        info.CouponId = item.Id;
                        _iCouponService.AddCouponRecord(info);
                        result++;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 发送优惠券
        /// </summary>
        /// <param name="labelids">发送标签</param>
        /// <param name="labelinfos">标签名称</param>
        /// <param name="couponIds">优惠券名称</param>
        /// <returns>-1:优惠券不足;-2:请选择发送的优惠券;-3:标签中用户数为0</returns>
        public static string SendCouponMsg(string labelids, string labelinfos, string couponIds, string url)
        {

            var messageEmali = PluginsManagement.GetPlugin<IMessagePlugin>("Himall.Plugin.Message.Email");
            var messageSMS = PluginsManagement.GetPlugin<IMessagePlugin>("Himall.Plugin.Message.SMS");
            string result = "";
            if (!couponIds.TrimEnd(',').Equals(""))
            {
                //取出标签对应的会员信息
                long[] lids = string.IsNullOrWhiteSpace(labelids) ? null : labelids.Split(',').Select(s => long.Parse(s)).ToArray();
                int pageNo = 1, pageSize = 100;
                var pageMode = _iMemberService.GetMembers(new Himall.IServices.QueryModel.MemberQuery
                {
                    LabelId = lids,
                    PageNo = pageNo,
                    PageSize = pageSize
                });
                if (pageMode.Total > 0)
                {
                    List<UserMemberInfo> mUserMember = new List<UserMemberInfo>();
                    while (pageMode.Models.Count() > 0)//循环批量获取用户信息
                    {
                        string[] dests = pageMode.Models.Select(e => e.Email).ToArray();
                        foreach (var item in pageMode.Models)
                        {
                            mUserMember.Add(item);
                        }
                        pageNo += 1;
                        pageMode = _iMemberService.GetMembers(new Himall.IServices.QueryModel.MemberQuery
                        {
                            LabelId = lids,
                            PageNo = pageNo,
                            PageSize = pageSize
                        });
                    }

                    string[] arrStr = couponIds.TrimEnd(',').Split(',');
                    long[] arrcouponIds = arrStr.Select(a => long.Parse(a)).ToArray();

                    var model = _iCouponService.GetCouponInfo(arrcouponIds);//获取所选优惠券集合

                    //查询优惠券领取状况
                    var mCouponRecord = _iCouponService.GetCouponRecordTotal(arrcouponIds);

                    decimal price = 0;
                    List<SendmessagerecordCouponInfo> lsendInfo = new List<SendmessagerecordCouponInfo>();
                    //验证优惠券是否充足
                    foreach (var item in model)
                    {
                        price += item.Price;
                        lsendInfo.Add(new SendmessagerecordCouponInfo() { CouponId = item.Id });
                        if ((item.Num - item.Himall_CouponRecord.Count()) < mUserMember.Count)
                        {
                            result = item.CouponName + "优惠券的数量不足，无法赠送";
                            break;
                        }
                    }
                    var siteName = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().SiteName;
                    if (result == "")
                    {
                        //发送优惠券
                        bool alTotal = false;
                        for (int i = 0; i < mUserMember.Count; i++)
                        {
                            bool suTotal = false;//会员发送优惠券成功数
                            foreach (var item in model)
                            {
                                //判断会员领取限制，是否可领取此优惠券
                                bool isf = true;
                                if (item.PerMax > 0)
                                {
                                    int total = mCouponRecord.Where(p => p.UserId == mUserMember[i].Id && p.CouponId == item.Id).ToList().Count;
                                    if (item.PerMax <= total)
                                    {
                                        isf = false;
                                    }
                                }

                                if (isf)
                                {
                                    suTotal = true;
                                    alTotal = true;

                                    CouponRecordInfo info = new CouponRecordInfo();
                                    info.UserId = mUserMember[i].Id;
                                    info.UserName = mUserMember[i].UserName;
                                    info.ShopId = item.ShopId;
                                    info.CouponId = item.Id;
                                    _iCouponService.AddCouponRecord(info);
                                }
                            }

                            if (suTotal)
                            {
                                MessageCouponInfo info = new MessageCouponInfo();
                                info.Money = price;
                                info.SiteName = siteName;
                                info.UserName = mUserMember[i].UserName;
                                _iMessageService.SendMessageOnCouponSuccess(mUserMember[i].Id, info);
                            }
                        }

                        Log.Debug("sendCoupon:" + alTotal);
                        //查看成功发送会员数
                        if (alTotal)
                        {
                            //记录发送历史
                            var sendRecord = new Himall.Model.SendMessageRecordInfo
                            {
                                ContentType = Model.WXMsgType.wxcard,
                                MessageType = Model.MsgType.Coupon,
                                SendContent = "",
                                SendState = 1,
                                SendTime = DateTime.Now,
                                ToUserLabel = labelinfos == null ? "" : labelinfos,
                                Himall_SendmessagerecordCoupon = lsendInfo
                            };
                            _iWXMsgTemplateService.AddSendRecord(sendRecord);
                        }
                        else
                        {
                            result = "此标签下无符合领取此优惠券的会员";
                        }
                    }
                }
                else
                    result = "该标签下无任何会员";
            }
            else
                result = "请选择发送的优惠券";
            return result;
        }

        public static PageModel<CouponModel> GetCouponByName(string text, DateTime endDate, int ReceiveType, int page, int pageSize)
        {
            var couponList = _iCouponService.GetCouponByName(text, endDate, ReceiveType, page, pageSize);
            var pageModel = new PageModel<CouponModel>();

            var lmCoupon = new List<Himall.DTO.CouponModel>();
            foreach (var item in couponList.Models.ToList())
            {
                CouponModel couponModel = new CouponModel();
                couponModel.CouponName = item.CouponName;
                couponModel.Id = item.Id;
                couponModel.Num = item.Num;
                couponModel.useNum = item.Himall_CouponRecord.Count();
                couponModel.inventory = item.Num - item.Himall_CouponRecord.Count();
                couponModel.OrderAmount = item.OrderAmount == 0 ? "不限制" : "满" + item.OrderAmount;
                couponModel.Price = item.Price;
                couponModel.ShopName = item.ShopName;
                couponModel.EndTime = item.EndTime;
                couponModel.StartTime = item.StartTime;
                couponModel.perMax = item.PerMax;
                lmCoupon.Add(couponModel);
            }
            pageModel.Models = lmCoupon.AsQueryable();
            pageModel.Total = couponList.Total;
            return pageModel;
        }

        public static CouponModel Get(long id)
        {
            var couponList = _iCouponService.GetCouponInfo(id);
            if (couponList == null)
                return null;
            var lmCoupon = new Himall.DTO.CouponModel();
            lmCoupon.CouponName = couponList.CouponName;
            lmCoupon.Id = couponList.Id;
            lmCoupon.Num = couponList.Num;
            lmCoupon.useNum = couponList.Himall_CouponRecord.Count();
            lmCoupon.inventory = couponList.Num - couponList.Himall_CouponRecord.Count();
            lmCoupon.OrderAmount = couponList.OrderAmount == 0 ? "不限制" : "满" + couponList.OrderAmount;
            lmCoupon.Price = couponList.Id;
            lmCoupon.ShopName = couponList.ShopName;
            lmCoupon.EndTime = couponList.EndTime;
            lmCoupon.StartTime = couponList.StartTime;
            lmCoupon.perMax = couponList.PerMax;
            return lmCoupon;
        }
    }
}
