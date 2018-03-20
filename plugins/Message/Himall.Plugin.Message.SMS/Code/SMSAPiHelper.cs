using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace Himall.Plugin.Message.SMS
{
   public static class SMSAPiHelper
    {
        public static Dictionary<string, string> Parameterfilter(SortedDictionary<string, string> dicArrayPre)
        {
            Dictionary<string, string> dicArray = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> temp in dicArrayPre)
            {
                if (temp.Key.ToLower() != "sign" && temp.Key.ToLower() != "sign_type" && temp.Value != "" && temp.Value != null)
                {
                    dicArray.Add(temp.Key.ToLower(), temp.Value);
                }
            }

            return dicArray;
        }

        public static string BuildSign(Dictionary<string, string> dicArray, string key, string sign_type, string _input_charset)
        {
            string prestr = CreateLinkstring(dicArray);  //把数组所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串

            prestr = prestr + key;                      //把拼接后的字符串再与安全校验码直接连接起来
            string mysign = Sign(prestr, sign_type, _input_charset);	//把最终的字符串签名，获得签名结果

            return mysign;
        }

        public static string CreateLinkstring(Dictionary<string, string> dicArray)
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
       /// 签名
       /// </summary>
       /// <param name="prestr">签名字符串</param>
       /// <param name="sign_type">签名</param>
       /// <param name="_input_charset"></param>
       /// <returns></returns>
        public static string Sign(string prestr, string sign_type, string _input_charset)
        {
            StringBuilder sb = new StringBuilder(32);
            if (sign_type.ToUpper() == "MD5")
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] t = md5.ComputeHash(Encoding.GetEncoding(_input_charset).GetBytes(prestr));
                for (int i = 0; i < t.Length; i++)
                {
                    sb.Append(t[i].ToString("x").PadLeft(2, '0'));
                }
            }
            return sb.ToString();
        }

        public static string PostData(string url, string postData)
        {
            string result = string.Empty;
            try
            {
                Uri uri = new Uri(url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                Encoding encoding = Encoding.UTF8;
                byte[] bytes = encoding.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;
                using (Stream writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }

                #region 读取服务器返回信息

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        Encoding _encodingResponse = Encoding.UTF8;
                        //if(response)
                        Stream decompress = responseStream;
                        //decompress
                        if (response.ContentEncoding.ToLower() == "gzip")
                        {
                            decompress = new GZipStream(responseStream, CompressionMode.Decompress);
                        }
                        else if (response.ContentEncoding.ToLower() == "deflate")
                        {
                            decompress = new DeflateStream(responseStream, CompressionMode.Decompress);
                        }
                        using (StreamReader readStream = new StreamReader(decompress, _encodingResponse))
                        {
                            result = readStream.ReadToEnd();
                        }
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                result = string.Format("获取信息错误：{0}", e.Message);

            }

            return result;
        }

    }
}
