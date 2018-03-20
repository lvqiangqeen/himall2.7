namespace Hishop.Weixin.MP.Api
{
    using Hishop.Weixin.MP.Util;
    using System;
    using System.Configuration;
    using System.Web.Script.Serialization;

    public class TokenApi
    {
        public static string GetToken(string appid, string secret)
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", appid, secret);
            return new WebUtils().DoGet(url, null);
        }

        public static string GetToken_Message(string appid, string secret)
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", appid, secret);
            string input = new WebUtils().DoGet(url, null);
            if (input.Contains("access_token"))
            {
                input = new JavaScriptSerializer().Deserialize<Token>(input).access_token;
            }
            return input;
        }

        public string AppId
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("AppId");
            }
        }

        public string AppSecret
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("AppSecret");
            }
        }
    }
}

