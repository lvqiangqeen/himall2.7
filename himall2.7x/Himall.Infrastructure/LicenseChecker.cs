using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Himall.Core.Helper;

namespace Himall.Infrastructure
{

    public class ServiceCheck
    {
        public string key { set; get; }
        public int state { get; set; }
    }

    public static class LicenseChecker
    {
        private static string PostData(string url, string postData)
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

        private static string GetKey(string host)
        {
            var firstMD5 = SecureHelper.MD5(host);
            var result = SecureHelper.MD5(firstMD5 + host);
            return result;
        }

        //   private const string CacheCommercialKey = "FileCache_CommercialLicenser";

        /// <summary>
        /// 检查店铺是否已经获得了商业授权的服务
        /// </summary>
        /// <returns></returns>
        public static bool Check(out string Msg,string host)
        {
            Msg = "";
            string url = "http://ysc.kuaidiantong.cn";
            try
            {
                string result = PostData(string.Format("{0}/valid.ashx", url),
                    "action=himall&product=1&host=" + host);
                JavaScriptSerializer json = new JavaScriptSerializer();
                var check = json.Deserialize<ServiceCheck>(result);
                var sourceKey = GetKey(host);
                if (check.state == 1 && check.key == sourceKey)
                {
                    return true;
                }
                else if (check.state == 0 && check.key == sourceKey)
                {
                    Msg = "您的产品已经过期，请续费.";
                    return false;
                }
                else
                {
                    Msg = "该产品未经授权，您使用的是非正式授权软件，请到<a target=\"_blank\" href=\"http://www.hishop.com.cn/\">海商</a>购买.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                Msg = "验证产品出现异常，详情：" + ex.Message + "，请联系管理员或者软件提供商.";
                return false;
            }
        }
    }
}
