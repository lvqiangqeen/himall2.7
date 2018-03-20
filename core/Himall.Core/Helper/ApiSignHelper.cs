using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Himall.Core.Helper
{
    /// <summary>
    /// Api签名帮助类
    /// </summary>
    public class ApiSignHelper
    {
        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time"> DateTime时间格式</param>
        /// <returns>Unix时间戳格式</returns>
        public static long ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime dtResult = time;           
            long t = (dtResult.Ticks - startTime.Ticks) / 10000; //除10000调整为13位  
            return t;
        }

        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStamp">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        public static DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));

            long lTime = long.Parse(timeStamp + (timeStamp.Length == 13 ? "0000" : "0000000"));
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        /// <summary>
        /// 时间戳验证
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static bool CheckTimeStamp(string timestamp)
        {
            DateTime time = DateTime.Parse(timestamp);
            TimeSpan span = (TimeSpan)(DateTime.Now - time);
            return (Math.Abs(span.TotalMinutes) <= 5.0);
        }
        /// <summary>
        /// 数据排序
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static SortedDictionary<string, string> SortedData(IDictionary<string, string> dic) { 
            SortedDictionary<string, string> result = new SortedDictionary<string, string>(dic);
            return result;
        }
        /// <summary>
        /// 参数过滤 sign  sign_type
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static SortedDictionary<string, string> Parameterfilter(IDictionary<string, string> dic)
        {
            SortedDictionary<string, string> dictionary = new SortedDictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in dic)
            {
                if (((pair.Key.ToLower() != "sign") && (pair.Key.ToLower() != "sign_type")) && ((pair.Value != "") && (pair.Value != null)))
                {
                    string key = pair.Key.ToLower();
                    dictionary.Add(key, pair.Value);
                }
            }
            return dictionary;
        }
        /// <summary>
        /// 生成待签名数据字符
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string Data2Linkstring(IDictionary<string, string> dic)
        {
            SortedDictionary<string, string> dictionary = new SortedDictionary<string, string>(dic);
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in dictionary)
            {
                if (!string.IsNullOrEmpty(pair.Key) && !string.IsNullOrEmpty(pair.Value))
                {
                    builder.Append(pair.Key + pair.Value);
                }
            }
            return builder.ToString();
        }
        /// <summary>
        /// 获取签名
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="appSecret"></param>
        /// <param name="sign_type"></param>
        /// <param name="_input_charset"></param>
        /// <returns></returns>
        public static string GetSign(IDictionary<string, string> dic, string appSecret, string sign_type="MD5", string _input_charset="utf-8")
        {
            string signstr = Data2Linkstring(dic) + appSecret;
            return BuildSign(signstr, sign_type, _input_charset);
        }
        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="prestr"></param>
        /// <param name="sign_type"></param>
        /// <param name="_input_charset"></param>
        /// <returns></returns>
        public static string BuildSign(string prestr, string sign_type = "MD5", string _input_charset = "utf-8")
        {
            StringBuilder builder = new StringBuilder(0x50);
            if (sign_type.ToUpper() == "MD5")
            {
                byte[] buffer = new MD5CryptoServiceProvider().ComputeHash(Encoding.GetEncoding(_input_charset).GetBytes(prestr));
                for (int i = 0; i < buffer.Length; i++)
                {
                    builder.Append(buffer[i].ToString("x").PadLeft(2, '0'));
                }
            }
            return builder.ToString().ToUpper();
        }
        /// <summary>
        /// 生成带签名的链接
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="appSecret"></param>
        /// <param name="sign_type"></param>
        /// <param name="_input_charset"></param>
        /// <returns></returns>
        public static string Data2URIAndSign(IDictionary<string, string> dic, string appSecret, string sign_type = "MD5", string _input_charset = "utf-8")
        {
            StringBuilder builder = new StringBuilder(0x50);
            builder.Append("?");
            foreach (KeyValuePair<string, string> pair in dic)
            {
                if (!string.IsNullOrEmpty(pair.Key) && !string.IsNullOrEmpty(pair.Value))
                {
                    builder.Append(pair.Key +"="+ pair.Value+"&");
                }
            }
            string sign = GetSign(Parameterfilter(dic), appSecret, sign_type, _input_charset);
            builder.Append("sign_type=MD5&");
            builder.Append("sign=" + sign);
            return builder.ToString();
        }
        /// <summary>
        /// 签名验证
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="appSecret"></param>
        /// <param name="sign_type"></param>
        /// <param name="_input_charset"></param>
        /// <returns></returns>
        public static bool CheckSign(IDictionary<string, string> dic, string appSecret, string sign_type = "MD5", string _input_charset = "utf-8")
        {
            var sign = GetSign(Parameterfilter(dic), appSecret, sign_type, _input_charset);
            bool flag = sign == dic["sign"];
            return flag;
        }

    }
}
