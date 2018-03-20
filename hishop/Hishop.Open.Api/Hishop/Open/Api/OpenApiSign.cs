namespace Hishop.Open.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;

    public static class OpenApiSign
    {
        public static string BuildSign(Dictionary<string, string> dicArray, string appSecret, string sign_type, string _input_charset)
        {
            return Sign(CreateLinkstring(dicArray) + appSecret, sign_type, _input_charset);
        }

        public static bool CheckSign(SortedDictionary<string, string> tmpParas, string appSecret, ref string message)
        {
            bool flag = BuildSign(Parameterfilter(tmpParas), appSecret, "MD5", "utf-8") == tmpParas["sign"];
            message = flag ? "" : OpenApiErrorMessage.ShowErrorMsg(OpenApiErrorCode.Invalid_Signature, "sign");
            return flag;
        }

        public static bool CheckTimeStamp(string timestamp)
        {
            DateTime time = DateTime.Parse(timestamp);
            TimeSpan span = (TimeSpan) (DateTime.Now - time);
            return (span.TotalMinutes <= 10.0);
        }

        public static string CreateLinkstring(Dictionary<string, string> dicArray)
        {
            SortedDictionary<string, string> dictionary = new SortedDictionary<string, string>(dicArray);
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

        public static string GetData(string url, string method = "GET")
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = method;
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string str = reader.ReadToEnd();
            reader.Close();
            return str;
        }

        public static string GetSign(SortedDictionary<string, string> tmpParas, string keycode)
        {
            return BuildSign(Parameterfilter(tmpParas), keycode, "MD5", "utf-8");
        }

        public static Dictionary<string, string> Parameterfilter(SortedDictionary<string, string> dicArrayPre)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in dicArrayPre)
            {
                if (((pair.Key.ToLower() != "sign") && (pair.Key.ToLower() != "sign_type")) && ((pair.Value != "") && (pair.Value != null)))
                {
                    string key = pair.Key.ToLower();
                    dictionary.Add(key, pair.Value);
                }
            }
            return dictionary;
        }

        public static string PostData(string url, string postData)
        {
            string str = string.Empty;
            try
            {
                Uri requestUri = new Uri(url);
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);
                byte[] bytes = Encoding.UTF8.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    using (Stream stream2 = response.GetResponseStream())
                    {
                        Encoding encoding = Encoding.UTF8;
                        Stream stream3 = stream2;
                        if (response.ContentEncoding.ToLower() == "gzip")
                        {
                            stream3 = new GZipStream(stream2, CompressionMode.Decompress);
                        }
                        else if (response.ContentEncoding.ToLower() == "deflate")
                        {
                            stream3 = new DeflateStream(stream2, CompressionMode.Decompress);
                        }
                        using (StreamReader reader = new StreamReader(stream3, encoding))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                str = string.Format("获取信息错误：{0}", exception.Message);
            }
            return str;
        }

        public static string Sign(string prestr, string sign_type, string _input_charset)
        {
            StringBuilder builder = new StringBuilder(0x20);
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
    }
}

