namespace Hishop.Weixin.MP.Api
{
    using Hishop.Weixin.MP.Util;
    using System;

    public class MenuApi
    {
        public static string CreateMenus(string accessToken, string json)
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/menu/create?access_token={0}", accessToken);
            return new WebUtils().DoPost(url, json);
        }

        public static string DeleteMenus(string accessToken)
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/menu/delete?access_token={0}", accessToken);
            return new WebUtils().DoGet(url, null);
        }

        public static string GetMenus(string accessToken)
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/menu/get?access_token={0}", accessToken);
            return new WebUtils().DoGet(url, null);
        }
    }
}

