using Himall.Core;
using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;
using Senparc.Weixin.Helpers;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.Card;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Helpers;

namespace Himall.Service
{
    public class WXCardService : ServiceBase, IWXCardService
    {
        #region 私有
        /// <summary>
        /// 库存最大数 1000000
        /// </summary>
        private int MaxStock = 1000000;
        private Weixin.WXHelper wxhelper;
        /// <summary>
        /// 获取访问令牌
        /// </summary>
        /// <param name="AppId"></param>
        /// <param name="AppSecret"></param>
        /// <returns></returns>
        private string GetAccessToken(string appid, string secret)
        {
            string result = "";
            wxhelper = new Weixin.WXHelper();
            result = wxhelper.GetAccessToken(appid, secret);
            return result;
        }
        /// <summary>
        /// 获取JSApi令牌(卡券)
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private string GetCardJSApiTicket(string accessToken)
        {
            string result = "";
            wxhelper = new Weixin.WXHelper();
            result = wxhelper.GetTicketByToken(accessToken, "wx_card");
            return result;
        }
        /// <summary>
        /// 获取JSApi令牌
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private string GetJSApiTicket(string appid, string secret)
        {
            string result = "";
            wxhelper = new Weixin.WXHelper();
            result = wxhelper.GetTicket(appid, secret, "jsapi");
            return result;
        }
        /// <summary>
        /// 获取JSApi令牌
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private string GetJSApiTicket(string accessToken)
        {
            string result = "";
            wxhelper = new Weixin.WXHelper();
            result = wxhelper.GetTicketByToken(accessToken, "jsapi");
            return result;
        }
        #endregion

        /// <summary>
        /// 添加卡券
        /// </summary>
        /// <param name="info"></param>
        public bool Add(WXCardLogInfo info)
        {
            bool issuccess = false;
            string acctoken = "";
            string curHost = "http://" + Core.Helper.WebHelper.GetHost();
            string imagefile="/images/defaultwxlogo.png";
            string wxlogo = Core.HimallIO.GetImagePath(imagefile);
            if (wxlogo.IndexOf("http") < 0)
            {
                wxlogo = curHost + imagefile;
            }
            var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
            var shopser = ServiceProvider.Instance<IShopService>.Create;
            var vshopser = ServiceProvider.Instance<IVShopService>.Create;
            CardCreateResultJson wxResultJson = new CardCreateResultJson();

            #region 组织数据
            //基础数据
            Card_BaseInfoBase _BaseInfo = new Card_BaseInfoBase()
            {
                logo_url = wxlogo,
                brand_name = siteSetting.SiteName,
                code_type = Card_CodeType.CODE_TYPE_BARCODE,
                title = info.CardTitle,
                sub_title = info.CardSubTitle,
                color = info.CardColor,
                notice = "专供" + siteSetting.SiteName + "使用",
                description = @"" + (info.LeastCost > 0 ? "满￥" + (info.LeastCost / 100).ToString("F2") + "使用" : "无门槛使用") + "，有效期至" + info.BeginTime.ToString("yyyy年MM月dd日") + "-" + info.EndTime.ToString("yyyy年MM月dd日"),
                date_info = new Card_BaseInfo_DateInfo()
                {
                    type = Card_DateInfo_Type.DATE_TYPE_FIX_TIME_RANGE.ToString(),
                    begin_timestamp = DateTimeHelper.GetWeixinDateTime(info.BeginTime),
                    end_timestamp = DateTimeHelper.GetWeixinDateTime(info.EndTime),
                },
                sku = new Card_BaseInfo_Sku()
                {
                    quantity = info.Quantity == 0 ? MaxStock : info.Quantity
                },
                get_limit = info.GetLimit == 0 ? MaxStock : info.GetLimit,
                use_custom_code = false,
                bind_openid = false,
                can_share = false,
                can_give_friend = false,
                custom_url_name = "立即使用",
            };
            //代金券数据
            var cardData = new Card_GeneralCouponData()
            {
                base_info = _BaseInfo,
                default_detail = info.DefaultDetail
            };
            #endregion

            #region 商家发布
            if (info.ShopId > 0)
            {
                _BaseInfo.custom_url = curHost + "/Shop/Home/" + info.ShopId.ToString();
            }
            var vshopSetting = vshopser.GetVShopSetting(info.ShopId);
            var vshopinfo = vshopser.GetVShopByShopId(info.ShopId);
            var shopinfo = shopser.GetShop(info.ShopId);
            if (vshopSetting != null && shopinfo != null && vshopinfo != null)
            {
                if (!string.IsNullOrWhiteSpace(vshopSetting.AppId) && !string.IsNullOrWhiteSpace(vshopSetting.AppSecret))
                {
                    acctoken = GetAccessToken(vshopSetting.AppId, vshopSetting.AppSecret);
                    if (!string.IsNullOrWhiteSpace(acctoken))
                    {
                        _BaseInfo.brand_name = shopinfo.ShopName;
                        if (!string.IsNullOrWhiteSpace(vshopinfo.WXLogo))
                        {
                            wxlogo = curHost + vshopinfo.WXLogo;
                            _BaseInfo.logo_url = wxlogo;
                        }
                        wxResultJson = CardApi.CreateCard(acctoken, cardData);
                        if (wxResultJson.errcode == Senparc.Weixin.ReturnCode.请求成功)
                        {
                            info.AppId = vshopSetting.AppId;
                            info.AppSecret = vshopSetting.AppSecret;
                            info.CardId = wxResultJson.card_id;
                            issuccess = true;
                        }
                    }
                }
            }
            #endregion

            if (!issuccess)
            {
                #region 平台发布
                if (!string.IsNullOrWhiteSpace(siteSetting.WeixinAppId) && !string.IsNullOrWhiteSpace(siteSetting.WeixinAppSecret))
                {
                    acctoken = GetAccessToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
                    if (!string.IsNullOrWhiteSpace(acctoken))
                    {
                        _BaseInfo.brand_name = shopinfo.ShopName;
                        if (!string.IsNullOrWhiteSpace(siteSetting.WXLogo))
                        {
                            wxlogo = curHost + siteSetting.WXLogo;
                            _BaseInfo.logo_url = wxlogo;
                        }
                        wxResultJson = CardApi.CreateCard(acctoken, cardData);
                        if (wxResultJson.errcode == Senparc.Weixin.ReturnCode.请求成功)
                        {
                            info.AppId = siteSetting.WeixinAppId;
                            info.AppSecret = siteSetting.WeixinAppSecret;
                            info.CardId = wxResultJson.card_id;
                            issuccess = true;
                        }
                    }
                }

                if (info.ShopId < 1)
                {
                    _BaseInfo.custom_url = curHost + "/";
                }
                #endregion
            }

            if (issuccess)
            {
                info.AuditStatus = (int)WXCardLogInfo.AuditStatusEnum.Auditin;    //初始审核状态
                //数据入库
                Context.WXCardLogInfo.Add(info);
                Context.SaveChanges();
            }
            return issuccess;
        }

        #region 卡券修改

        /// <summary>
        /// 修改卡券限领
        /// </summary>
        /// <param name="num">null表示不限领取数量</param>
        /// <param name="cardid"></param>
        /// <param name="stock">库存数</param>
        public void EditGetLimit(int? num, string cardid)
        {
            //实际处理中，不限领改为0
            if (num == null) num = 0;
            var carddata = Context.WXCardLogInfo.FirstOrDefault(d => d.CardId == cardid);
            if (carddata != null)
            {
                var acctoken = GetAccessToken(carddata.AppId, carddata.AppSecret);
                if (!string.IsNullOrWhiteSpace(acctoken))
                {
                    var wxResultJson = ApiHandlerWapper.TryCommonApi(accessToken =>
                    {
                        var urlFormat = string.Format("https://api.weixin.qq.com/card/update?access_token={0}", accessToken);
                        var data = new
                        {
                            card_id = carddata.CardId,
                            general_coupon = new
                            {
                                base_info = new
                                {
                                    get_limit = num
                                }
                            }
                        };
                        return CommonJsonSend.Send<Senparc.Weixin.Entities.WxJsonResult>(null, urlFormat, data);
                    }, acctoken);

                    if (wxResultJson.errcode != Senparc.Weixin.ReturnCode.请求成功)
                    {
                        Core.Log.Error("微信同步修改卡券个人限领失败", new Exception(((int)wxResultJson.errcode).ToString() + ":" + wxResultJson.errmsg));
                    }
                }
            }
        }
        /// <summary>
        /// 修改卡券限领
        /// </summary>
        /// <param name="num">null表示不限领取数量</param>
        /// <param name="id"></param>
        /// <param name="stock">库存数</param>
        public void EditGetLimit(int? num, long id)
        {
            var carddata = Context.WXCardLogInfo.FirstOrDefault(d => d.Id == id);
            if (carddata != null)
            {
                EditGetLimit(num, carddata.CardId);
            }
            else
            {
                throw new HimallException("错误的数据编号");
            }

        }
        /// <summary>
        /// 修改卡券库存
        /// </summary>
        /// <param name="num"></param>
        /// <param name="cardid"></param>
        public void EditStock(int num, string cardid)
        {
            var carddata = Context.WXCardLogInfo.FirstOrDefault(d => d.CardId == cardid);
            if (carddata != null)
            {
                var acctoken = GetAccessToken(carddata.AppId, carddata.AppSecret);
                if (!string.IsNullOrWhiteSpace(acctoken))
                {
                    //获取现在库存
                    var wxcarddata = CardApi.CardDetailGet(acctoken, carddata.CardId);
                    if (wxcarddata != null)
                    {
                        //计算差值
                        var diffnum = num - wxcarddata.card.general_coupon.base_info.sku.quantity;
                        if (diffnum != 0)
                        {
                            //提交修改
                            var wxResultJson = CardApi.ModifyStock(acctoken, carddata.CardId, (diffnum > 0 ? diffnum : 0), (diffnum < 0 ? Math.Abs(diffnum) : 0));
                            if (wxResultJson.errcode != Senparc.Weixin.ReturnCode.请求成功)
                            {
                                Core.Log.Error("微信同步修改卡券库存失败", new Exception(((int)wxResultJson.errcode).ToString()));
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 修改卡券库存
        /// </summary>
        /// <param name="num"></param>
        /// <param name="id"></param>
        public void EditStock(int num, long id)
        {
            var carddata = Context.WXCardLogInfo.FirstOrDefault(d => d.Id == id);
            if (carddata != null)
            {
                EditStock(num, carddata.CardId);
            }
            else
            {
                throw new HimallException("错误的数据编号");
            }

        }
        #endregion

        #region 卡券删除
        /// <summary>
        /// 删除卡券
        /// </summary>
        /// <param name="cardid"></param>
        public void Delete(string cardid)
        {
            var card = Context.WXCardLogInfo.FirstOrDefault(d => d.CardId == cardid);
            if (card != null)
            {
                Delete(card.Id);
            }
        }
        /// <summary>
        /// 删除卡券
        /// </summary>
        /// <param name="id"></param>
        public void Delete(long id)
        {
            var card = Context.WXCardLogInfo.FirstOrDefault(d => d.Id == id);
            if (card != null)
            {
                string acctoken = GetAccessToken(card.AppId, card.AppSecret);

                #region 先核销所有已领卡券
                //同步核销
                List<WXCardCodeLogInfo> wxclist = Context.WXCardCodeLogInfo.Where(d => d.CardId == card.CardId).ToList();
                foreach (var item in wxclist)
                {
                    Consume(item.Id);
                }
                #endregion

                Context.WXCardLogInfo.Remove(card);
                var wxResultJson = CardApi.CardDelete(acctoken, card.CardId);
                if (wxResultJson.errcode != Senparc.Weixin.ReturnCode.请求成功)
                {
                    Core.Log.Error("微信同步删除卡券失败", new Exception(((int)wxResultJson.errcode).ToString()));
                }
                
                Context.SaveChanges();
            }
        }
        #endregion

        #region 同步卡券
        /// <summary>
        /// 卡券与红包领取信息同步前持久化
        /// </summary>
        /// <param name="cardid"></param>
        /// <param name="openid"></param>
        /// <param name="couponRecordId"></param>
        /// <param name="couponType"></param>
        private long SyncCouponRecordInfo(string cardid, long couponRecordId, WXCardLogInfo.CouponTypeEnum couponType)
        {
            long result = 0;
            if (!string.IsNullOrWhiteSpace(cardid))
            {
                var card = Context.WXCardLogInfo.FirstOrDefault(d => d.CardId == cardid);
                if (card != null)
                {
                    var wxcdata = Context.WXCardCodeLogInfo.FirstOrDefault(d => d.CouponCodeId == couponRecordId && d.CouponType == couponType);
                    if (wxcdata == null)
                    {
                        //记录数据
                        wxcdata = new WXCardCodeLogInfo();
                        wxcdata.CardId = cardid;
                        wxcdata.CodeStatus = (int)WXCardCodeLogInfo.CodeStatusEnum.WaitReceive;
                        wxcdata.CouponType = couponType;
                        wxcdata.CouponCodeId = couponRecordId;
                        wxcdata.SendTime = DateTime.Now;
                        wxcdata.CardLogId = card.Id;
                        Context.WXCardCodeLogInfo.Add(wxcdata);
                        Context.SaveChanges();
                        switch (wxcdata.CouponType)
                        {
                            case WXCardLogInfo.CouponTypeEnum.Coupon:
                                var coupondata = Context.CouponRecordInfo.FirstOrDefault(d => d.Id == couponRecordId);
                                if (coupondata != null)
                                {
                                    coupondata.WXCodeId = wxcdata.Id;
                                }
                                break;
                        }
                        Context.SaveChanges();
                    }
                    result = wxcdata.Id;
                }
            }
            return result;
        }
        /// <summary>
        /// 同步微信JS信息获取
        /// <para>卡券的票据与其他票据不同，请调用此功能</para>
        /// <param name="cardid"></param>
        /// <returns></returns>
        private WXSyncJSInfoByCard GetWXSyncJSInfo(string cardid, string url)
        {
            WXSyncJSInfoByCard result = null;
            if (!string.IsNullOrWhiteSpace(cardid))
            {
                var card = Context.WXCardLogInfo.FirstOrDefault(d => d.CardId == cardid);
                if (card != null)
                {
                    //string acctoken = GetAccessToken(card.AppId, card.AppSecret);
                    string apiticket = GetJSApiTicket(card.AppId, card.AppSecret);
                    if (!string.IsNullOrWhiteSpace(apiticket))
                    {
                        result = new WXSyncJSInfoByCard();
                        JSSDKHelper jsshelper = new JSSDKHelper();
                        result.appid = card.AppId;
                        result.apiticket = apiticket;
                        result.timestamp = JSSDKHelper.GetTimestamp();
                        result.nonceStr = JSSDKHelper.GetNoncestr();
                        result.signature = jsshelper.GetSignature(result.apiticket, result.nonceStr, result.timestamp, url);
                    }
                    else
                    {
                        Log.Info("[Coupon]票据获取失败");
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 生成同步微信按扭JS信息
        /// </summary>
        /// <param name="cardid"></param>
        /// <param name="openid"></param>
        /// <param name="couponRecordId"></param>
        /// <param name="couponType"></param>
        /// <param name="url">当前页地址</param>
        /// <returns></returns>
        private WXSyncJSInfoCardInfo MakeSyncWXJSInfo(string cardid, long couponRecordId, WXCardLogInfo.CouponTypeEnum couponType)
        {
            WXSyncJSInfoCardInfo result = null;
            if (!string.IsNullOrWhiteSpace(cardid))
            {
                var card = Context.WXCardLogInfo.FirstOrDefault(d => d.CardId == cardid);
                if (card != null)
                {
                    string acctoken = GetAccessToken(card.AppId, card.AppSecret);
                    string apiticket = GetCardJSApiTicket(acctoken);
                    if (!string.IsNullOrWhiteSpace(apiticket))
                    {
                        result = new WXSyncJSInfoCardInfo();
                        //生成记录
                        long logid = SyncCouponRecordInfo(cardid, couponRecordId, couponType);
                        int outerid = (int)logid;
                        JSSDKHelper jsshelper = new JSSDKHelper();
                        result.card_id = cardid;
                        result.timestamp = JSSDKHelper.GetTimestamp();
                        result.nonce_str = "";
                        result.signature = jsshelper.GetCardSign2015(apiticket, result.nonce_str, result.timestamp, result.card_id);
                        result.outerid = outerid;
                    }
                    else
                    {
                        Log.Info("[Coupon]票据获取失败");
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 获取卡券领取地址
        /// </summary>
        /// <param name="cardid"></param>
        /// <returns></returns>
        public string GetCardReceiveUrl(string cardid, long couponRecordId, WXCardLogInfo.CouponTypeEnum couponType)
        {
            string result = "";
            if (!string.IsNullOrWhiteSpace(cardid))
            {
                var card = Context.WXCardLogInfo.FirstOrDefault(d => d.CardId == cardid);
                if (card != null)
                {
                    string acctoken = GetAccessToken(card.AppId, card.AppSecret);
                    //生成记录
                    long logid = SyncCouponRecordInfo(cardid, couponRecordId, couponType);
                    int outerid = (int)logid;
                    CreateQRResultJson wxResultJson = CardApi.CreateQR(acctoken, card.CardId, null, null, null, false, null, outerid);
                    if (wxResultJson.errcode == Senparc.Weixin.ReturnCode.请求成功)
                    {
                        result = wxResultJson.url;
                    }
                    else
                    {
                        
                        Log.Info("[Coupon]" + ((int)wxResultJson.errcode).ToString() + ":" + wxResultJson.errmsg);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 获取卡券领取地址
        /// </summary>
        /// <param name="cardid"></param>
        /// <returns></returns>
        public string GetCardReceiveUrl(long id, long couponRecordId, WXCardLogInfo.CouponTypeEnum couponType)
        {
            string cardid = "";
            var card = Context.WXCardLogInfo.FirstOrDefault(d => d.Id == id);
            if (card != null)
            {
                cardid = card.CardId;
            }
            return GetCardReceiveUrl(cardid, couponRecordId, couponType);
        }
        /// <summary>
        /// 是否可以同步微信
        /// </summary>
        /// <param name="couponid"></param>
        /// <param name="couponcodeid"></param>
        /// <param name="couponType"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public WXSyncJSInfoByCard GetSyncWeiXin(long couponid, long couponcodeid, WXCardLogInfo.CouponTypeEnum couponType, string url)
        {
            WXSyncJSInfoByCard result = null;
            bool isNeedSyncWX = false;
            WXCardLogInfo wxcardinfo = null;
            WXCardCodeLogInfo wxcodeinfo = null;
            wxcardinfo = Get(couponid, couponType);
            if (wxcardinfo != null)
            {
                if (wxcardinfo.AuditStatus == (int)WXCardLogInfo.AuditStatusEnum.Audited)
                {
                    isNeedSyncWX = true;
                }
            }
            if (isNeedSyncWX)
            {
                wxcodeinfo = GetCodeInfo(couponcodeid, couponType);
                if (wxcodeinfo != null)
                {
                    if (wxcodeinfo.CodeStatus != (int)WXCardCodeLogInfo.CodeStatusEnum.WaitReceive)
                    {
                        isNeedSyncWX = false;
                    }
                }
            }
            if (isNeedSyncWX)
            {
                result = GetWXSyncJSInfo(wxcardinfo.CardId, url);
            }

            return result;
        }
        /// <summary>
        /// 获取同步微信JS
        /// </summary>
        /// <param name="couponcodeid"></param>
        /// <param name="couponType"></param>
        /// <returns></returns>
        public WXJSCardModel GetJSWeiXinCard(long couponid, long couponcodeid, WXCardLogInfo.CouponTypeEnum couponType)
        {
            WXJSCardModel result = new WXJSCardModel();
            result.cardId = "0"; //默认不可同步
            bool isNeedSyncWX = false;
            WXCardLogInfo wxcardinfo = null;
            WXCardCodeLogInfo wxcodeinfo = null;
            wxcardinfo = Get(couponid, couponType);
            if (wxcardinfo != null)
            {
                if (wxcardinfo.AuditStatus == (int)WXCardLogInfo.AuditStatusEnum.Audited)
                {
                    isNeedSyncWX = true;
                }
            }
            if (isNeedSyncWX)
            {
                wxcodeinfo = GetCodeInfo(couponcodeid, couponType);
                if (wxcodeinfo != null)
                {
                    if (wxcodeinfo.CodeStatus != (int)WXCardCodeLogInfo.CodeStatusEnum.WaitReceive)
                    {
                        isNeedSyncWX = false;
                    }
                }
            }
            if (isNeedSyncWX)
            {
                var data = MakeSyncWXJSInfo(wxcardinfo.CardId, couponcodeid, couponType);
                if (data != null)
                {
                    result.cardId = data.card_id;
                    result.cardExt = new WXJSCardExtModel();
                    result.cardExt.signature = data.signature;
                    result.cardExt.timestamp = data.timestamp;
                    result.cardExt.nonce_str = data.nonce_str;
                    result.cardExt.outer_id = data.outerid;
                }
            }
            return result;
        }
        #endregion

        #region 核销卡券
        /// <summary>
        /// 使用卡券
        /// </summary>
        /// <param name="code"></param>
        /// <param name="cardid"></param>
        /// <returns></returns>
        public void Consume(string cardid, string code)
        {
            WXCardCodeLogInfo wxcdata = Context.WXCardCodeLogInfo.FirstOrDefault(d => d.CardId == cardid && d.Code == code);
            if (wxcdata != null)
            {
                var carddata = Context.WXCardLogInfo.FirstOrDefault(d => d.Id == wxcdata.CardLogId.Value);
                if (carddata != null)
                {
#if DEBUG
                    Core.Log.Info("开始核销卡券：" + wxcdata.Code);
#endif
                    var acctoken = GetAccessToken(carddata.AppId, carddata.AppSecret);
                    if (!string.IsNullOrWhiteSpace(acctoken) && !string.IsNullOrWhiteSpace(wxcdata.Code))
                    {
                        var wxResultJson = CardApi.CardUnavailable(acctoken, wxcdata.Code, wxcdata.CardId);
                        if (wxResultJson.errcode != Senparc.Weixin.ReturnCode.请求成功)
                        {
                            Core.Log.Error("微信同步使用卡券失败", new Exception(((int)wxResultJson.errcode).ToString() + ":" + wxResultJson.errmsg));
                        }
                    }
                }
                wxcdata.CodeStatus = (int)WXCardCodeLogInfo.CodeStatusEnum.HasConsume;
                wxcdata.UsedTime = DateTime.Now;
                Context.SaveChanges();
            }
        }
        /// <summary>
        /// 使用卡券
        /// <para>核销卡券</para>
        /// </summary>
        /// <param name="id">投放记录编号</param>
        public void Consume(long id)
        {
            WXCardCodeLogInfo wxcdata = Context.WXCardCodeLogInfo.FirstOrDefault(d => d.Id == id);
            if (wxcdata != null)
            {
                Consume(wxcdata.CardId, wxcdata.Code);
            }
        }
        /// <summary>
        /// 使用卡券
        /// <para>核销卡券</para>
        /// </summary>
        /// <param name="couponcodeid">红包记录号</param>
        /// <param name="coupontype">红包类型</param>
        public void Consume(long couponcodeid, WXCardLogInfo.CouponTypeEnum coupontype)
        {
            WXCardCodeLogInfo wxcdata = Context.WXCardCodeLogInfo.FirstOrDefault(d => d.CouponCodeId == couponcodeid && d.CouponType == coupontype);
            if (wxcdata != null)
            {
                Consume(wxcdata.CardId, wxcdata.Code);
            }
        }
        #endregion

        #region 卡券Code失效
        /// <summary>
        /// 卡券Code失效
        /// </summary>
        /// <param name="code"></param>
        /// <param name="cardid"></param>
        public void Unavailable(string cardid, string code)
        {
            WXCardCodeLogInfo wxcdata = Context.WXCardCodeLogInfo.FirstOrDefault(d => d.CardId == cardid && d.Code == code);
            if (wxcdata != null)
            {
                var carddata = Context.WXCardLogInfo.FirstOrDefault(d => d.Id == wxcdata.CardLogId.Value);
                if (carddata != null)
                {
                    var acctoken = GetAccessToken(carddata.AppId, carddata.AppSecret);
                    if (!string.IsNullOrWhiteSpace(acctoken))
                    {
                        var wxResultJson = CardApi.CardUnavailable(acctoken, wxcdata.Code, wxcdata.CardId);
                        if (wxResultJson.errcode != Senparc.Weixin.ReturnCode.请求成功)
                        {
                            Core.Log.Error("微信同步修改卡券库存失败", new Exception(((int)wxResultJson.errcode).ToString() + ":" + wxResultJson.errmsg));
                        }
                    }
                }
                wxcdata.CodeStatus = (int)WXCardCodeLogInfo.CodeStatusEnum.HasFailed;
                wxcdata.UsedTime = DateTime.Now;
                Context.SaveChanges();
            }

        }
        /// <summary>
        /// 卡券Code失效
        /// </summary>
        /// <param name="id">投放记录编号</param>
        public void Unavailable(long id)
        {
            WXCardCodeLogInfo wxcdata = Context.WXCardCodeLogInfo.FirstOrDefault(d => d.Id == id);
            if (wxcdata != null)
            {
                Unavailable(wxcdata.CardId, wxcdata.Code);
            }
        }
        #endregion

        #region 获取卡券信息
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WXCardLogInfo Get(long id)
        {
            return Context.WXCardLogInfo.FirstOrDefault(d => d.Id == id);
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WXCardLogInfo Get(string cardid)
        {
            return Context.WXCardLogInfo.FirstOrDefault(d => d.CardId == cardid);
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WXCardLogInfo Get(long couponId, WXCardLogInfo.CouponTypeEnum couponType)
        {
            return Context.WXCardLogInfo.FirstOrDefault(d => d.CouponId == couponId && d.CouponType == couponType);
        }

        /// <summary>
        /// 获取领取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WXCardCodeLogInfo GetCodeInfo(long id)
        {
            return Context.WXCardCodeLogInfo.FirstOrDefault(d => d.Id == id);
        }
        /// <summary>
        /// 获取领取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WXCardCodeLogInfo GetCodeInfo(string cardid, string code)
        {
            return Context.WXCardCodeLogInfo.FirstOrDefault(d => d.CardId == cardid && d.Code == code);
        }
        /// <summary>
        /// 获取领取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WXCardCodeLogInfo GetCodeInfo(long couponCodeId, WXCardLogInfo.CouponTypeEnum couponType)
        {
            return Context.WXCardCodeLogInfo.FirstOrDefault(d => d.CouponCodeId == couponCodeId && d.CouponType == couponType);
        }
        #endregion

        #region 事件处理
        /// <summary>
        /// 用户主动删除卡包内优惠券
        /// </summary>
        /// <param name="cardid"></param>
        /// <param name="code"></param>
        public void Event_Unavailable(string cardid, string code)
        {
            if (!string.IsNullOrWhiteSpace(cardid) && !string.IsNullOrWhiteSpace(code))
            {
                var data = Context.WXCardCodeLogInfo.FirstOrDefault(d => d.CardId == cardid && d.Code == code);
                if (data != null)
                {
                    Context.WXCardCodeLogInfo.Remove(data);
                    Context.SaveChanges();
                }
            }
        }
        /// <summary>
        /// 投放卡券
        /// <para>由事件推送调用</para>
        /// </summary>
        /// <param name="cardid"></param>
        /// <param name="code"></param>
        public void Event_Send(string cardid, string code, string openid, int outerid)
        {
            if (!string.IsNullOrWhiteSpace(cardid) && !string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(openid))
            {
                long wxcid = outerid;
                var wxcdata = Context.WXCardCodeLogInfo.FirstOrDefault(d => d.CardId == cardid && d.Id == wxcid);
                //用户主动领取不同步在系统内领取
                if (wxcdata != null)
                {
                    wxcdata.Code = code;
                    wxcdata.CodeStatus = (int)WXCardCodeLogInfo.CodeStatusEnum.Normal;
                    wxcdata.OpenId = openid;
                    wxcdata.SendTime = DateTime.Now;
                }
                Context.SaveChanges();
            }
        }
        /// <summary>
        /// 审核卡券
        /// <para>由事件推送调用</para> //TODO:审核通知事件
        /// </summary>
        /// <param name="cardid"></param>
        /// <param name="auditstatus"></param>
        public void Event_Audit(string cardid, WXCardLogInfo.AuditStatusEnum auditstatus)
        {
#if DEBUG
            Core.Log.Info(cardid + "进入审核：" + auditstatus.ToString());
#endif
            var card = Context.WXCardLogInfo.FirstOrDefault(d => d.CardId == cardid);
            if (card != null)
            {
                switch (card.CouponType)
                {
                    case WXCardLogInfo.CouponTypeEnum.Coupon:

                        if (card.CouponId.HasValue)
                        {
                            var couponser = ServiceProvider.Instance<ICouponService>.Create;
                            couponser.SyncWeixinCardAudit(card.CouponId.Value, cardid, auditstatus);
                        }
                        break;
                }
                card.AuditStatus = (int)auditstatus;
                Context.SaveChanges();
            }
        }
        #endregion
    }
}
