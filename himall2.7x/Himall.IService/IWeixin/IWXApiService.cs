using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.CommonModel.Model;

namespace Himall.IServices
{
    public interface IWXApiService : IService
    {
        /// <summary>
        /// 添加订阅（关注）记录
        /// </summary>
        void Subscribe(string openId);


        /// <summary>
        /// 取消订阅（关注）记录
        /// </summary>
        void UnSubscribe(string openId);


        /// <summary>
        /// 获取微信ticket
        /// </summary>
        string GetTicket(string appid, string appsecret);

		/// <summary>
		/// 获取AccessToken
		/// </summary>
		/// <param name="appId"></param>
		/// <param name="appSecret"></param>
		/// <param name="getNewToken">是否刷新缓存</param>
		/// <returns></returns>
		string TryGetToken(string appId, string appSecret, bool getNewToken = false);

        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="accessTokenOrAppId"></param>
        /// <param name="openId"></param>
        /// <param name="templateId"></param>
        /// <param name="topcolor"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        void SendMessageByTemplate(string appid, string appsecret, string openId, string templateId, string topcolor, string url, object data);

		/// <summary>
		/// 获取微信分享参数
		/// </summary>
		/// <param name="appid"></param>
		/// <param name="appsecret"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		WeiXinShareArgs GetWeiXinShareArgs(string appid, string appsecret, string url);
    }
}
