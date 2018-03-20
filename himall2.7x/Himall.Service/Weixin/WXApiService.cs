using Himall.Core;
using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Himall.Entity;
using Senparc.Weixin.Exceptions;
using Himall.CommonModel.Model;
using Senparc.Weixin.MP.Helpers;

namespace Himall.Service
{
	public class WXApiService : ServiceBase, IWXApiService
	{
		private static object _locker = new object();

		public void Subscribe(string openId)
		{
			Entities entity = new Entities();
			var model = entity.OpenIdsInfo.FirstOrDefault(p => p.OpenId == openId);
			if (model == null)
			{
				model = new Model.OpenIdsInfo();
				model.OpenId = openId;
				model.SubscribeTime = DateTime.Now;
				model.IsSubscribe = true;
				entity.OpenIdsInfo.Add(model);
				entity.Configuration.ValidateOnSaveEnabled = false;
				entity.SaveChanges();
				entity.Configuration.ValidateOnSaveEnabled = true;
			}
			else
			{
				if (!model.IsSubscribe)
				{
					model.IsSubscribe = true;
					entity.Configuration.ValidateOnSaveEnabled = false;
					entity.SaveChanges();
					entity.Configuration.ValidateOnSaveEnabled = true;
				}
			}

		}

		public void UnSubscribe(string openId)
		{
			Entities entity = new Entities();
			var model = entity.OpenIdsInfo.FirstOrDefault(p => p.OpenId == openId);
			if (model != null)
			{
				model.IsSubscribe = false;
				entity.SaveChanges();
			}
			else
			{
				model = new Model.OpenIdsInfo();
				model.OpenId = openId;
				model.SubscribeTime = DateTime.Now;
				model.IsSubscribe = false;
				entity.OpenIdsInfo.Add(model);
				entity.SaveChanges();
			}
		}

		public string GetTicket(string appid, string appsecret)
		{
			var context = Context;
			var model = context.WeiXinBasicInfo.AsNoTracking().FirstOrDefault();
			if (model != null && model.TicketOutTime > DateTime.Now && !string.IsNullOrEmpty(model.Ticket))
				return model.Ticket;

			lock (_locker)
			{
				model = context.WeiXinBasicInfo.FirstOrDefault();
				if (model != null && model.TicketOutTime > DateTime.Now && !string.IsNullOrEmpty(model.Ticket))
					return model.Ticket;

				if (model == null)
				{
					model = new Model.WeiXinBasicInfo();
					context.WeiXinBasicInfo.Add(model);
				}

				var ticketRequest = new Senparc.Weixin.MP.Entities.JsApiTicketResult();
				ticketRequest.errcode = Senparc.Weixin.ReturnCode.系统繁忙此时请开发者稍候再试;
				try
				{
					var accessToken = this.TryGetToken(appid, appsecret);
					model.AccessToken = accessToken;
					ticketRequest = CommonApi.GetTicket(accessToken);
				}
				catch (Exception e)
				{
					Log.Error("请求Ticket出错，强制刷新acess_token", e);
					var accessToken = this.TryGetToken(appid, appsecret, true);
					model.AccessToken = accessToken;
					ticketRequest = CommonApi.GetTicket(accessToken);
				}
				if (ticketRequest.errcode == Senparc.Weixin.ReturnCode.请求成功 && !string.IsNullOrEmpty(ticketRequest.ticket))
				{
					if (ticketRequest.expires_in > 3600)
					{
						ticketRequest.expires_in = 3600;
					}
					model.AppId = appid;
					model.TicketOutTime = DateTime.Now.AddSeconds(ticketRequest.expires_in);
					model.Ticket = ticketRequest.ticket;

					context.SaveChanges();

					return model.Ticket;
				}
				else
				{
					throw new HimallException("请求微信接口出错");
				}
			}
		}

		/// <summary>
		/// 获取AccessToken
		/// </summary>
		/// <param name="appId"></param>
		/// <param name="appSecret"></param>
		/// <param name="getNewToken">是否刷新缓存</param>
		/// <returns></returns>
		public string TryGetToken(string appId, string appSecret, bool getNewToken = false)
		{
			//AccessTokenContainer的TryGetToken有问题，所以自己实现一遍
			if (!AccessTokenContainer.CheckRegistered(appId))
				lock (_locker)
					if (!AccessTokenContainer.CheckRegistered(appId))
						AccessTokenContainer.Register(appId, appSecret);

			return AccessTokenContainer.GetToken(appId, getNewToken);
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
		/// <returns></returns>
		public void SendMessageByTemplate(string appid, string appsecret, string openId, string templateId, string topcolor, string url, object data)
		{
			if (!string.IsNullOrWhiteSpace(templateId))
			{
				var accessToken = this.TryGetToken(appid, appsecret);
				TemplateApi.SendTemplateMessage(accessToken, openId, templateId, topcolor, url, data);
			}
		}

		/// <summary>
		/// 获取微信分享参数
		/// </summary>
		/// <param name="appid"></param>
		/// <param name="appsecret"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public WeiXinShareArgs GetWeiXinShareArgs(string appid, string appsecret, string url)
		{
			string ticket = GetTicket(appid, appsecret);

			var jssdk = new JSSDKHelper();
			string timestamp = JSSDKHelper.GetTimestamp();
			string nonceStr = JSSDKHelper.GetNoncestr();
			string signature = jssdk.GetSignature(ticket, nonceStr, timestamp, url);           
			return new WeiXinShareArgs(appid, timestamp, nonceStr, signature, ticket);
		}
	}
}
