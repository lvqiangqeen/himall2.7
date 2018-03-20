using Himall.IServices;
using System;

namespace Himall.Web.Framework
{
    public class UserCookieEncryptHelper
    {
		private static string _userCookieKey;
		private static object _locker = new object();

        /// <summary>
        /// 用户标识Cookie加密
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>返回加密后的Cookie值</returns>
        public static string Encrypt(long userId,string role)
        {
			if (_userCookieKey == null)
				lock (_locker)
					if (_userCookieKey == null)
						_userCookieKey = GetUserCookieKey();

            string text = string.Empty;
            try
            {
				string plainText = string.Format("{0},{1},{2}", _userCookieKey,role, userId);
				text = Core.Helper.SecureHelper.AESEncrypt(plainText, _userCookieKey);
                text = Core.Helper.SecureHelper.EncodeBase64(text);
                return text;
            }
            catch(Exception ex)
            {
                Core.Log.Error(string.Format("加密用户标识Cookie出错", text), ex);
                throw;
            }
        }

        /// <summary>
        /// 用户标识Cookie解密
        /// </summary>
        /// <param name="userIdCookie">用户IdCookie密文</param>
        /// <returns></returns>
		public static long Decrypt(string userIdCookie, string role)
		{
			if (_userCookieKey == null)
				lock (_locker)
					if (_userCookieKey == null)
						_userCookieKey = GetUserCookieKey();

            string plainText = string.Empty;
            long userId = 0;
            try
            {
                if (!string.IsNullOrWhiteSpace(userIdCookie))
                {
                    userIdCookie = System.Web.HttpUtility.UrlDecode(userIdCookie);
                    userIdCookie = Core.Helper.SecureHelper.DecodeBase64(userIdCookie);
                    plainText = Core.Helper.SecureHelper.AESDecrypt(userIdCookie, _userCookieKey);//解密
					var temp=plainText.Split(',');
					if (temp[0] == _userCookieKey && temp[1].Equals(role, StringComparison.OrdinalIgnoreCase) && long.TryParse(temp[2], out userId))
						return userId;
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error(string.Format("解密用户标识Cookie出错，Cookie密文：{0}", userIdCookie), ex);
            }

			return userId;
        }

		private static string GetUserCookieKey()
		{
			string key = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().UserCookieKey;
			if (string.IsNullOrEmpty(key))
			{
				key = Core.Helper.SecureHelper.MD5(Guid.NewGuid().ToString());
				ServiceProvider.Instance<ISiteSettingService>.Create.SaveSetting("UserCookieKey", key);
			}

			return key;
		}
    }
}
