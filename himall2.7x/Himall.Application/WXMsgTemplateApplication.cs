using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.CommonModel;
using AutoMapper;
using Himall.DTO;

namespace Himall.Application
{
    public class WXMsgTemplateApplication
    {

        private static IWXMsgTemplateService _iWXMsgTemplateService = ObjectContainer.Current.Resolve<IWXMsgTemplateService>();
        /// <summary>
        /// 新增图文
        /// </summary>
        /// <param name="info"></param>
        /// 
        public static WXUploadNewsResult Add(IEnumerable<WXMaterialInfo> info, string appid, string appsecret)
        {
            return _iWXMsgTemplateService.Add(info, appid, appsecret);
        }
        /// <summary>
        /// 更新单条图文消息
        /// </summary>
        /// <param name="mediaid"></param>
        /// <param name="news"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        public static IEnumerable<WxJsonResult> UpdateMedia(string mediaid, IEnumerable<WXMaterialInfo> news, string appid, string appsecret)
        {
            return _iWXMsgTemplateService.UpdateMedia(mediaid, news, appid, appsecret);
        }
        /// <summary>
        /// 删除素材
        /// </summary>
        /// <param name="mediaid"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        public static WxJsonResult DeleteMedia(string mediaid, string appid, string appsecret)
        {
            return _iWXMsgTemplateService.DeleteMedia(mediaid, appid, appsecret);
        }
        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns>media_id</returns>
        public static string AddImage(string filename, string appid, string appsecret)
        {
            return _iWXMsgTemplateService.AddImage(filename, appid, appsecret);
        }
        /// <summary>
        /// 获取图文素材列表
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static MediaNewsItemList GetMediaMsgTemplateList(string appid, string appsecret, int offset, int count)
        {
            return _iWXMsgTemplateService.GetMediaMsgTemplateList(appid, appsecret, offset, count);
        }
        /// <summary>
        /// 取素材总数
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        public static MediaItemCount GetMediaItemCount(string appid, string appsecret)
        {
            return _iWXMsgTemplateService.GetMediaItemCount(appid, appsecret);
        }
        /// <summary>
        /// 群发送消息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static SendInfoResult SendWXMsg(SendMsgInfo info)
        {
            return _iWXMsgTemplateService.SendWXMsg(info);
        }
        /// <summary>
        /// 取图文素材
        /// </summary>
        /// <param name="mediaid"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        public static IEnumerable<WXMaterialInfo> GetMedia(string mediaid, string appid, string appsecret)
        {
           return  _iWXMsgTemplateService.GetMedia(mediaid, appid, appsecret);
        }
        /// <summary>
        /// 取非图文素材
        /// </summary>
        /// <param name="mediaid"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <param name="stream"></param>
        public static void GetMedia(string mediaid, string appid, string appsecret, Stream stream)
        {
             _iWXMsgTemplateService.GetMedia(mediaid, appid, appsecret, stream);
        }
        /// <summary>
        /// 添加发送记录
        /// </summary>
        /// <param name="info"></param>
        public static void AddSendRecord(SendMessageRecordInfo info)
        {
             _iWXMsgTemplateService.AddSendRecord(info);
        }
        public static SendMessageRecordInfo AddSendRecordItem(SendMessageRecordInfo info)
        {
            return _iWXMsgTemplateService.AddSendRecord(info);
        }
        /// <summary>
        /// 取发送记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<SendMessageRecord> GetSendRecords(IServices.QueryModel.SendRecordQuery query)
        {
            var data = _iWXMsgTemplateService.GetSendRecords(query);
            QueryPageModel<SendMessageRecord> item = new QueryPageModel<SendMessageRecord>();
            item.Total = data.Total;
            var list = data.Models.ToList();
            var dataList = Mapper.Map<List<SendMessageRecord>>(list);
            foreach (var info in dataList)
            {
                var record = _iWXMsgTemplateService.GetSendrecordCouponSnById(info.Id);
                info.CurrentCouponCount = record.Count;
                foreach (var items in record)
                {
                    var result = _iWXMsgTemplateService.GetCouponRecordBySn(items.CouponSN);
                    var orderResult = result.OrderId == null ? null : OrderApplication.GetOrder(result.OrderId.Value);
                    if (result != null && orderResult != null)
                        info.CurrentUseCouponCount++;
                }
            }
            item.Models = dataList;
            return item;
        }
        public static List<SendmessagerecordCouponSNInfo> GetSendrecordCouponSnById(long id)
        {
            return _iWXMsgTemplateService.GetSendrecordCouponSnById(id);
        }
        /// <summary>
        /// 指定openIds发送微信消息
        /// </summary>
        /// <param name="openIds">发送openId集合</param>
        /// <param name="msgType">类型</param>
        /// <param name="content">文本内容</param>
        /// <param name="mediaId">模板ID</param>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <returns></returns>
        public static SendInfoResult SendWXMsg(IEnumerable<string> openIds, WXMsgType msgType, string content, string mediaId, string appId, string appSecret)
        {
            return _iWXMsgTemplateService.SendWXMsg(openIds, msgType, content, mediaId, appId, appSecret);
        }



        #region 模板消息
        /// <summary>
        /// 获取微信模板消息列表
        /// </summary>
        /// <returns></returns>
        public static List<WeiXinMsgTemplateInfo> GetWeiXinMsgTemplateList()
        {
           return  _iWXMsgTemplateService.GetWeiXinMsgTemplateList();
        }
        /// <summary>
        /// 获取微信模板信息
        /// </summary>
        /// <returns></returns>
        public static WeiXinMsgTemplateInfo GetWeiXinMsgTemplate(Himall.Core.Plugins.Message.MessageTypeEnum type)
        {
          return   _iWXMsgTemplateService.GetWeiXinMsgTemplate(type);
        }
        /// <summary>
        /// 设置微信模板消息配置
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static WeiXinMsgTemplateInfo UpdateWeiXinMsgTemplate(WeiXinMsgTemplateInfo info)
        {
           return  _iWXMsgTemplateService.UpdateWeiXinMsgTemplate(info);
        }
        /// <summary>
        /// 设置微信消息开启状态
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isOpen"></param>
        public static void UpdateWeiXinMsgOpenState(Himall.Core.Plugins.Message.MessageTypeEnum type, bool isOpen)
        {
            _iWXMsgTemplateService.UpdateWeiXinMsgOpenState(type, isOpen);
        }
        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId">为0时使用openid</param>
        /// <param name="data">信息数据</param>
        /// <param name="url"></param>
        /// <param name="openid">与userid配合使用，userid为0时使用此字段</param>
        public static void SendMessageByTemplate(Himall.Core.Plugins.Message.MessageTypeEnum type, long userId, WX_MsgTemplateSendDataModel data, string url = "", string wxopenid = "")
        {
            _iWXMsgTemplateService.SendMessageByTemplate(type, userId, data, url,wxopenid);
        }
        /// <summary>
        /// 获取模板消息跳转URL
        /// </summary>
        /// <param name="type"></param>
        public static string GetMessageTemplateShowUrl(Himall.Core.Plugins.Message.MessageTypeEnum type)
        {
           return  _iWXMsgTemplateService.GetMessageTemplateShowUrl(type);
        }
        /// <summary>
        /// 添加消息模板
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <param name="type">null表示所有都重置</param>
        public static void AddMessageTemplate(Himall.Core.Plugins.Message.MessageTypeEnum? type = null)
        {
            _iWXMsgTemplateService.AddMessageTemplate(type);
        }
        #endregion
    }
}
