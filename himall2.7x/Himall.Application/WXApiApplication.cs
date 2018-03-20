using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.CommonModel.Model;
using Himall.IServices;

namespace Himall.Application
{
	public class WXApiApplication
	{
		#region 字段
		private static IWXApiService _wxApiService;
		#endregion

		#region 构造函数
		static WXApiApplication()
		{
			_wxApiService = Core.ObjectContainer.Current.Resolve<IWXApiService>();
		}
		#endregion

		#region 方法
		/// <summary>
		/// 添加订阅（关注）记录
		/// </summary>
		public static void Subscribe(string openId)
		{
			_wxApiService.Subscribe(openId);
		}

		/// <summary>
		/// 取消订阅（关注）记录
		/// </summary>
		public static void UnSubscribe(string openId)
		{
			_wxApiService.UnSubscribe(openId);
		}

		/// <summary>
		/// 获取微信ticket
		/// </summary>
		public static string GetTicket(string appid, string appsecret)
		{
			return _wxApiService.GetTicket(appid, appsecret);
		}
		
		/// <summary>
		/// 获取AccessToken
		/// </summary>
		/// <param name="appId"></param>
		/// <param name="appSecret"></param>
		/// <param name="getNewToken">是否刷新缓存</param>
		/// <returns></returns>
		public static string TryGetToken(string appId, string appSecret, bool getNewToken = false)
		{
			return _wxApiService.TryGetToken(appId, appSecret, getNewToken);
		}

		/// <summary>
		/// 获取微信分享参数
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static WeiXinShareArgs GetWeiXinShareArgs(string url)
		{
			var siteSettings = SiteSettingApplication.GetSiteSettings();
			if (string.IsNullOrWhiteSpace(siteSettings.WeixinAppId) || string.IsNullOrWhiteSpace(siteSettings.WeixinAppSecret))
				return null;
			return _wxApiService.GetWeiXinShareArgs(siteSettings.WeixinAppId, siteSettings.WeixinAppSecret, url);
		}

		/// <summary>
		/// 发送模板消息
		/// </summary>
		/// <param name="accessTokenOrAppId"></param>
		/// <param name="openId"></param>
		/// <param name="templateId"></param>
		/// <param name="topcolor"></param>
		/// <param name="url"></param>
		/// <param name="data"></param>
		public static void SendMessageByTemplate(string appid, string appsecret, string openId, string templateId, string topcolor, string url, object data)
		{
			_wxApiService.SendMessageByTemplate(appid, appsecret, openId, templateId, topcolor, url, data);
		}
		#endregion
	}
}
