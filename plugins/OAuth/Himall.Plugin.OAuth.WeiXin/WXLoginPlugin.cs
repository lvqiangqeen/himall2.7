using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Himall.Core.Plugins.OAuth;
using Himall.Core.Plugins;
using Himall.Plugin.OAuth.WeiXin.Model;
using Himall.Plugin.OAuth.WeiXin.Assistant;

namespace Himall.Plugin.OAuth.WeiXin
{
    public class WXLoginPlugin:IOAuthPlugin
    {
        public static string WXWorkDirectory = string.Empty;
        static string ReturnUrl = string.Empty;

        public FormData GetFormData()
        {
            OAuthWXConfigInfo config = ConfigService<OAuthWXConfigInfo>.GetConfig(WXWorkDirectory + "\\Config\\OAuthWXConfig.config");
            return new FormData()
              {
                  Items = new Core.Plugins.FormData.FormItem[] { 
                   //AppId
                   new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "AppId",
                     Name = "AppId",
                     IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.text,
                       Value=config.AppId
                   },

                   //AppKey
                   new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "AppSecret",
                     Name = "AppSecret",
                     IsRequired = true,
                      Type= Core.Plugins.FormData.FormItemType.text,
                      Value=config.AppSecret
                   }
                }
              };
            
        }

        public string GetOpenLoginUrl(string returnUrl)
        {
            ReturnUrl = returnUrl;
            OAuthWXConfigInfo config = ConfigService<OAuthWXConfigInfo>.GetConfig(WXWorkDirectory + "\\Config\\OAuthWXConfig.config");
            OAuthRule rule = ConfigService<OAuthRule>.GetConfig(WXWorkDirectory + "\\Config\\OAuthUrl.config");

            if (string.IsNullOrEmpty(returnUrl))
                throw new PluginException("未传入回调地址！");
            if (string.IsNullOrEmpty(config.AppId))
                throw new PluginException("未设置AppId！");
            if (string.IsNullOrEmpty(config.AppSecret))
                throw new PluginException("未设置AppSecret！");
            if (string.IsNullOrEmpty(rule.GetCodeUrl))
                throw new PluginException("未设置微信接口地址！");

            string strGetCodeUrl = string.Format(rule.GetCodeUrl, config.AppId, ReturnUrl);

            return strGetCodeUrl;
        }

        public OAuthUserInfo GetUserInfo(NameValueCollection queryString)
        {
            OAuthUserInfo oAuthUser = new OAuthUserInfo();
            string strCode = string.Empty;
            string strState = string.Empty;

            OAuthWXConfigInfo config = ConfigService<OAuthWXConfigInfo>.GetConfig(WXWorkDirectory + "\\Config\\OAuthWXConfig.config");
            if (queryString["code"] != null && queryString["state"] != null)
            {
                strCode = queryString["code"];
                strState = queryString["state"];
                if (string.IsNullOrEmpty(config.AppSecret))
                    throw new System.MissingFieldException("未设置AppSecret！");

                UserInfo userinfo = WeiXinApi.GetUserInfo(strCode, config.AppId, config.AppSecret);
                oAuthUser.OpenId = userinfo.openid;
                oAuthUser.NickName = userinfo.nickname;
                oAuthUser.UnionId = userinfo.unionid;
                oAuthUser.IsMale = userinfo.sex == 0 ? false : true;
            }
            return oAuthUser;
        }

        public string GetValidateContent()
        {
            return string.Empty;
        }


        public void SetFormValues(IEnumerable<KeyValuePair<string, string>> values)
        {
            KeyValuePair<string,string> appid = values.FirstOrDefault(item => item.Key == "AppId");
            if (string.IsNullOrWhiteSpace(appid.Value))
            {
                throw new PluginException("Appid不能为空");
            }
            KeyValuePair<string, string> appsecret = values.FirstOrDefault(item => item.Key == "AppSecret");
            if (string.IsNullOrWhiteSpace(appsecret.Value))
            {
                throw new PluginException("AppSecret不能为空");
            }

            OAuthWXConfigInfo config = new OAuthWXConfigInfo();
            config.AppId = appid.Value;
            config.AppSecret = appsecret.Value;
            ConfigService<OAuthWXConfigInfo>.SaveConfig(config, WXWorkDirectory + "\\Config\\OAuthWXConfig.config");
            //ConfigService<OAuthWXConfigInfo>.SaveConfig()
        }

        public string ShortName
        {
            get { return "微信"; }
        }

        public void CheckCanEnable()
        {
            OAuthWXConfigInfo config = ConfigService<OAuthWXConfigInfo>.GetConfig(WXWorkDirectory + "\\Config\\OAuthWXConfig.config");
            OAuthRule rule = ConfigService<OAuthRule>.GetConfig(WXWorkDirectory + "\\Config\\OAuthUrl.config");

            if (string.IsNullOrEmpty(config.AppId))
                throw new PluginException("未设置AppId！");
            if (string.IsNullOrEmpty(config.AppSecret))
                throw new PluginException("未设置AppSecret！");
            if (string.IsNullOrEmpty(rule.GetCodeUrl))
                throw new PluginException("未设置微信接口地址！");
        }

        public string WorkDirectory
        {
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException("WorkDirectory不能为空");
                WXLoginPlugin.WXWorkDirectory = value;
            }
        }


        public string Icon_Default
        {
            get
            {
                if (string.IsNullOrWhiteSpace(WXWorkDirectory))
                {
                    throw new System.MissingFieldException("未设置工作目录！");
                }
                return WXWorkDirectory + "/Resource/weixin1.png";
            }
        }

        public string Icon_Hover
        {
            get
            {
                if (string.IsNullOrWhiteSpace(WXWorkDirectory))
                {
                    throw new System.MissingFieldException("未设置工作目录！");
                }
                return WXWorkDirectory + "/Resource/weixin2.png";
            }
        }
    }
}
