using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;
using System.IO;
using Himall.CommonModel;
using Himall.Core.Plugins.Message;

namespace Himall.IServices
{
    public interface IWXMsgTemplateService : IService
    {
        /// <summary>
        /// 新增图文
        /// </summary>
        /// <param name="info"></param>
        /// 
        WXUploadNewsResult Add(IEnumerable<WXMaterialInfo> info, string appid, string appsecret);
        /// <summary>
        /// 更新单条图文消息
        /// </summary>
        /// <param name="mediaid"></param>
        /// <param name="news"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        IEnumerable<WxJsonResult> UpdateMedia(string mediaid, IEnumerable<WXMaterialInfo> news, string appid, string appsecret);
        /// <summary>
        /// 删除素材
        /// </summary>
        /// <param name="mediaid"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        WxJsonResult DeleteMedia(string mediaid, string appid, string appsecret);
        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns>media_id</returns>
        string AddImage(string filename, string appid, string appsecret);
        /// <summary>
        /// 获取图文素材列表
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        MediaNewsItemList GetMediaMsgTemplateList(string appid, string appsecret, int offset, int count);
        /// <summary>
        /// 取素材总数
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        MediaItemCount GetMediaItemCount(string appid, string appsecret);
        /// <summary>
        /// 群发送消息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        SendInfoResult SendWXMsg(SendMsgInfo info);



        /// <summary>
        /// 指定openIds发送微信消息
        /// </summary>
        /// <param name="openIds"></param>
        /// <param name="msgType"></param>
        /// <param name="content"></param>
        /// <param name="mediaId"></param>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <returns></returns>
        SendInfoResult SendWXMsg(IEnumerable<string> openIds, WXMsgType msgType, string content, string mediaId, string appId, string appSecret);

        /// <summary>
        /// 取图文素材
        /// </summary>
        /// <param name="mediaid"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        IEnumerable<WXMaterialInfo> GetMedia(string mediaid, string appid, string appsecret);
        /// <summary>
        /// 取非图文素材
        /// </summary>
        /// <param name="mediaid"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <param name="stream"></param>
        void GetMedia(string mediaid, string appid, string appsecret,Stream stream);
        /// <summary>
        /// 添加发送记录
        /// </summary>
        /// <param name="info"></param>
        SendMessageRecordInfo AddSendRecord(SendMessageRecordInfo info);
        /// <summary>
        /// 取发送记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<SendMessageRecordInfo> GetSendRecords(QueryModel.SendRecordQuery query);
        List<SendmessagerecordCouponSNInfo> GetSendrecordCouponSnById(long id);
        CouponRecordInfo GetCouponRecordBySn(string CouponSn);

        #region 模板消息
        /// <summary>
        /// 获取微信模板消息列表
        /// </summary>
        /// <returns></returns>
        List<WeiXinMsgTemplateInfo> GetWeiXinMsgTemplateList();

              /// <summary>
        /// 获取微信模板消息列表
        /// </summary>
        /// <returns></returns>
        List<WeiXinMsgTemplateInfo> GetWeiXinMsgTemplateListByApplet();  
        /// <summary>
        /// 获取微信模板信息
        /// </summary>
        /// <returns></returns>
        WeiXinMsgTemplateInfo GetWeiXinMsgTemplate(Himall.Core.Plugins.Message.MessageTypeEnum type);
        /// <summary>
        /// 设置微信模板消息配置
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        WeiXinMsgTemplateInfo UpdateWeiXinMsgTemplate(WeiXinMsgTemplateInfo info);
        /// <summary>
        /// 设置微信消息开启状态
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isOpen"></param>
        void UpdateWeiXinMsgOpenState(Himall.Core.Plugins.Message.MessageTypeEnum type,bool isOpen);
        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId">为0时使用openid</param>
        /// <param name="data">信息数据</param>
        /// <param name="url"></param>
        /// <param name="openid">与userid配合使用，userid为0时使用此字段</param>
        void SendMessageByTemplate(Himall.Core.Plugins.Message.MessageTypeEnum type, long userId, WX_MsgTemplateSendDataModel data, string url = "", string wxopenid = "");
        /// <summary>
        /// 获取模板消息跳转URL
        /// </summary>
        /// <param name="type"></param>
        string GetMessageTemplateShowUrl(Himall.Core.Plugins.Message.MessageTypeEnum type);
        /// <summary>
        /// 添加消息模板
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <param name="type">null表示所有都重置</param>
        void AddMessageTemplate(Himall.Core.Plugins.Message.MessageTypeEnum? type = null);
        #endregion
        void UpdateWXsmallMessage(IEnumerable<KeyValuePair<string, string>> items);

        /// <summary>
        /// 获取小程序模板消息跳转URL
        /// </summary>
        /// <param name="type"></param>
        string GetWXAppletMessageTemplateShowUrl(Himall.Core.Plugins.Message.MessageTypeEnum type);

        /// <summary>
        /// 获取微信模板信息
        /// </summary>
        /// <returns></returns>
        WeiXinMsgTemplateInfo GetWXAppletMsgTemplate(Himall.Core.Plugins.Message.MessageTypeEnum type);

        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId">为0时使用openid</param>
        /// <param name="data">信息数据</param>
        /// <param name="url"></param>
        /// <param name="openid">与userid配合使用，userid为0时使用此字段</param>
        void SendAppletMessageByTemplate(Himall.Core.Plugins.Message.MessageTypeEnum type, long userId, WX_MsgTemplateSendDataModel data, string url = "", string wxopenid = "", string formid = "");

        void AddWXAppletFromData(WXAppletFormDatasInfo wxapplet);
        WXAppletFormDatasInfo GetWXAppletFromDataById(MessageTypeEnum type, string OrderId);
    }
}
