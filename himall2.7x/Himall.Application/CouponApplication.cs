using System;
using System.Collections.Generic;
using System.Linq;
using Himall.IServices;
using Himall.Model;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.DTO;
using Himall.IServices.QueryModel;
using Himall.CommonModel;

namespace Himall.Application
{
    /// <summary>
    /// 优惠券业务实现
    /// </summary>
    public class CouponApplication
    {

        private static ICouponService _iCouponService = ObjectContainer.Current.Resolve<ICouponService>();

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
                CouponSendByRegisterApplication.AddCouponSendByRegister(model);
            }
            else
            {
                CouponSendByRegisterApplication.UpdateCouponSendByRegister(model);
            }
        }

        /// <summary>
        /// 获取优惠券设置
        /// </summary>
        /// <returns></returns>
        public static Himall.DTO.CouponSendByRegisterModel GetCouponSendByRegister()
        {
            var vModel = new Himall.DTO.CouponSendByRegisterModel();
            var model = CouponSendByRegisterApplication.GetCouponSendByRegister();
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
            if (model != null && model.Status.Equals(Himall.CommonModel.CouponSendByRegisterStatus.Open) &&  model.total > 0)//如果活动开启，且优惠券有剩余
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
                var pageMode = MemberApplication.GetMembers(new MemberQuery
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
                        pageMode = MemberApplication.GetMembers(new MemberQuery
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
                    List<SendmessagerecordCouponSNInfo> lsendSN = new List<SendmessagerecordCouponSNInfo>();
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
                                    var couponRecord =_iCouponService.AddCouponRecord(info);
                                    lsendSN.Add(new SendmessagerecordCouponSNInfo() { CouponSN = couponRecord.CounponSN });
                                }
                            }

                            if (suTotal)
                            {
                                MessageCouponInfo info = new MessageCouponInfo();
                                info.Money = price;
                                info.SiteName = siteName;
                                info.UserName = mUserMember[i].UserName;
                                MessageApplication.SendMessageOnCouponSuccess(mUserMember[i].Id, info);
                            }
                        }

                        Log.Debug("sendCoupon:" + alTotal);
                        //查看成功发送会员数
                        if (alTotal)
                        {
                            //记录发送历史
                            var sendRecord = new Himall.Model.SendMessageRecordInfo
                            {
                                ContentType = WXMsgType.wxcard,
                                MessageType = MsgType.Coupon,
                                SendContent = "",
                                SendState = 1,
                                SendTime = DateTime.Now,
                                ToUserLabel = labelinfos == null ? "" : labelinfos,
                                Himall_SendmessagerecordCoupon = lsendInfo
                            };
                            var record = WXMsgTemplateApplication.AddSendRecordItem(sendRecord);
                            foreach (var item in lsendSN)
                            {
                                item.MessageId = record.Id;
                            }
                            _iCouponService.AddSendmessagerecordCouponSN(lsendSN);
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



        /// <summary>
        /// 发送优惠券，根据会员ID
        /// </summary>
        /// <param name="userIds">发送对象</param>
        /// <param name="couponIds">优惠券名称</param>
        public static void SendCouponByUserIds(IEnumerable<long> userIds, IEnumerable<long> couponIds)
        {
            var model = _iCouponService.GetCouponInfo(couponIds.ToArray());
            var siteName = SiteSettingApplication.GetSiteSettings().SiteName;
            var mCouponRecord = _iCouponService.GetCouponRecordTotal(couponIds.ToArray());
            var mUserMember = MemberApplication.GetMembers(userIds);
            decimal price = 0;
            string result = "";
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
                        MessageApplication.SendMessageOnCouponSuccess(mUserMember[i].Id, info);
                    }
                }

                Log.Debug("sendCoupon:" + alTotal);
                //查看成功发送会员数
                if (alTotal)
                {
                    //记录发送历史
                    var sendRecord = new Himall.Model.SendMessageRecordInfo
                    {
                        ContentType = WXMsgType.wxcard,
                        MessageType = MsgType.Coupon,
                        SendContent = "",
                        SendState = 1,
                        SendTime = DateTime.Now,
                        ToUserLabel = "",
                        Himall_SendmessagerecordCoupon = lsendInfo
                    };
                    WXMsgTemplateApplication.AddSendRecord(sendRecord);
                }
                else
                {
                    result = "无符合领取此优惠券的会员";
                }
            }
            else
                result = "该标签下无任何会员";
            if (!string.IsNullOrWhiteSpace(result))
            {
                throw new HimallException(result);
            }
        }



        /// <summary>
        /// 发送优惠券，根据搜索条件
        /// </summary>
        /// <param name="query"></param>
        /// <param name="couponIds"></param>
        public static void SendCoupon(MemberPowerQuery query, IEnumerable<long> couponIds, string labelinfos = "")
        {
            var siteName = SiteSettingApplication.GetSiteSettings().SiteName;
            decimal price = 0;
            string result = "";
            //会员领取优惠券记录ID
            //   List<long> memberCouponIds = new List<long>();
            // dictResult = new Dictionary<string, int>();  

            query.PageSize = 500;
            query.PageNo = 1;

            var pageMode = MemberApplication.GetPurchasingPowerMember(query);
            if (pageMode.Total > 0)
            {
                var mUserMember = new List<MemberPurchasingPower>();
                while (pageMode.Models.Count() > 0)//循环批量获取用户信息
                {
                    //   string[] dests = pageMode.Models.Select(e => e.).ToArray();
                    foreach (var item in pageMode.Models)
                    {
                        mUserMember.Add(item);
                    }
                    query.PageNo += 1;
                    pageMode = MemberApplication.GetPurchasingPowerMember(query);
                }


                var model = _iCouponService.GetCouponInfo(couponIds.ToArray());//获取所选优惠券集合

                //查询优惠券领取状况
                var mCouponRecord = _iCouponService.GetCouponRecordTotal(couponIds.ToArray());

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
                            MessageApplication.SendMessageOnCouponSuccess(mUserMember[i].Id, info);
                        }
                    }

                    Log.Debug("sendCoupon:" + alTotal);
                    //查看成功发送会员数
                    if (alTotal)
                    {
                        //记录发送历史
                        var sendRecord = new Himall.Model.SendMessageRecordInfo
                        {
                            ContentType = WXMsgType.wxcard,
                            MessageType = MsgType.Coupon,
                            SendContent = "",
                            SendState = 1,
                            SendTime = DateTime.Now,
                            ToUserLabel = labelinfos == null ? "" : labelinfos,
                            Himall_SendmessagerecordCoupon = lsendInfo
                        };
                        WXMsgTemplateApplication.AddSendRecord(sendRecord);
                    }
                    else
                    {
                        result = "此标签下无符合领取此优惠券的会员";
                    }
                }
            }
            else
                result = "该标签下无任何会员";


            if (!string.IsNullOrWhiteSpace(result))
            {
                throw new HimallException(result);
            }
        }



        public static ObsoletePageModel<CouponModel> GetCouponByName(string text, DateTime endDate, int ReceiveType, int page, int pageSize)
        {
            var couponList = _iCouponService.GetCouponByName(text, endDate, ReceiveType, page, pageSize);
            var pageModel = new ObsoletePageModel<CouponModel>();

            var lmCoupon = new List<Himall.DTO.CouponModel>();
            foreach (var item in couponList.Models.ToList())
            {
                if (item.IsSyncWeiXin == 0 || (item.IsSyncWeiXin == 1 && item.WXAuditStatus == (int)WXCardLogInfo.AuditStatusEnum.Audited))
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
        /// <summary>
        /// 商家添加一个优惠券
        /// </summary>
        /// <param name="info"></param>
        public static void AddCoupon(CouponInfo info)
        {
            _iCouponService.AddCoupon(info);
        }

        //使优惠券失效
        public static void CancelCoupon(long couponId, long shopId)
        {
            _iCouponService.CancelCoupon(couponId, shopId);
        }
        /// <summary>
        /// 商家修改一个优惠券
        /// </summary>
        /// <param name="info"></param>
        public static void EditCoupon(CouponInfo info)
        {
            _iCouponService.EditCoupon(info);
        }

        /// <summary>
        /// 领取一个优惠券
        /// </summary>
        /// <param name="info"></param>
        public static void AddCouponRecord(CouponRecordInfo info)
        {
            _iCouponService.AddCouponRecord(info);
        }

        //使用优惠券
        public static void UseCoupon(long userId, IEnumerable<long> Ids, IEnumerable<OrderInfo> orders)
        {
            _iCouponService.UseCoupon(userId, Ids, orders);
        }

        /// <summary>
        /// 获取店铺订购的优惠券信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static ActiveMarketServiceInfo GetCouponService(long shopId)
        {
            return _iCouponService.GetCouponService(shopId);
        }

        /// <summary>
        /// 获取商家添加的优惠券列表
        /// </summary>
        /// <returns></returns>
        public static ObsoletePageModel<CouponInfo> GetCouponList(CouponQuery query)
        {
            return _iCouponService.GetCouponList(query);
        }

        /// <summary>
        /// 获取商家添加的优惠券列表(全部)
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public static IQueryable<CouponInfo> GetCouponList(long shopid)
        {
            return _iCouponService.GetCouponList(shopid);
        }
        /// <summary>
        /// 获取领取的优惠券列表
        /// </summary>
        /// <returns></returns>
        public static ObsoletePageModel<CouponRecordInfo> GetCouponRecordList(CouponRecordQuery query)
        {
            return _iCouponService.GetCouponRecordList(query);
        }
        /// <summary>
        /// 获取已邻取优惠券信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static CouponRecordInfo GetCouponRecordById(long id)
        {
            return _iCouponService.GetCouponRecordById(id);
        }

        /// <summary>
        /// 获取优惠券信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="couponId"></param>
        /// <returns></returns>
        public static CouponInfo GetCouponInfo(long shopId, long couponId)
        {
            return _iCouponService.GetCouponInfo(shopId, couponId);
        }
        /// <summary>
        /// 获取优惠券信息（couponid）
        /// </summary>
        /// <param name="couponId"></param>
        /// <returns></returns>
        public static CouponInfo GetCouponInfo(long couponId)
        {
            return _iCouponService.GetCouponInfo(couponId);
        }

        /// <summary>
        /// 批量获取优惠券信息（couponIds）
        /// </summary>
        /// <param name="couponIds">优惠券数组</param>
        /// <returns></returns>
        public static List<CouponInfo> GetCouponInfo(long[] couponIds)
        {
            return _iCouponService.GetCouponInfo(couponIds);
        }
        /// <summary>
        /// 获取已使用的的某一个优惠券的详细
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static CouponRecordInfo GetCouponRecordInfo(long userId, long orderId)
        {
            return _iCouponService.GetCouponRecordInfo(userId, orderId);
        }

        /// <summary>
        /// 获取可用优惠券
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static IEnumerable<CouponInfo> GetTopCoupon(long shopId, int top = 5, PlatformType type = Core.PlatformType.PC)
        {
            return _iCouponService.GetTopCoupon(shopId, top, type);
        }

        /// <summary>
        /// 获取用户领取的某个优惠券的数量
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public static int GetUserReceiveCoupon(long couponId, long userId)
        {
            return _iCouponService.GetUserReceiveCoupon(couponId, userId);
        }

        /// <summary>
        /// 获取一个用户在某个店铺的可用优惠券
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="userId"></param>
        /// <param name="totalPrice">总金额</param>
        /// <returns></returns>
        public static List<CouponRecordInfo> GetUserCoupon(long shopId, long userId, decimal totalPrice)
        {
            return _iCouponService.GetUserCoupon(shopId, userId, totalPrice);
        }

        ///获取用户将要使用的优惠券列表
        ///
        public static IEnumerable<CouponRecordInfo> GetOrderCoupons(long userId, IEnumerable<long> Ids)
        {
            return _iCouponService.GetOrderCoupons(userId, Ids);
        }
        /// <summary>
        /// 取用户领取的所有优惠卷信息
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static IQueryable<UserCouponInfo> GetUserCouponList(long userid)
        {
            return _iCouponService.GetUserCouponList(userid);
        }

        public static List<UserCouponInfo> GetAllUserCoupon(long userid)
        {
            return _iCouponService.GetAllUserCoupon(userid);
        }
        /// <summary>
        /// 是否可以添加积分兑换红包
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public static bool CanAddIntegralCoupon(long shopid, long id = 0)
        {
            return _iCouponService.CanAddIntegralCoupon(shopid, id);

        }
        /// <summary>
        /// 取积分优惠券列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static ObsoletePageModel<CouponInfo> GetIntegralCoupons(int page, int pageSize)
        {
            return _iCouponService.GetIntegralCoupons(page, pageSize);
        }
        /// <summary>
        /// 同步微信卡券审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cardid"></param>
        /// <param name="auditstatus">审核状态</param>
        public static void SyncWeixinCardAudit(long id, string cardid, WXCardLogInfo.AuditStatusEnum auditstatus)
        {
            _iCouponService.SyncWeixinCardAudit(id, cardid, auditstatus);
        }
        /// <summary>
        /// 处理错误的卡券同步信息
        /// </summary>
        public static void ClearErrorWeiXinCardSync()
        {

            _iCouponService.ClearErrorWeiXinCardSync();
        }

        /// <summary>
        /// 获取指定优惠券会员领取情况统计
        /// </summary>
        /// <param name="couponIds">优惠券ID数组</param>
        /// <returns></returns>
        public static List<CouponRecordInfo> GetCouponRecordTotal(long[] couponIds)
        {
            return _iCouponService.GetCouponRecordTotal(couponIds);
        }
    }
}