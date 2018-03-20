using Himall.Core.Plugins;
using Himall.Core.Plugins.OAuth;
using QConnectSDK;
using QConnectSDK.Config;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Himall.Plugin.OAuth.QQ
{
    public class Service:IOAuthPlugin
    {
        static string ReturnUrl = string.Empty;
       
        public string GetOpenLoginUrl(string returnUrl)
        {
            ReturnUrl = returnUrl;
            OAuthQQConfig qqconfig = QQCore.GetConfig();
            
            if (string.IsNullOrWhiteSpace(qqconfig.AppId))
                throw new MissingFieldException("未配置AppId");
            if (string.IsNullOrWhiteSpace(qqconfig.AppKey))
                throw new MissingFieldException("未配置AppKey");

            string state = "test";
            string scope = "get_user_info";
            string url = string.Format(qqconfig.AuthorizeURL + "?response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state={3}", qqconfig.AppId, returnUrl, scope, state);
            return url;
        }

        public string WorkDirectory
        {
            set { QQCore.WorkDirectory = value; }
        }


        public OAuthUserInfo GetUserInfo(NameValueCollection queryString)
        {
            QOpenClient qzone = null;
            var verifier = queryString["code"];
            var state = queryString["state"];

            OAuthQQConfig qqconfig = QQCore.GetConfig();
            string url = string.Format(qqconfig.AuthorizeURL + "?grant_type=authorization_code&client_id={0}&state={2}&client_secret={3}&code={4}&redirect_uri={1}", qqconfig.AppId, ReturnUrl, state, qqconfig.AppKey, verifier);

            QQConnectConfig.SetCallBackUrl(ReturnUrl);
            qzone = new QOpenClient(qqconfig.AuthorizeURL, qqconfig.AppId, qqconfig.AppKey, verifier, state);
            OAuthUserInfo userInfo = null;
            if (qzone != null)
            {
                userInfo = new OAuthUserInfo();
                var currentUser = qzone.GetCurrentUser();
                userInfo.NickName = currentUser.Nickname;
                userInfo.RealName = currentUser.Nickname;
                if (!string.IsNullOrWhiteSpace(currentUser.Gender) && (currentUser.Gender == "男" || currentUser.Gender == "女"))
                    userInfo.IsMale = currentUser.Gender == "男" ? true : false;
                userInfo.OpenId = qzone.OAuthToken.OpenId;
            }
            return userInfo;
        }


        public void CheckCanEnable()
        {
            OAuthQQConfig qqconfig = QQCore.GetConfig();
            if (string.IsNullOrWhiteSpace(qqconfig.AppId))
                throw new Himall.Core.PluginConfigException("未设置AppId");

            if (string.IsNullOrWhiteSpace(qqconfig.AppKey))
                throw new Himall.Core.PluginConfigException("未设置AppKey");

            if (string.IsNullOrWhiteSpace(qqconfig.AuthorizeURL))
                throw new Himall.Core.PluginConfigException("未设置授权地址(AuthorizeURL)");
        }

        public string ShortName
        {
            get
            {
                return "QQ";
            }
        }


        public Core.Plugins.FormData GetFormData()
        {

            var config = QQCore.GetConfig();

            var formData = new Core.Plugins.FormData()
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
                     DisplayName = "AppKey",
                     Name = "AppKey",
                     IsRequired = true,
                      Type= Core.Plugins.FormData.FormItemType.text,
                      Value=config.AppKey
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
            var appidItem = values.FirstOrDefault(item => item.Key == "AppId");
            if (string.IsNullOrWhiteSpace(appidItem.Value))
                throw new PluginException("AppId不能为空");

            var appKeyItem = values.FirstOrDefault(item => item.Key == "AppKey");
            if (string.IsNullOrWhiteSpace(appKeyItem.Value))
                throw new PluginException("AppKey不能为空");

            var validateContent =  values.FirstOrDefault(item => item.Key == "ValidateContent");
            if (!string.IsNullOrWhiteSpace(validateContent.Value))//如果验证内容不为空，则该内容必须是<meta>节点
            {
                var lowerValidate = validateContent.Value.ToLower();
                if (!lowerValidate.StartsWith("<meta "))
                    throw new PluginException("验证内容必须以meta标签开头"); 
                if (!lowerValidate.EndsWith(" />"))
                    throw new PluginException("验证内容必须以 /> 结尾");
            }

            OAuthQQConfig oldConfig = QQCore.GetConfig();
            oldConfig.AppId = appidItem.Value;
            oldConfig.AppKey = appKeyItem.Value;
            oldConfig.ValidateContent = validateContent.Value;
            QQCore.SaveConfig(oldConfig);
        }


        public string GetValidateContent()
        {
            OAuthQQConfig config = QQCore.GetConfig();
            return config.ValidateContent;
        }


        public string Icon_Default
        {
            get
            {
                if (string.IsNullOrWhiteSpace(QQCore.WorkDirectory))
                    throw new MissingFieldException("没有设置插件工作目录");
                return QQCore.WorkDirectory + "/qq1.png";
            }
        }

        public string Icon_Hover
        {
            get
            {
                if (string.IsNullOrWhiteSpace(QQCore.WorkDirectory))
                    throw new MissingFieldException("没有设置插件工作目录");
                return QQCore.WorkDirectory + "/qq2.png";
            }
        }
    }
}
