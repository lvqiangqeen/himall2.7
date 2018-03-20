using Himall.Core;
using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using Himall.Model;
using Senparc.Weixin.Helpers;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.Media;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Helpers;
using Senparc.Weixin.MP.AdvancedAPIs.GroupMessage;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Himall.Service.Weixin;
using Himall.Entity;
using System.IO;
using System.Dynamic;
using Himall.CommonModel;
using Himall.Core.Plugins.Message;

namespace Himall.Service
{
    public class WXMsgTemplateService : ServiceBase, IWXMsgTemplateService
    {
        #region 素材管理
        public WXUploadNewsResult Add(IEnumerable<WXMaterialInfo> info, string appid, string appsecret)
        {
            var wxHelper = new WXHelper();
            var token = wxHelper.GetAccessToken(appid, appsecret);
            var models = info.Select(e => new NewsModel
            {
                author = e.author,
                content = e.content,
                content_source_url = e.content_source_url,
                digest = e.digest,
                show_cover_pic = e.show_cover_pic,
                thumb_media_id = e.thumb_media_id,
                title = e.title
            }).ToArray();
            var uploadNewsResult = MediaApi.UploadNews(token, news: models);
            return new WXUploadNewsResult { errmsg = uploadNewsResult.errmsg, media_id = uploadNewsResult.media_id };
        }

        public WxJsonResult DeleteMedia(string mediaid, string appid, string appsecret)
        {
            var wxHelper = new WXHelper();
            var token = wxHelper.GetAccessToken(appid, appsecret);
            var result = MediaApi.DeleteForeverMedia(token, mediaid);
            return new WxJsonResult() { errmsg = result.errmsg };

        }

        public IEnumerable<WxJsonResult> UpdateMedia(string mediaid, IEnumerable<WXMaterialInfo> news, string appid, string appsecret)
        {
            var wxHelper = new WXHelper();
            var token = wxHelper.GetAccessToken(appid, appsecret);
            int idx = 0;
            List<WxJsonResult> resultList = new List<WxJsonResult>();
            foreach (var model in news)
            {
                var result = MediaApi.UpdateForeverNews(token, mediaid, idx, new NewsModel()
                {
                    author = model.author,
                    content = model.content,
                    content_source_url = model.content_source_url,
                    digest = model.digest,
                    show_cover_pic = model.show_cover_pic,
                    thumb_media_id = model.thumb_media_id,
                    title = model.title
                });
                resultList.Add(new WxJsonResult { errmsg = result.errmsg });
            }
            return resultList;
        }
        public string AddImage(string filename, string appid, string appsecret)
        {
            var wxHelper = new WXHelper();
            var token = wxHelper.GetAccessToken(appid, appsecret);
            var uploadResult = MediaApi.UploadForeverMedia(token, filename);
            return uploadResult.media_id;
        }
        public IEnumerable<WXMaterialInfo> GetMedia(string mediaid, string appid, string appsecret)
        {
            var wxHelper = new WXHelper();
            var token = wxHelper.GetAccessToken(appid, appsecret);
            var mediaNews = MediaApi.GetForeverNews(token, mediaid).news_item.Select(e => new WXMaterialInfo
            {
                author = e.author,
                title = e.title,
                thumb_media_id = e.thumb_media_id,
                show_cover_pic = e.show_cover_pic,
                digest = e.digest,
                content_source_url = e.content_source_url,
                content = e.content,
                url = e.url
            });

            return mediaNews;
        }
        public void GetMedia(string mediaid, string appid, string appsecret, Stream stream)
        {
            var wxHelper = new WXHelper();
            var token = wxHelper.GetAccessToken(appid, appsecret);
            MediaApi.GetForeverMedia(token, mediaid, stream);
        }
        public MediaNewsItemList GetMediaMsgTemplateList(string appid, string appsecret, int offset, int count)
        {
            var wxHelper = new WXHelper();
            var token = wxHelper.GetAccessToken(appid, appsecret);
            var mediaList = MediaApi.GetNewsMediaList(token, offset, count);

            var tempList = new MediaNewsItemList
            {
                count = mediaList.item_count,
                total_count = mediaList.total_count,
                content = mediaList.item == null ? null : mediaList.item.Select(e => new MediaNewsItem
                {
                    media_id = e.media_id,
                    items = e.content.news_item.Select(item => new WXMaterialInfo
                    {
                        author = item.author,
                        title = item.title,
                        thumb_media_id = item.thumb_media_id,
                        show_cover_pic = item.show_cover_pic,
                        digest = item.digest,
                        content_source_url = item.content_source_url,
                        content = item.content,
                        url = item.url
                    }),
                    update_time = DateTime.Parse("1970-01-01").AddSeconds(e.update_time).ToString()
                }),
                errCode = mediaList.errcode.ToString(),
                errMsg = mediaList.errmsg
            };
            return tempList;
        }

        public MediaItemCount GetMediaItemCount(string appid, string appsecret)
        {
            var wxHelper = new WXHelper();
            var token = wxHelper.GetAccessToken(appid, appsecret);
            var result = MediaApi.GetMediaCount(token);
            var itemcount = new MediaItemCount
            {
                image_count = result.image_count,
                news_count = result.news_count,
                video_count = result.video_count,
                voice_count = result.voice_count,
                errMsg = result.errmsg,
                errCode = result.errcode.ToString()
            };
            return itemcount;
        }
        #endregion 素材管理

        #region 群发消息
        public SendInfoResult SendWXMsg(SendMsgInfo info)
        {
            var toUsers = GetToUser(info.ToUserLabel);
            if (toUsers.Length > 0)
            {
                if (toUsers.Length == 1)
                {
                    return new SendInfoResult { errCode = "群发微信消息，至少需要2个发送对象！", errMsg = "" };
                }
                var wxHelper = new WXHelper();
                var token = wxHelper.GetAccessToken(info.AppId, info.AppSecret);
                var sendResult = GroupMessageApi.SendGroupMessageByOpenId(token, (GroupMessageType)info.MsgType, info.MsgType == WXMsgType.text ? info.Content : info.MediaId, openIds: toUsers);
                if (!string.IsNullOrWhiteSpace(sendResult.msg_id))
                {
                    SendMessageRecordInfo model = new SendMessageRecordInfo()
                    {
                        ContentType = info.MsgType,
                        MessageType = MsgType.WeiXin,
                        SendContent = info.Content,
                        SendTime = DateTime.Now,
                        ToUserLabel = info.ToUserDesc,
                        SendState = 1
                    };
                    Context.SendMessageRecordInfo.Add(model);
                    Context.SaveChanges();
                }
                return new SendInfoResult { errCode = sendResult.errcode.ToString(), errMsg = sendResult.errmsg };
            }
            else
            {
                return new SendInfoResult { errCode = "未找到符合条件的发送对象！", errMsg = "" };
            }
        }


        public SendInfoResult SendWXMsg(IEnumerable<string> openIds, WXMsgType msgType, string content, string mediaId, string appId, string appSecret)
        {
            if (openIds.Count() <= 1)
            {
                return new SendInfoResult { errCode = "群发微信消息，至少需要2个发送对象！", errMsg = "" };
            }

            var wxHelper = new WXHelper();
            var token = wxHelper.GetAccessToken(appId, appSecret);
            var sendResult = GroupMessageApi.SendGroupMessageByOpenId(token, (GroupMessageType)msgType, msgType == WXMsgType.text ? content : mediaId, openIds: openIds.ToArray());
            return new SendInfoResult { errCode = sendResult.errcode.ToString(), errMsg = sendResult.errmsg };
        }

        private string[] GetToUser(SendToUserLabel labelinfo)
        {
            var memOpenid = (from m in Context.MemberOpenIdInfo
                             join o in Context.OpenIdsInfo on m.OpenId equals o.OpenId
                             join u in Context.UserMemberInfo on m.UserId equals u.Id
                             where o.IsSubscribe
                             select new
                             {
                                 userid = m.UserId,
                                 openid = m.OpenId,
                                 regionid = u.TopRegionId,
                                 sex = u.Sex
                             });

            if (labelinfo.ProvinceId.HasValue)
            {
                memOpenid = memOpenid.Where(e => e.regionid == labelinfo.ProvinceId.Value);
            }
            if (labelinfo.Sex.HasValue)
            {
                memOpenid = memOpenid.Where(e => e.sex == labelinfo.Sex.Value);
            }
            if (labelinfo.LabelIds != null && labelinfo.LabelIds.Length > 0)
            {
                memOpenid = (from m in Context.MemberLabelInfo
                             join u in memOpenid on m.MemId equals u.userid
                             join l in labelinfo.LabelIds on m.LabelId equals l
                             select u);
            }

            return memOpenid.Select(e => e.openid).Distinct().ToArray();
        }

        public SendMessageRecordInfo AddSendRecord(SendMessageRecordInfo info)
        {
            var item = Context.SendMessageRecordInfo.Add(info);
            Context.SaveChanges();
            return item;
        }

        public QueryPageModel<SendMessageRecordInfo> GetSendRecords(IServices.QueryModel.SendRecordQuery query)
        {
            IQueryable<SendMessageRecordInfo> sendRecords = Context.SendMessageRecordInfo.AsQueryable();
            if (query.msgType.HasValue)
            {
                var msgType = query.msgType.Value;
                sendRecords = sendRecords.Where(p => p.MessageType == (MsgType)msgType);
            }
            if (query.sendState.HasValue)
            {
                sendRecords = sendRecords.Where(e => e.SendState == query.sendState.Value);
            }
            if (query.startDate.HasValue)
            {
                DateTime sdt = query.startDate.Value;
                sendRecords = sendRecords.Where(d => d.SendTime >= sdt);
            }
            if (query.endDate.HasValue)
            {
                DateTime edt = query.endDate.Value.AddDays(1);
                sendRecords = sendRecords.Where(d => d.SendTime < edt);
            }
            int total = 0;
            sendRecords = sendRecords.GetPage(out total, query.PageNo, query.PageSize);
            QueryPageModel<SendMessageRecordInfo> pageModel = new QueryPageModel<SendMessageRecordInfo>() { Models = sendRecords.ToList(), Total = total };
            return pageModel;
        }
        public List<SendmessagerecordCouponSNInfo> GetSendrecordCouponSnById(long id)
        {
            var datalist = new List<FightGroupOrderInfo>();
            var data = Context.SendmessagerecordCouponSNInfo.AsQueryable();
            if (id > 0)
            {
                data = data.Where(p => p.MessageId == id);
            }
            return data.ToList();
        }
        public CouponRecordInfo GetCouponRecordBySn(string CouponSn)
        {
            var data = Context.CouponRecordInfo.AsNoTracking().FirstOrDefault(p => p.CounponSN == CouponSn);
            return data;
        }
        #endregion  群发消息

        #region 模板消息
        /// <summary>
        /// 获取微信模板消息列表
        /// </summary>
        /// <returns></returns>
        public List<WeiXinMsgTemplateInfo> GetWeiXinMsgTemplateList()
        {
            List<WeiXinMsgTemplateInfo> result = new List<WeiXinMsgTemplateInfo>();
            result = Context.WeiXinMsgTemplateInfo.ToList();
            bool isHaveSave = false;
            //初始表
            var _wxmsglist = Himall.Model.WeiXin.WX_MsgTemplateLinkData.GetList();
            foreach (var item in _wxmsglist)
            {
                int _tmptype = (int)item.MsgType;
                var _tmpmsg = result.FirstOrDefault(d => d.MessageType == _tmptype);
                if (_tmpmsg == null)
                {
                    isHaveSave = true;
                    result.Add(GetWeiXinMsgTemplate(item.MsgType));
                }
                else
                {
                    if (_tmpmsg.TemplateNum != item.MsgTemplateShortId)
                    {
                        isHaveSave = true;
                        //数据修正
                        _tmpmsg.TemplateNum = item.MsgTemplateShortId;
                        _tmpmsg.TemplateId = "";   //如果模板对应出错，需重置
                    }
                }
            }
            if (isHaveSave)
            {
                //context.WeiXinMsgTemplateInfo.AddRange(waitadds);
                Context.SaveChanges();
                result = Context.WeiXinMsgTemplateInfo.ToList();
            }
            return result;
        }

        
        /// <summary>
        /// 获取微信模板信息
        /// </summary>
        /// <returns></returns>
        public WeiXinMsgTemplateInfo GetWeiXinMsgTemplate(Himall.Core.Plugins.Message.MessageTypeEnum type)
        {
            int msgtype = (int)type;
            WeiXinMsgTemplateInfo result = Context.WeiXinMsgTemplateInfo.FirstOrDefault(d => d.MessageType == msgtype);
            var _tmp = Himall.Model.WeiXin.WX_MsgTemplateLinkData.GetList().FirstOrDefault(d => d.MsgType == type);
            if (result == null)
            {
                result = new WeiXinMsgTemplateInfo();
                result.MessageType = msgtype;
                if (_tmp != null)
                {
                    result.TemplateNum = _tmp.MsgTemplateShortId;
                }
                result.UpdateDate = DateTime.Now;
                result.IsOpen = false;
                Context.WeiXinMsgTemplateInfo.Add(result);
                Context.SaveChanges();
            }
            return result;
        }


        /// <summary>
        /// 获取小程序微信模板消息列表
        /// </summary>
        /// <returns></returns>
        public List<WeiXinMsgTemplateInfo> GetWeiXinMsgTemplateListByApplet()
        {
            List<WeiXinMsgTemplateInfo> result = new List<WeiXinMsgTemplateInfo>();
            result = Context.WeiXinMsgTemplateInfo.Where(d => d.UserInWxApplet == 1).ToList();
            bool isHaveSave = false;
            //初始表
            var _wxmsglist = Himall.Model.WeiXin.WXApplet_MsgTemplateLinkData.GetList();
            foreach (var item in _wxmsglist)
            {
                int _tmptype = (int)item.MsgType;
                var _tmpmsg = result.FirstOrDefault(d => d.MessageType == _tmptype);
                if (_tmpmsg == null)
                {
                    isHaveSave = true;
                    result.Add(GetWeiXinMsgTemplateByApplet(item.MsgType));
                }
                else
                {
                    if (_tmpmsg.TemplateNum != item.MsgTemplateShortId)
                    {
                        isHaveSave = true;
                        //数据修正
                        _tmpmsg.TemplateNum = item.MsgTemplateShortId;
                        _tmpmsg.TemplateId = "";   //如果模板对应出错，需重置
                    }
                }
            }
            if (isHaveSave)
            {
                //context.WeiXinMsgTemplateInfo.AddRange(waitadds);
                Context.SaveChanges();
                result = Context.WeiXinMsgTemplateInfo.Where(d => d.UserInWxApplet == 1).ToList();
            }
            return result;
        }
        /// <summary>
        /// 获取小程序微信模版
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public WeiXinMsgTemplateInfo GetWeiXinMsgTemplateByApplet(Himall.Core.Plugins.Message.MessageTypeEnum type)
        {
            int msgtype = (int)type;
            WeiXinMsgTemplateInfo result = Context.WeiXinMsgTemplateInfo.Where(d=>d.UserInWxApplet==1).FirstOrDefault(d => d.MessageType == msgtype);
            var _tmp = Himall.Model.WeiXin.WXApplet_MsgTemplateLinkData.GetList().FirstOrDefault(d => d.MsgType == type);
            if (result == null)
            {
                result = new WeiXinMsgTemplateInfo();
                result.MessageType = msgtype;
                if (_tmp != null)
                {
                    result.TemplateNum = _tmp.MsgTemplateShortId;
                }
                result.UpdateDate = DateTime.Now;
                result.IsOpen = false;
                result.UserInWxApplet = 1;
                Context.WeiXinMsgTemplateInfo.Add(result);
                Context.SaveChanges();
            }
            return result;
        }
        /// <summary>
        /// 设置微信模板消息配置
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public WeiXinMsgTemplateInfo UpdateWeiXinMsgTemplate(WeiXinMsgTemplateInfo info)
        {
            Himall.Core.Plugins.Message.MessageTypeEnum _msgtype = (Himall.Core.Plugins.Message.MessageTypeEnum)info.MessageType;
            WeiXinMsgTemplateInfo data = GetWeiXinMsgTemplate(_msgtype);
            if (data != null)
            {
                info.Id = data.Id;
                data.TemplateId = info.TemplateId;
                data.MessageType = info.MessageType;
                data.TemplateId = info.TemplateId;
                data.UpdateDate = info.UpdateDate;
                data.IsOpen = info.IsOpen;
            }
            else
            {
                Context.WeiXinMsgTemplateInfo.Add(info);
            }
            Context.SaveChanges();
            return info;
        }
        /// <summary>
        /// 设置微信消息开启状态
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isOpen"></param>
        public void UpdateWeiXinMsgOpenState(Himall.Core.Plugins.Message.MessageTypeEnum type, bool isOpen)
        {
            WeiXinMsgTemplateInfo data = GetWeiXinMsgTemplate(type);
            data.MessageType = (int)type;
            data.IsOpen = isOpen;
            Context.SaveChanges();
        }
        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId">为0时使用openid</param>
        /// <param name="data">信息数据</param>
        /// <param name="url"></param>
        /// <param name="openid">与userid配合使用，userid为0时使用此字段</param>
        public void SendMessageByTemplate(Himall.Core.Plugins.Message.MessageTypeEnum type, long userId, WX_MsgTemplateSendDataModel data, string url = "", string wxopenid = "")
        {
            var siteSetting = Himall.ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
            string appid = siteSetting.WeixinAppId;
            string appsecret = siteSetting.WeixinAppSecret;
            if (string.IsNullOrWhiteSpace(appid) || string.IsNullOrWhiteSpace(appsecret))
            {
                throw new HimallException("未配置微信公众信息");
            }
            string dataerr = "";
#if DEBUG
            Core.Log.Info("[模板消息]开始准备数据：" + userId.ToString() + "[" + type.ToDescription() + "]");
#endif
            bool isdataok = true;
            string openId = wxopenid;
            if (userId == 0)
            {
                if (string.IsNullOrWhiteSpace(wxopenid))
                {
                    throw new HimallException("错误的OpenId");
                }
                openId = wxopenid;
            }
            else
            {
                openId = GetPlatformOpenIdByUserId(userId);
            }
            if (string.IsNullOrWhiteSpace(openId))
            {
                dataerr = "openid为空";
                isdataok = false;
            }
            var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
            if (userId != 0)
            {
                if (userinfo == null)
                {
                    dataerr = "用户信息未取到" + userId;
                    isdataok = false;
                }
            }
            var _msgtmplinfo = GetWeiXinMsgTemplate(type);
            if (_msgtmplinfo == null)
            {
                dataerr = "消息模板未取到";
                isdataok = false;
            }
            string templateId = "";
            string topcolor = "#000000";
            if (isdataok)
            {
                templateId = _msgtmplinfo.TemplateId;
                if (string.IsNullOrWhiteSpace(templateId))
                {
                    dataerr = "消息模板未取到";
                    isdataok = false;
                }
            }
            if (!_msgtmplinfo.IsOpen)
            {
                dataerr = "未开启";
                isdataok = false;
            }

            if (isdataok)
            {
#if DEBUG
                Core.Log.Info("[模板消息]开始发送前");
#endif
                object msgdata;
                switch (type)
                {
                    case Core.Plugins.Message.MessageTypeEnum.OrderCreated:
                        #region 创建订单(买家)
                        var _ocmsgdata = new WX_MsgTemplateKey3DataModel();
                        _ocmsgdata.first.value = data.first.value;
                        _ocmsgdata.first.color = data.first.color;
                        _ocmsgdata.keyword1.value = data.keyword1.value;
                        _ocmsgdata.keyword1.color = data.keyword1.color;
                        _ocmsgdata.keyword2.value = data.keyword2.value;
                        _ocmsgdata.keyword2.color = data.keyword2.color;
                        _ocmsgdata.keyword3.value = data.keyword3.value;
                        _ocmsgdata.keyword3.color = data.keyword3.color;
                        _ocmsgdata.remark.value = data.remark.value;
                        _ocmsgdata.remark.color = data.remark.color;
                        msgdata = _ocmsgdata;
                        break;
                    #endregion
                    case Core.Plugins.Message.MessageTypeEnum.OrderPay:
                        #region 订单支付(买家)
                        var _opmsgdata = new WX_MsgTemplateKey4DataModel();
                        _opmsgdata.first.value = data.first.value;
                        _opmsgdata.first.color = data.first.color;
                        _opmsgdata.keyword1.value = data.keyword1.value;
                        _opmsgdata.keyword1.color = data.keyword1.color;
                        _opmsgdata.keyword2.value = data.keyword2.value;
                        _opmsgdata.keyword2.color = data.keyword2.color;
                        _opmsgdata.keyword3.value = data.keyword3.value;
                        _opmsgdata.keyword3.color = data.keyword3.color;
                        _opmsgdata.keyword4.value = data.keyword4.value;
                        _opmsgdata.keyword4.color = data.keyword4.color;
                        _opmsgdata.remark.value = data.remark.value;
                        _opmsgdata.remark.color = data.remark.color;
                        msgdata = _opmsgdata;
                        break;
                    #endregion
                    case Core.Plugins.Message.MessageTypeEnum.OrderShipping:
                        #region 订单发货(买家)
                        var _osmsgdata = new WX_MsgTemplateKey4DataModel();
                        _osmsgdata.first.value = data.first.value;
                        _osmsgdata.first.color = data.first.color;
                        _osmsgdata.keyword1.value = data.keyword1.value;
                        _osmsgdata.keyword1.color = data.keyword1.color;
                        _osmsgdata.keyword2.value = data.keyword2.value;
                        _osmsgdata.keyword2.color = data.keyword2.color;
                        _osmsgdata.keyword3.value = data.keyword3.value;
                        _osmsgdata.keyword3.color = data.keyword3.color;
                        _osmsgdata.keyword4.value = data.keyword4.value;
                        _osmsgdata.keyword4.color = data.keyword4.color;
                        _osmsgdata.remark.value = data.remark.value;
                        _osmsgdata.remark.color = data.remark.color;
                        msgdata = _osmsgdata;
                        break;
                    #endregion
                    case Core.Plugins.Message.MessageTypeEnum.OrderRefund:
                        #region 退款退货(买家)
                        var _ormsgdata = new WX_MsgTemplateRefundDataModel();
                        _ormsgdata.first.value = data.first.value;
                        _ormsgdata.first.color = data.first.color;
                        _ormsgdata.orderProductPrice.value = data.keyword1.value;
                        _ormsgdata.orderProductPrice.color = data.keyword1.color;
                        _ormsgdata.orderProductName.value = data.keyword2.value;
                        _ormsgdata.orderProductName.color = data.keyword2.color;
                        _ormsgdata.orderName.value = data.keyword3.value;
                        _ormsgdata.orderName.color = data.keyword3.color;
                        _ormsgdata.remark.value = data.remark.value;
                        _ormsgdata.remark.color = data.remark.color;
                        msgdata = _ormsgdata;
                        break;
                    #endregion
                    case Core.Plugins.Message.MessageTypeEnum.RefundDeliver:
                        #region 退款退货(买家)
                        var _rdmsgdata = new WX_MsgTemplateKey5DataModel();
                        _rdmsgdata.first.value = data.first.value;
                        _rdmsgdata.first.color = data.first.color;
                        _rdmsgdata.keyword1.value = data.keyword1.value;
                        _rdmsgdata.keyword1.color = data.keyword1.color;
                        _rdmsgdata.keyword2.value = data.keyword2.value;
                        _rdmsgdata.keyword2.color = data.keyword2.color;
                        _rdmsgdata.keyword3.value = data.keyword3.value;
                        _rdmsgdata.keyword3.color = data.keyword3.color;
                        _rdmsgdata.keyword4.value = data.keyword4.value;
                        _rdmsgdata.keyword4.color = data.keyword4.color;
                        _rdmsgdata.keyword5.value = data.keyword5.value;
                        _rdmsgdata.keyword5.color = data.keyword5.color;
                        _rdmsgdata.remark.value = data.remark.value;
                        _rdmsgdata.remark.color = data.remark.color;
                        msgdata = _rdmsgdata;
                        break;
                    #endregion
                    case Core.Plugins.Message.MessageTypeEnum.ShopHaveNewOrder:
                        #region 店铺有新订单(卖家)
                        var _shnomsgdata = new WX_MsgTemplateKey5DataModel();
                        _shnomsgdata.first.value = data.first.value;
                        _shnomsgdata.first.color = data.first.color;
                        _shnomsgdata.keyword1.value = data.keyword1.value;
                        _shnomsgdata.keyword1.color = data.keyword1.color;
                        _shnomsgdata.keyword2.value = data.keyword2.value;
                        _shnomsgdata.keyword2.color = data.keyword2.color;
                        _shnomsgdata.keyword3.value = data.keyword3.value;
                        _shnomsgdata.keyword3.color = data.keyword3.color;
                        _shnomsgdata.keyword4.value = data.keyword4.value;
                        _shnomsgdata.keyword4.color = data.keyword4.color;
                        _shnomsgdata.keyword5.value = data.keyword5.value;
                        _shnomsgdata.keyword5.color = data.keyword5.color;
                        _shnomsgdata.remark.value = data.remark.value;
                        _shnomsgdata.remark.color = data.remark.color;
                        msgdata = _shnomsgdata;
                        break;
                    #endregion
                    case Core.Plugins.Message.MessageTypeEnum.ReceiveBonus:
                        #region 领取现金红包
                        var _rbmsgdata = new WX_MSGGetCouponModel();
                        _rbmsgdata.first.value = data.first.value;
                        _rbmsgdata.first.color = data.first.color;
                        _rbmsgdata.toName.value = data.keyword1.value;
                        _rbmsgdata.toName.color = data.keyword1.color;
                        _rbmsgdata.gift.value = data.keyword2.value;
                        _rbmsgdata.gift.color = data.keyword2.color;
                        _rbmsgdata.time.value = data.keyword3.value;
                        _rbmsgdata.time.color = data.keyword3.color;
                        _rbmsgdata.remark.value = data.remark.value;
                        _rbmsgdata.remark.color = data.remark.color;
                        msgdata = _rbmsgdata;
                        break;
                    #endregion
                    case Core.Plugins.Message.MessageTypeEnum.LimitTimeBuy:
                        #region 限时购开始
                        var _ltbmsgdata = new WX_MsgTemplateKey4DataModel();
                        _ltbmsgdata.first.value = data.first.value;
                        _ltbmsgdata.first.color = data.first.color;
                        _ltbmsgdata.keyword1.value = data.keyword1.value;
                        _ltbmsgdata.keyword1.color = data.keyword1.color;
                        _ltbmsgdata.keyword2.value = data.keyword2.value;
                        _ltbmsgdata.keyword2.color = data.keyword2.color;
                        _ltbmsgdata.keyword3.value = data.keyword3.value;
                        _ltbmsgdata.keyword3.color = data.keyword3.color;
                        _ltbmsgdata.keyword4.value = data.keyword4.value;
                        _ltbmsgdata.keyword4.color = data.keyword4.color;
                        _ltbmsgdata.remark.value = data.remark.value;
                        _ltbmsgdata.remark.color = data.remark.color;
                        msgdata = _ltbmsgdata;
                        break;
                    #endregion
                    case Core.Plugins.Message.MessageTypeEnum.SubscribeLimitTimeBuy:
                        #region 订阅限时购
                        var _sltbmsgdata = new WX_MsgTemplateKey3DataModel();
                        _sltbmsgdata.first.value = data.first.value;
                        _sltbmsgdata.first.color = data.first.color;
                        _sltbmsgdata.keyword1.value = data.keyword1.value;
                        _sltbmsgdata.keyword1.color = data.keyword1.color;
                        _sltbmsgdata.keyword2.value = data.keyword2.value;
                        _sltbmsgdata.keyword2.color = data.keyword2.color;
                        _sltbmsgdata.keyword3.value = data.keyword3.value;
                        _sltbmsgdata.keyword3.color = data.keyword3.color;
                        _sltbmsgdata.remark.value = data.remark.value;
                        _sltbmsgdata.remark.color = data.remark.color;
                        msgdata = _sltbmsgdata;
                        break;
                    #endregion
                    case Core.Plugins.Message.MessageTypeEnum.FightGroupOpenSuccess:
                        #region 开团成功
                        var _fgosmsgdata = new WX_MsgTemplateKey3DataModel();
                        _fgosmsgdata.first.value = data.first.value;
                        _fgosmsgdata.first.color = data.first.color;
                        _fgosmsgdata.keyword1.value = data.keyword1.value;
                        _fgosmsgdata.keyword1.color = data.keyword1.color;
                        _fgosmsgdata.keyword2.value = data.keyword2.value;
                        _fgosmsgdata.keyword2.color = data.keyword2.color;
                        _fgosmsgdata.keyword3.value = data.keyword3.value;
                        _fgosmsgdata.keyword3.color = data.keyword3.color;
                        _fgosmsgdata.remark.value = data.remark.value;
                        _fgosmsgdata.remark.color = data.remark.color;
                        msgdata = _fgosmsgdata;
                        break;
                    #endregion
                    case Core.Plugins.Message.MessageTypeEnum.FightGroupJoinSuccess:
                        #region 参团成功
                        var _fgjsmsgdata = new WX_MsgTemplateKey3DataModel();
                        _fgjsmsgdata.first.value = data.first.value;
                        _fgjsmsgdata.first.color = data.first.color;
                        _fgjsmsgdata.keyword1.value = data.keyword1.value;
                        _fgjsmsgdata.keyword1.color = data.keyword1.color;
                        _fgjsmsgdata.keyword2.value = data.keyword2.value;
                        _fgjsmsgdata.keyword2.color = data.keyword2.color;
                        _fgjsmsgdata.keyword3.value = data.keyword3.value;
                        _fgjsmsgdata.keyword3.color = data.keyword3.color;
                        _fgjsmsgdata.remark.value = data.remark.value;
                        _fgjsmsgdata.remark.color = data.remark.color;
                        msgdata = _fgjsmsgdata;
                        #endregion
                        break;
                    case Core.Plugins.Message.MessageTypeEnum.FightGroupNewJoin:
                        #region 新成员参团成功
                        var _fgnjmsgdata = new WX_MsgFightGroupNewJoinDataModel();
                        _fgnjmsgdata.first.value = data.first.value;
                        _fgnjmsgdata.first.color = data.first.color;
                        _fgnjmsgdata.time.value = data.keyword1.value;
                        _fgnjmsgdata.time.color = data.keyword1.color;
                        _fgnjmsgdata.number.value = data.keyword2.value;
                        _fgnjmsgdata.number.color = data.keyword2.color;
                        _fgnjmsgdata.remark.value = data.remark.value;
                        _fgnjmsgdata.remark.color = data.remark.color;
                        msgdata = _fgnjmsgdata;
                        #endregion
                        break;
                    case Core.Plugins.Message.MessageTypeEnum.FightGroupFailed:
                        #region 拼团失败
                        var _fgfmsgdata = new WX_MsgTemplateKey3DataModel();
                        _fgfmsgdata.first.value = data.first.value;
                        _fgfmsgdata.first.color = data.first.color;
                        _fgfmsgdata.keyword1.value = data.keyword1.value;
                        _fgfmsgdata.keyword1.color = data.keyword1.color;
                        _fgfmsgdata.keyword2.value = data.keyword2.value;
                        _fgfmsgdata.keyword2.color = data.keyword2.color;
                        _fgfmsgdata.keyword3.value = data.keyword3.value;
                        _fgfmsgdata.keyword3.color = data.keyword3.color;
                        _fgfmsgdata.remark.value = data.remark.value;
                        _fgfmsgdata.remark.color = data.remark.color;
                        msgdata = _fgfmsgdata;
                        #endregion
                        break;
                    case Core.Plugins.Message.MessageTypeEnum.FightGroupSuccess:
                        #region 拼团成功
                        var _fgsmsgdata = new WX_MsgTemplateKey2DataModel();
                        _fgsmsgdata.first.value = data.first.value;
                        _fgsmsgdata.first.color = data.first.color;
                        _fgsmsgdata.keyword1.value = data.keyword1.value;
                        _fgsmsgdata.keyword1.color = data.keyword1.color;
                        _fgsmsgdata.keyword2.value = data.keyword2.value;
                        _fgsmsgdata.keyword2.color = data.keyword2.color;
                        _fgsmsgdata.remark.value = data.remark.value;
                        _fgsmsgdata.remark.color = data.remark.color;
                        msgdata = _fgsmsgdata;
                        #endregion
                        break;
                    default:
                        throw new HimallException("无此模板消息，不能完成消息推送");
                        break;
                }

#if DEBUG
                Core.Log.Info("[模板消息]开始发送到openid:" + openId);
#endif
                var wxhelper = new Weixin.WXHelper();
                var accessToken = wxhelper.GetAccessToken(appid, appsecret);
#if DEBUG
                Core.Log.Info("[模板消息]取到Token");
#endif
                var _result = TemplateApi.SendTemplateMessage(accessToken, openId, templateId, topcolor, url, msgdata);
                //小程序发送消息接口
                // Senparc.Weixin.WxOpen.AdvancedAPIs.Template.TemplateApi.SendTemplateMessage(accessToken, openId, templateId,  msgdata,formId:"");
                if (_result.errcode != Senparc.Weixin.ReturnCode.请求成功)
                {
                    Core.Log.Info(_result.errcode.ToString() + ":" + _result.errmsg);
                }

#if DEBUG
                Core.Log.Info("[模板消息]发送结束");
#endif
            }
            else
            {

#if DEBUG
                Core.Log.Info("[模板消息]发送失败：数据验证未通过-" + dataerr + "[" + type.ToDescription() + "]");
#endif
            }
        }

        /// <summary>
        /// 获取模板消息跳转URL
        /// </summary>
        /// <param name="type"></param>
        public string GetMessageTemplateShowUrl(Himall.Core.Plugins.Message.MessageTypeEnum type)
        {
            string result = "";
            var _tmplinkdata = Himall.Model.WeiXin.WX_MsgTemplateLinkData.GetList().FirstOrDefault(d => d.MsgType == type);
            if (_tmplinkdata != null)
            {
                if (!string.IsNullOrWhiteSpace(_tmplinkdata.ReturnUrl))
                {
                    result = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];
                    result += _tmplinkdata.ReturnUrl;
                }
            }
            return result;
        }
        /// <summary>
        /// 取当前用户对应平台的OpenId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private string GetPlatformOpenIdByUserId(long userId)
        {
            string result = "";
            if (userId > 0)
            {
                var _tmp = (from mo in Context.MemberOpenIdInfo
                            where mo.ServiceProvider == "Himall.Plugin.OAuth.WeiXin" && mo.AppIdType == MemberOpenIdInfo.AppIdTypeEnum.Payment
                            select mo).FirstOrDefault(d => d.UserId == userId);
                if (_tmp != null)
                {
                    result = _tmp.OpenId;
                }
            }
            return result;
        }
        /// <summary>
        /// 添加消息模板
        /// </summary>
        /// <param name="type">null表示所有都重置</param>
        public void AddMessageTemplate(Himall.Core.Plugins.Message.MessageTypeEnum? type = null)
        {
            var siteSetting = Himall.ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
            string appid = siteSetting.WeixinAppId;
            string appsecret = siteSetting.WeixinAppSecret;
            var wxhelper = new Weixin.WXHelper();
            var accessToken = wxhelper.GetAccessToken(appid, appsecret);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new HimallException("获取Token失败");
            }
            List<WeiXinMsgTemplateInfo> msgtmplist = new List<WeiXinMsgTemplateInfo>();
            if (type != null)
            {
                WeiXinMsgTemplateInfo _tmp = GetWeiXinMsgTemplate(type.Value);
                if (_tmp != null)
                {
                    msgtmplist.Add(_tmp);
                }
            }
            else
            {
                msgtmplist = GetWeiXinMsgTemplateList();
            }
            foreach (var item in msgtmplist)
            {
                var rdata = TemplateApi.AddMessageTemplate(accessToken, item.TemplateNum);
                if (rdata.errcode == Senparc.Weixin.ReturnCode.请求成功)
                {
                    item.TemplateId = rdata.template_id;
                }
                else
                {
                    item.TemplateId = "";   //重置失败会清理之前的值
                    Core.Log.Info("[重置消息模板]" + item.MessageType.ToString() + ":" + rdata.errcode.ToString() + " - " + rdata.errmsg);
                }
                item.UpdateDate = DateTime.Now;
            }
            Context.SaveChanges();
        }
        #endregion

        public void UpdateWXsmallMessage(IEnumerable<KeyValuePair<string, string>> items)
        {
            foreach (var model in items)
            {
                WeiXinMsgTemplateInfo data = GetWeiXinMsgTemplateById(long.Parse(model.Key));
                data.TemplateId =model.Value;
                Context.SaveChanges();
            }
        }

        /// <summary>
        /// 获取微信模板信息
        /// </summary>
        /// <returns></returns>
        public WeiXinMsgTemplateInfo GetWeiXinMsgTemplateById(long Id)
        {
            WeiXinMsgTemplateInfo result = Context.WeiXinMsgTemplateInfo.FirstOrDefault(d => d.Id == Id);            
            return result;
        }


        #region 小程序消息模版
        public string GetWXAppletMessageTemplateShowUrl(Himall.Core.Plugins.Message.MessageTypeEnum type)
        {
            string result = "";
            var _tmplinkdata = Himall.Model.WeiXin.WXApplet_MsgTemplateLinkData.GetList().FirstOrDefault(d => d.MsgType == type);
            if (_tmplinkdata != null)
            {
                //if (!string.IsNullOrWhiteSpace(_tmplinkdata.ReturnUrl))
                //{
                //    result = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];//获取域名
                //    result += _tmplinkdata.ReturnUrl;
                //}
                result = _tmplinkdata.ReturnUrl;
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 获取微信模板信息
        /// </summary>
        /// <returns></returns>
        public WeiXinMsgTemplateInfo GetWXAppletMsgTemplate(Himall.Core.Plugins.Message.MessageTypeEnum type)
        {
            int msgtype = (int)type;
            WeiXinMsgTemplateInfo result = Context.WeiXinMsgTemplateInfo.FirstOrDefault(d => d.MessageType == msgtype && d.UserInWxApplet==1);
            return result;
        }

        /// <summary>
        /// 小程序发送模板消息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId">为0时使用openid</param>
        /// <param name="data">信息数据</param>
        /// <param name="url"></param>
        /// <param name="openid">与userid配合使用，userid为0时使用此字段</param>
        public void SendAppletMessageByTemplate(Himall.Core.Plugins.Message.MessageTypeEnum type, long userId, WX_MsgTemplateSendDataModel data, string url = "", string wxopenid = "",string formId="")
        {
            var siteSetting = Himall.ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
            string appid = siteSetting.WeixinAppletId;
            string appsecret = siteSetting.WeixinAppletSecret;
            if (string.IsNullOrWhiteSpace(appid) || string.IsNullOrWhiteSpace(appsecret))
            {
                throw new HimallException("未配置微信公众信息");
            }
            string dataerr = "";
#if DEBUG
            Core.Log.Info("[模板消息]开始准备数据：" + userId.ToString() + "[" + type.ToDescription() + "]");
#endif
            bool isdataok = true;
            string openId = wxopenid;
            if (userId == 0)
            {
                if (string.IsNullOrWhiteSpace(wxopenid))
                {
                    throw new HimallException("错误的OpenId");
                }
                openId = wxopenid;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(wxopenid))
                {
                    openId = wxopenid;
                }
            }
            if (string.IsNullOrWhiteSpace(openId))
            {
                dataerr = "openid为空";
                isdataok = false;
            }
            var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
            if (userId != 0)
            {
                if (userinfo == null)
                {
                    dataerr = "用户信息未取到" + userId;
                    isdataok = false;
                }
            }
            var _msgtmplinfo = GetWeiXinMsgTemplateByApplet(type);
            if (_msgtmplinfo == null)
            {
                dataerr = "消息模板未取到";
                isdataok = false;
            }
            string templateId = "";
            string topcolor = "#000000";
            if (isdataok)
            {
                templateId = _msgtmplinfo.TemplateId;
                if (string.IsNullOrWhiteSpace(templateId))
                {
                    dataerr = "消息模板未取到";
                    isdataok = false;
                }
            }
            if (!_msgtmplinfo.IsOpen)
            {
                dataerr = "未开启";
                isdataok = false;
            }

            if (isdataok)
            {
#if DEBUG
                Core.Log.Info("[模板消息]开始发送前");
#endif
                object msgdata;
                switch (type)
                {
                    case Core.Plugins.Message.MessageTypeEnum.OrderCreated:
                        #region 创建订单(买家)
                        var _ocmsgdata = new WX_MsgTemplateKey5DataModel();
                        _ocmsgdata.keyword1.value = data.keyword1.value;
                        _ocmsgdata.keyword1.color = data.keyword1.color;
                        _ocmsgdata.keyword2.value = data.keyword2.value;
                        _ocmsgdata.keyword2.color = data.keyword2.color;
                        _ocmsgdata.keyword3.value = data.keyword3.value;
                        _ocmsgdata.keyword3.color = data.keyword3.color;
                        _ocmsgdata.keyword4.value = data.keyword4.value;
                        _ocmsgdata.keyword4.color = data.keyword4.color;
                        _ocmsgdata.keyword5.value = data.keyword5.value;
                        _ocmsgdata.keyword5.color = data.keyword5.color;
                        msgdata = _ocmsgdata;
                        break;
                        #endregion
                    case Core.Plugins.Message.MessageTypeEnum.OrderPay:
                        #region 订单支付(买家)
                        var _opmsgdata = new WX_MsgTemplateKey4DataModel();
                        _opmsgdata.keyword1.value = data.keyword1.value;
                        _opmsgdata.keyword1.color = data.keyword1.color;
                        _opmsgdata.keyword2.value = data.keyword2.value;
                        _opmsgdata.keyword2.color = data.keyword2.color;
                        _opmsgdata.keyword3.value = data.keyword3.value;
                        _opmsgdata.keyword3.color = data.keyword3.color;
                        _opmsgdata.keyword4.value = data.keyword4.value;
                        _opmsgdata.keyword4.color = data.keyword4.color;
                        msgdata = _opmsgdata;
                        break;
                        #endregion
                    case Core.Plugins.Message.MessageTypeEnum.OrderShipping:                        
                        #region 订单发货(买家)
                        var _osmsgdata = new WX_MsgTemplateKey5DataModel();
                        _osmsgdata.keyword1.value = data.keyword1.value;
                        _osmsgdata.keyword1.color = data.keyword1.color;
                        _osmsgdata.keyword2.value = data.keyword2.value;
                        _osmsgdata.keyword2.color = data.keyword2.color;
                        _osmsgdata.keyword3.value = data.keyword3.value;
                        _osmsgdata.keyword3.color = data.keyword3.color;
                        _osmsgdata.keyword4.value = data.keyword4.value;
                        _osmsgdata.keyword4.color = data.keyword4.color;
                        _osmsgdata.keyword5.value = data.keyword5.value;
                        _osmsgdata.keyword5.color = data.keyword5.color;
                        msgdata = _osmsgdata;
                        break;
                        #endregion
                    case Core.Plugins.Message.MessageTypeEnum.OrderRefund:
                        #region 退款退货(买家)
                        var _ormsgdata = new WX_MsgTemplateKey5DataModel();
                        _ormsgdata.keyword1.value = data.keyword1.value;
                        _ormsgdata.keyword1.color = data.keyword1.color;
                        _ormsgdata.keyword2.value = data.keyword2.value;
                        _ormsgdata.keyword2.color = data.keyword2.color;
                        _ormsgdata.keyword3.value = data.keyword3.value;
                        _ormsgdata.keyword3.color = data.keyword3.color;
                        _ormsgdata.keyword4.value = data.keyword4.value;
                        _ormsgdata.keyword4.color = data.keyword4.color;
                        _ormsgdata.keyword5.value = data.keyword5.value;
                        _ormsgdata.keyword5.color = data.keyword5.color;
                        msgdata = _ormsgdata;
                        break;
                        #endregion
                    case Core.Plugins.Message.MessageTypeEnum.RefundDeliver:
                        #region 退款退货(买家)
                        var _rdmsgdata = new WX_MsgTemplateKey5DataModel();
                        _rdmsgdata.keyword1.value = data.keyword1.value;
                        _rdmsgdata.keyword1.color = data.keyword1.color;
                        _rdmsgdata.keyword2.value = data.keyword2.value;
                        _rdmsgdata.keyword2.color = data.keyword2.color;
                        _rdmsgdata.keyword3.value = data.keyword3.value;
                        _rdmsgdata.keyword3.color = data.keyword3.color;
                        _rdmsgdata.keyword4.value = data.keyword4.value;
                        _rdmsgdata.keyword4.color = data.keyword4.color;
                        _rdmsgdata.keyword5.value = data.keyword5.value;
                        _rdmsgdata.keyword5.color = data.keyword5.color;
                        msgdata = _rdmsgdata;
                        break;
                        #endregion
                    default:
                        throw new HimallException("无此模板消息，不能完成消息推送");
                        break;
                }
                #if DEBUG
                Core.Log.Info("[模板消息]开始发送到openid:" + openId);
                #endif
                var wxhelper = new Weixin.WXHelper();
                var accessToken = wxhelper.GetAccessToken(appid, appsecret);
                #if DEBUG
                Core.Log.Info("[模板消息]取到Token");
                #endif
                Core.Log.Info("[模版消息]发送开始:" + " openId<" + openId + "> Url=<" + url + "> formId<" + formId+">");
                var _result = Senparc.Weixin.WxOpen.AdvancedAPIs.Template.TemplateApi.SendTemplateMessage(accessToken, openId, templateId, msgdata, formId, url);
                if (_result.errcode != Senparc.Weixin.ReturnCode.请求成功)
                {
                    Core.Log.Info(_result.errcode.ToString() + ":" + _result.errmsg);
                }
                #if DEBUG
                Core.Log.Info("[模板消息]发送结束");
                #endif
            }
            else
            {
                #if DEBUG
                Core.Log.Info("[模板消息]发送失败：数据验证未通过-" + dataerr + "[" + type.ToDescription() + "]");
                #endif
            }
        }


        /// <summary>
        /// 新增小程序表单提交数据
        /// </summary>
        /// <param name="mWXSmallChoiceProductsInfo"></param>
        public void AddWXAppletFromData(WXAppletFormDatasInfo mWxAppletFromDateInfo)
        {
            Context.WXAppletFormDatasInfo.Add(mWxAppletFromDateInfo);
            Context.SaveChanges();
        }
        /// <summary>
        /// 获取表单保存数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="OrderId"></param>
        /// <returns></returns>
        public WXAppletFormDatasInfo GetWXAppletFromDataById(MessageTypeEnum type, string OrderId)
        {
            var model = Context.WXAppletFormDatasInfo.Where(d => d.EventId == (long)type && d.EventValue == OrderId).FirstOrDefault();
            return model;
        }
    }
}
