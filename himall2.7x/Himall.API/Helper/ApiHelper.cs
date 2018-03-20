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

namespace Himall.API.Helper
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
    }
}
