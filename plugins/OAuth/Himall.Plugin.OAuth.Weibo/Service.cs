using Himall.Core.Plugins;
using Himall.Core.Plugins.OAuth;
using Himall.Plugin.OAuth.Weibo.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.Plugin.OAuth.Weibo
{
    public class Service : IOAuthPlugin
    {
        static string ReturnUrl = string.Empty;

        public string ShortName
        {
            get { return "新浪微博"; }
        }

        public Core.Plugins.FormData GetFormData()
        {
            var config = WeiboCore.GetConfig();

            var formData = new Core.Plugins.FormData()
            {
                Items = new Core.Plugins.FormData.FormItem[] { 
                   //AppKey
                   new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "AppKey",
                     Name = "AppKey",
                     IsRequired = true,
                      Type= Core.Plugins.FormData.FormItemType.text,
                      Value=config.AppKey
                   },

                  //AppId
                   new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "AppSecret",
                     Name = "AppSecret",
                     IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.text,
                       Value=config.AppSecret
                   },
                   //验证内容
                   new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "验证内容",
                     Name = "ValidateContent",
                     IsRequired = true,
                      Type= Core.Plugins.FormData.FormItemType.text,
                      Value=config.ValidateContent
                   }
                }
            };
            return formData;
        }

        public void SetFormValues(IEnumerable<KeyValuePair<string, string>> values)
        {
            var appKeyItem = values.FirstOrDefault(item => item.Key == "AppKey");
            if (string.IsNullOrWhiteSpace(appKeyItem.Value))
                throw new Himall.Core.PluginConfigException("AppKey不能为空");

            var appidItem = values.FirstOrDefault(item => item.Key == "AppSecret");
            if (string.IsNullOrWhiteSpace(appidItem.Value))
                throw new Himall.Core.PluginConfigException("AppSecret不能为空");

            var validateContent = values.FirstOrDefault(item => item.Key == "ValidateContent");
            if (!string.IsNullOrWhiteSpace(validateContent.Value))//如果验证内容不为空，则该内容必须是<meta>节点
            {
                var lowerValidate = validateContent.Value.ToLower();
                if (!lowerValidate.StartsWith("<meta "))
                    throw new PluginException("验证内容必须以meta标签开头");
                if (!lowerValidate.EndsWith(" />"))
                    throw new PluginException("验证内容必须以 /> 结尾");
            }

            var oldConfig = WeiboCore.GetConfig();
            oldConfig.AppSecret = appidItem.Value;
            oldConfig.AppKey = appKeyItem.Value;
            oldConfig.ValidateContent = validateContent.Value;
            WeiboCore.SaveConfig(oldConfig);
        }

        public OAuthUserInfo GetUserInfo(System.Collections.Specialized.NameValueCollection queryString)
        {
            var config = WeiboCore.GetConfig();
            var oatuth = new NetDimension.Weibo.OAuth(config.AppKey, config.AppSecret, ReturnUrl);
            NetDimension.Weibo.Client client = new NetDimension.Weibo.Client(oatuth);
            var code = queryString["code"];
            var accessToken = oatuth.GetAccessTokenByAuthorizationCode(code);

            OAuthUserInfo userInfo = null;
            if (oatuth != null)
            {
                userInfo = new OAuthUserInfo();
                userInfo.OpenId = client.API.Entity.Account.GetUID();
                var user = client.API.Entity.Users.Show(userInfo.OpenId);
                userInfo.NickName = userInfo.RealName = user.Name;
                userInfo.IsMale = user.Gender == "m";
            }
            return userInfo;
        }

        public string GetOpenLoginUrl(string returnUrl)
        {
            ReturnUrl = returnUrl;
            var config = WeiboCore.GetConfig();
            string url = string.Format(config.AuthorizeURL + "?client_id={0}&response_type=code&redirect_uri={1}", config.AppKey, returnUrl);
            return url;
        }


        public string WorkDirectory
        {
            set { WeiboCore.WorkDirectory = value; }
        }

        public void CheckCanEnable()
        {
            return;
        }


        public string GetValidateContent()
        {
            var oldConfig = WeiboCore.GetConfig();
            return oldConfig.ValidateContent;
        }

        public string Icon_Default
        {
            get
            {
                if (string.IsNullOrWhiteSpace(WeiboCore.WorkDirectory))
                    throw new MissingFieldException("没有设置插件工作目录");
                return WeiboCore.WorkDirectory + "/weibo1.png";
            }
        }

        public string Icon_Hover
        {
            get
            {
                if (string.IsNullOrWhiteSpace(WeiboCore.WorkDirectory))
                    throw new MissingFieldException("没有设置插件工作目录");
                return WeiboCore.WorkDirectory + "/weibo2.png";
            }
        }
    }
}
