using Himall.Plugin.OAuth.WeiXin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Plugin.OAuth.WeiXin.Assistant
{
    public class WeiXinApi
    {
        public static UserInfo GetUserInfo(string code,string appid,string appsecret)
        {
            UserInfo userinfo= null;
            OAuthRule rule = ConfigService<OAuthRule>.GetConfig(WXLoginPlugin.WXWorkDirectory + "\\Config\\OAuthUrl.config");
            if (string.IsNullOrEmpty(rule.GetTokenUrl))
                throw new System.MissingFieldException("未设置微信接口地址:GetTokenUrl");
            if (string.IsNullOrEmpty(rule.GetUserInfoUrl))
                throw new System.MissingFieldException("未设置微信接口地址:GetUserInfoUrl");
            string url = string.Format(rule.GetTokenUrl, appid, appsecret, code);
            HttpHandler.ClientRequest clientRequest = new HttpHandler.ClientRequest(url);
            clientRequest.HttpMethod = "get";
            ErrResult err = new ErrResult();
            TokenResult tokenResult = HttpHandler.GetResponseResult<TokenResult, ErrResult>(clientRequest, err);
            if (err.errcode > 0)
            {
                throw new Himall.Core.Plugins.PluginException("微信登录接口GetToken出错: " + err.errmsg);
            }
            if (string.IsNullOrEmpty(tokenResult.access_token))
            {
                throw new Himall.Core.Plugins.PluginException("微信登录接口返回access_Token为空");
            }
            
            url = string.Format(rule.GetUserInfoUrl, tokenResult.access_token, tokenResult.openid);
            HttpHandler.ClientRequest GetUserRequest = new HttpHandler.ClientRequest(url);
            GetUserRequest.HttpMethod = "get";
            userinfo = HttpHandler.GetResponseResult<UserInfo, ErrResult>(GetUserRequest, err);
            if (err.errcode>0)
            {
                throw new Himall.Core.Plugins.PluginException("微信登录接口GetUserInfo出错: " + err.errmsg);
            }
            //if (tokenResult.access_token)
            return userinfo;
        }
    }
}
