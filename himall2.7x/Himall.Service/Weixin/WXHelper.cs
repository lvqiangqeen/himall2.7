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

namespace Himall.Service.Weixin
{
    public class WXHelper : ServiceBase
    {
        private IWXApiService ser_wxapi;
        public WXHelper()
        {
            ser_wxapi = ServiceProvider.Instance<IWXApiService>.Create;
        }
        /// <summary>
        /// 获取访问令牌
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public string GetAccessToken(string appid, string secret, bool IsGetNew = false)
        {
            string result = "";
            if (!string.IsNullOrWhiteSpace(appid) && !string.IsNullOrWhiteSpace(secret))
            {
                try
                {
                    if (IsGetNew)
                    {
                        result = AccessTokenContainer.TryGetToken(appid, secret, true);
                    }
                    else
                    {
                        result = AccessTokenContainer.TryGetToken(appid, secret);
                    }
                }catch(Exception ex)
                {
                    Log.Error("[WXACT]appId=" + appid + ";appSecret=" + secret + ";" + ex.Message);
                }
            }else
            {
                throw new HimallException("未配置微信公众信息");
            }
            return result;
        }
        /// <summary>
        /// 获取微信票据
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="secret"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetTicket(string appid, string secret, string type = "jsapi")
        {
            string result = "";
            if (type == "jsapi")
            {
                result = ser_wxapi.GetTicket(appid, secret);
            }
            else
            {
                try
                {
                    result = GetTicketByToken(GetAccessToken(appid, secret), type);
                }
                catch (Exception e)
                {
                    Log.Info("请求Ticket出错，强制刷新acess_token", e);
                    result = GetTicketByToken(GetAccessToken(appid, secret, true), type);
                }
            }
            return result;
        }
        /// <summary>
        /// 通过令牌获取票据
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="type"></param>
        /// <param name="IsRemote">是否强制从远程获取</param>
        /// <returns></returns>
        public string GetTicketByToken(string accessToken, string type = "jsapi", bool IsRemote = false)
        {
            string result = "";
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var wxresult = CommonApi.GetTicket(accessToken, type);
                if (wxresult.errcode == Senparc.Weixin.ReturnCode.请求成功)
                {
                    result = wxresult.ticket;
                }
                else
                {
                    throw new Exception("WXERR:[" + wxresult.errcode + "]" + wxresult.errmsg);
                }
            }
            return result;
        }
        /// <summary>
        /// 清理对应ticket缓存
        /// <para>使用微信功能失效时，调用此方法用以要求重新获取票据</para>
        /// </summary>
        /// <param name="ticket"></param>
        public void ClearTicketCache(string ticket)
        {

        }
    }
}
