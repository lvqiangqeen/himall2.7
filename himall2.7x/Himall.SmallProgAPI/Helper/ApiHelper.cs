using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;

using Himall.Core.Helper;
using Himall.Model;
using Himall.SmallProgAPI.Model;
using Newtonsoft.Json;

namespace Himall.SmallProgAPI.Helper
{
    public class ApiHelper
    {
        /// <summary>
        /// 检测基础参数和参数签名
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static void CheckBaseParamsAndSign(SortedDictionary<string, string> data)
        {
            if (data == null)
            {
                throw new HimallApiException(ApiErrorCode.System_Error, "no params");
            }
            if (data.Count < 1)
            {
                throw new HimallApiException(ApiErrorCode.System_Error, "no params");
            }
            string app_key = "";
            string timestamp = "";
            string sign = "";

            #region 基础检测
            if (!data.TryGetValue("app_key", out app_key))
            {
                throw new HimallApiException(ApiErrorCode.Missing_App_Key, "app_key");
            }
            if (string.IsNullOrWhiteSpace(app_key))
            {
                throw new HimallApiException(ApiErrorCode.Missing_App_Key, "app_key");
            }
            if (!data.TryGetValue("timestamp", out timestamp))
            {
                throw new HimallApiException(ApiErrorCode.Missing_Timestamp, "timestamp");
            }
            if (string.IsNullOrWhiteSpace(timestamp))
            {
                throw new HimallApiException(ApiErrorCode.Missing_Timestamp, "timestamp");
            }
            if (!ApiSignHelper.CheckTimeStamp(timestamp))
            {
                throw new HimallApiException(ApiErrorCode.Invalid_Timestamp, "timestamp");
            }
            if (!data.TryGetValue("sign", out sign))
            {
                throw new HimallApiException(ApiErrorCode.Missing_Signature, "sign");
            }
            if (string.IsNullOrWhiteSpace(sign))
            {
                throw new HimallApiException(ApiErrorCode.Missing_Signature, "sign");
            }
            #endregion

            var appbase = new AppBaseInfoHelper(app_key);
            string appSecret = appbase.AppSecret;

            SortedDictionary<string, string> tmpdata = new SortedDictionary<string, string>(data);
            tmpdata.Remove("sign");
            var signstr =ApiSignHelper.Data2Linkstring(ApiSignHelper.Parameterfilter(tmpdata));
            var tmpsign = ApiSignHelper.GetSign( ApiSignHelper.Parameterfilter(tmpdata), appSecret);

            if (!ApiSignHelper.CheckSign(data, appSecret))
            {
                throw new HimallApiException(ApiErrorCode.Invalid_Signature, "sign");
            }
        }

        /// <summary>
        /// 获取传递参数转换成字典
        /// </summary>
        /// <returns></returns>
        public static SortedDictionary<string, string> GetSortedParams(HttpRequestMessage curRequest)
        {
            HttpContextBase context = (HttpContextBase)curRequest.Properties["MS_HttpContext"];//获取传统context
            HttpRequestBase request = context.Request;//定义传统request对象
            NameValueCollection Parameters = new NameValueCollection { request.Form, request.QueryString };

            SortedDictionary<string, string> sortparmas = new SortedDictionary<string, string>();

            foreach (var item in Parameters.AllKeys)
            {
                sortparmas.Add(item, Parameters[item]);
            }
            return sortparmas;
        }
        /// <summary>
        /// 当前网址
        /// <para>参数需手动补充</para>
        /// </summary>
        public static string HostUrl { get; set; }
        
        public static Model.WxAppletUserInfo GetAppletUserInfo(HttpRequestMessage curRequest)
        {
            HttpContextBase context = (HttpContextBase)curRequest.Properties["MS_HttpContext"];//获取传统context
            string encryptedData =string.Empty;
            string iv = string.Empty;
            string sessionKey = string.Empty;
            if (context.Request["encryptedData"] != null && context.Request["iv"] != null && context.Request["session_key"] != null)
            {
                encryptedData = context.Request["encryptedData"].ToString();
                iv = context.Request["iv"].ToString();
                sessionKey = context.Request["session_key"].ToString();
            }
            if (!string.IsNullOrEmpty(encryptedData) && !(string.IsNullOrEmpty(iv)) && !string.IsNullOrEmpty(sessionKey))
            {  
                string decrydata = AESDecrypt(sessionKey, iv, encryptedData);

                if (!string.IsNullOrEmpty(decrydata))
                {
                    WxAppletUserInfo userinfo = JsonConvert.DeserializeObject<WxAppletUserInfo>(decrydata);
                    return userinfo;
                }
            }
            return null;
        }
        #region 微信小程序用户数据解密  

        /// <summary>  
        /// AES解密  
        /// </summary>  
        /// <param name="inputdata">输入的数据encryptedData</param>  
        /// <param name="AesKey">key</param>  
        /// <param name="AesIV">向量128</param>  
        /// <returns name="result">解密后的字符串</returns>  
        public static string AESDecrypt(string AesKey, string AesIV, string inputdata)
        {
                AesIV = AesIV.Replace(" ", "+");
                AesKey = AesKey.Replace(" ", "+");
                inputdata = inputdata.Replace(" ", "+");
                byte[] encryptedData = Convert.FromBase64String(inputdata);

                RijndaelManaged rijndaelCipher = new RijndaelManaged();
                rijndaelCipher.Key = Convert.FromBase64String(AesKey); // Encoding.UTF8.GetBytes(AesKey);  
                rijndaelCipher.IV = Convert.FromBase64String(AesIV);// Encoding.UTF8.GetBytes(AesIV);  
                rijndaelCipher.Mode = CipherMode.CBC;
                rijndaelCipher.Padding = PaddingMode.PKCS7;
                ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
                byte[] plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                string result = Encoding.UTF8.GetString(plainText);

                return result;
            
        }
        #endregion
    }
}
