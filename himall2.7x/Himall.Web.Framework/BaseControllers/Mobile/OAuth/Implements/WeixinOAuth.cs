using Himall.IServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace Himall.Web.Framework
{
    class WeixinOAuth : IMobileOAuth
    {
        public MobileOAuthUserInfo GetUserInfo(ActionExecutingContext filterContext, out string redirectUrl)
        {
            var settings = ServiceHelper.Create<ISiteSettingService>().GetSiteSettings();
            //var settings = ServiceHelper.Create<IVShopService>().GetVShopSetting(0);
            MobileOAuthUserInfo userInfo = null;
            redirectUrl = string.Empty;

            if (!string.IsNullOrEmpty(settings.WeixinAppId))
            {
                string code = filterContext.HttpContext.Request["code"];
                if (!string.IsNullOrEmpty(code)) // 如果用户同意授权
                {
                    string result = GetResponseResult(string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", settings.WeixinAppId, settings.WeixinAppSecret, code));
                    if (result.Contains("access_token"))
                    {
                        var resultObj = JsonConvert.DeserializeObject(result) as JObject;

                        string userStr = GetResponseResult("https://api.weixin.qq.com/sns/userinfo?access_token=" + resultObj["access_token"].ToString() + "&openid=" + resultObj["openid"].ToString() + "&lang=zh_CN");
                        if (userStr.Contains("nickname"))
                        {
                            var userObj = JsonConvert.DeserializeObject(userStr) as JObject;

                            userInfo = new MobileOAuthUserInfo()
                            {
                                NickName = userObj["nickname"].ToString(),
                                RealName = userObj["nickname"].ToString(),
                                OpenId = userObj[ "openid" ].ToString() ,
                                UnionId = (userObj["unionid"] == null || string.IsNullOrWhiteSpace(userObj["unionid"].ToString())) ? userObj["openid"].ToString() : userObj["unionid"].ToString(),
                                Headimgurl = userObj["headimgurl"].ToString(),
                                LoginProvider = "Himall.Plugin.OAuth.WeiXin",
                                Sex = userObj["sex"].ToString(),
                                City = userObj["city"].ToString(),
                                Province = userObj["province"].ToString(),
                                Country = userObj["country"].ToString()
                            };
                        }
                    }
                }
                else //还没有到用户授权页面
                {
                    string url = string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_userinfo&state=STATE#wechat_redirect"
                        , settings.WeixinAppId, System.Web.HttpUtility.UrlEncode(filterContext.HttpContext.Request.Url.ToString()));
                    redirectUrl = url;//指定跳转授权页面 
                }
            }
            return userInfo;
        }

        public MobileOAuthUserInfo GetUserInfo(ActionExecutingContext filterContext, out string redirectUrl, Model.WXShopInfo settings)
        {
            //var settings = ServiceHelper.Create<ISiteSettingService>().GetSiteSettings();
            //var settings = ServiceHelper.Create<IVShopService>().GetVShopSetting(0);
            MobileOAuthUserInfo userInfo = null;
            redirectUrl = string.Empty;

            if (!string.IsNullOrEmpty(settings.AppId))
            {
                string code = filterContext.HttpContext.Request["code"];
                if (!string.IsNullOrEmpty(code)) // 如果用户同意授权
                {
                    string result = GetResponseResult(string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", settings.AppId, settings.AppSecret, code));
                    if (result.Contains("access_token"))
                    {
                        var resultObj = JsonConvert.DeserializeObject(result) as JObject;

                        string userStr = GetResponseResult("https://api.weixin.qq.com/sns/userinfo?access_token=" + resultObj["access_token"].ToString() + "&openid=" + resultObj["openid"].ToString() + "&lang=zh_CN");
                        if (userStr.Contains("nickname"))
                        {
                            var userObj = JsonConvert.DeserializeObject(userStr) as JObject;

                            userInfo = new MobileOAuthUserInfo()
                            {
                                NickName = userObj["nickname"].ToString(),
                                RealName = userObj["nickname"].ToString(),
                                OpenId = userObj["openid"].ToString(),
                                UnionId = (userObj["unionid"] == null || string.IsNullOrWhiteSpace(userObj["unionid"].ToString())) ? userObj["openid"].ToString() : userObj["unionid"].ToString(),
                                Headimgurl = userObj["headimgurl"].ToString(),
                                LoginProvider = "Himall.Plugin.OAuth.WeiXin",
                                Sex = userObj["sex"].ToString(),
                                City = userObj["city"].ToString(),
                                Province = userObj["province"].ToString(),
                                Country = userObj["country"].ToString()
                            };
                        }
                    }
                }
                else //还没有到用户授权页面
                {
                    string url = string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_userinfo&state=STATE#wechat_redirect"
                        , settings.AppId, System.Web.HttpUtility.UrlEncode(filterContext.HttpContext.Request.Url.ToString()));
                    redirectUrl = url;//指定跳转授权页面 
                }
            }
            return userInfo;
        }
        /// <summary>
        /// 静默的方式获取openid
        /// </summary>
        /// <param name="filterContext"></param>
        /// <param name="redirectUrl"></param>
        /// <param name="settings">APPID、Secret</param>
        /// <returns></returns>
        public MobileOAuthUserInfo GetUserInfo_bequiet(ActionExecutingContext filterContext, out string redirectUrl, Model.WXShopInfo settings)
        {
            MobileOAuthUserInfo userInfo = null;
            redirectUrl = string.Empty;

            if (!string.IsNullOrEmpty(settings.AppId))
            {
                string code = filterContext.HttpContext.Request["code"];
                if (!string.IsNullOrEmpty(code)) // 如果用户同意授权
                {
                    string result = GetResponseResult(string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", settings.AppId, settings.AppSecret, code));
                    if (result.Contains("access_token"))
                    {
                        var resultObj = JsonConvert.DeserializeObject(result) as JObject;
                        string userStr = GetResponseResult("https://api.weixin.qq.com/sns/userinfo?access_token=" + resultObj["access_token"].ToString() + "&openid=" + resultObj["openid"].ToString() + "&lang=zh_CN");
                        if (userStr.Contains("nickname"))
                        {
                            var userObj = JsonConvert.DeserializeObject(userStr) as JObject;

                            userInfo = new MobileOAuthUserInfo()
                            {
                                NickName = userObj["nickname"].ToString(),
                                RealName = userObj["nickname"].ToString(),
                                OpenId = userObj["openid"].ToString(),
                                UnionId = (userObj["unionid"] == null || string.IsNullOrWhiteSpace(userObj["unionid"].ToString())) ? userObj["openid"].ToString() : userObj["unionid"].ToString(),
                                Headimgurl = userObj["headimgurl"].ToString(),
                                LoginProvider = "Himall.Plugin.OAuth.WeiXin",
                                Sex = userObj["sex"].ToString(),
                                City = userObj["city"].ToString(),
                                Province = userObj["province"].ToString(),
                                Country = userObj["country"].ToString()
                            };
                        }
                    }
                }
                else //还没有到用户授权页面
                {
                    string url = filterContext.HttpContext.Request.Url.ToString();
                    //scope=snsapi_base,静默方法
                    url = string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_base&state=STATE#wechat_redirect"
                                , settings.AppId, System.Web.HttpUtility.UrlEncode(url));
                    redirectUrl = url;//指定跳转授权页面 
                    //GetResponseResult(url);
                }
            }
            return userInfo;
        }
        
        string GetResponseResult(string url)
        {
            string result;
            WebRequest req = WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (Stream receiveStream = response.GetResponseStream())
                {

                    using (StreamReader readerOfStream = new StreamReader(receiveStream, System.Text.Encoding.UTF8))
                    {
                        result = readerOfStream.ReadToEnd();
                    }
                }
            }
            return result;
        }
    }
}
