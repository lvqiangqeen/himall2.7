namespace Hishop.Weixin.MP.Util
{
    using System;
    using System.Web.Security;

    public class CheckSignature
    {
        public static readonly string Token = "weixin_test";

        public static bool Check(string signature, string timestamp, string nonce, string token)
        {
            token = token ?? Token;
            string[] array = new string[] { timestamp, nonce, token };
            Array.Sort<string>(array);
            string str = FormsAuthentication.HashPasswordForStoringInConfigFile(string.Join("", array), "SHA1");
            return (signature == str.ToLower());
        }
    }
}

