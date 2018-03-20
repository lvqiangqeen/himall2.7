using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace Himall.Plugin.Payment.Alipay.Base
{
    public class UrlHelper
    {

        /// <summary>
        /// 除去数组中的空值和签名参数并以字母a到z的顺序排序
        /// </summary>
        /// <param name="dicArrayPre">过滤前的参数组</param>
        /// <returns>过滤后的参数组</returns>
        public static Dictionary<string, string> FilterPara(Dictionary<string, string> dicPara, params string[] ignorKeys)
        {
            Dictionary<string, string> dicArray = new Dictionary<string, string>();
            SortedDictionary<string, string> sdicPara = new SortedDictionary<string, string>(dicPara);
            foreach (KeyValuePair<string, string> temp in sdicPara)
            {
                if (!ignorKeys.Contains(temp.Key.ToLower()) && !string.IsNullOrEmpty(temp.Value))
                {
                    dicArray.Add(temp.Key, temp.Value);
                }
            }

            return dicArray;
        }

        /// <summary>
        /// 把数组所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串
        /// </summary>
        /// <param name="sArray">需要拼接的数组</param>
        /// <returns>拼接完成以后的字符串</returns>
        public static string CreateLinkString(Dictionary<string, string> dicArray)
        {
            StringBuilder prestr = new StringBuilder();
            foreach (KeyValuePair<string, string> temp in dicArray)
            {
                prestr.Append(temp.Key + "=" + temp.Value + "&");
            }

            //去掉最後一個&字符
            int nLen = prestr.Length;
            prestr.Remove(nLen - 1, 1);

            return prestr.ToString();
        }

        /// <summary>
        /// 把数组所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串，并对参数值做urlencode
        /// </summary>
        /// <param name="sArray">需要拼接的数组</param>
        /// <param name="code">字符编码</param>
        /// <returns>拼接完成以后的字符串</returns>
        public static string CreateLinkStringUrlencode(Dictionary<string, string> dicArray, Encoding code, bool valueIsUpper = false)
        {
            StringBuilder prestr = new StringBuilder();
            foreach (KeyValuePair<string, string> temp in dicArray)
            {
                string val = "";
                //val = temp.Value;
                val = HttpUtility.UrlEncode(temp.Value, code);
                if (valueIsUpper)
                {
                    val = val.ToUpper();
                }
                prestr.Append(temp.Key + "=" + val + "&");
            }

            //去掉最後一個&字符
            int nLen = prestr.Length;
            prestr.Remove(nLen - 1, 1);

            return prestr.ToString();
        }

        /// <summary>
        /// 获取支付宝GET过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public static Dictionary<string, string> GetRequestGet(NameValueCollection queryString)
        {
            int i = 0;
            Dictionary<string, string> sArray = new Dictionary<string, string>();

            // Get names of all forms into a string array.
            String[] requestItem = queryString.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], queryString[requestItem[i]]);
            }

            return sArray;
        }


        /// <summary>
        /// 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public static Dictionary<string, string> GetRequestPost(NameValueCollection form)
        {
            int i = 0;
            Dictionary<string, string> sArray = new Dictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], form[requestItem[i]]);
            }

            return sArray;
        }
        /// <summary>
        /// 拼接URL
        /// </summary>
        /// <param name="str1">URL前字符串</param>
        /// <param name="str2">URL参数字符串</param>
        /// <returns></returns>
        public static string UrlSplicing(string str1, string str2)
        {
            string result = "";
            result = str1;
            if (!string.IsNullOrWhiteSpace(result))
            {
                string lastchar = result.Substring(result.Length - 1, 1);
                if ("?&".Contains(lastchar))
                {
                    result = result.Substring(0, result.Length - 1);
                }
            }
            //是否有?
            if (result.IndexOf("?") > -1)
            {
                result = result + "&" + str2;
            }
            else
            {
                result = result + "?" + str2;
            }
            return result;
        }


    }
}
