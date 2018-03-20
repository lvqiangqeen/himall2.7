using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Himall.Plugin.OAuth.WeiXin.Assistant
{
    public class HttpHandler
    {
        public class BinaryData
        {
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public byte[] Binary { get; set; }
        }
        public class ClientRequest
        {
            public string Url { get; set; }
            public string HttpMethod { get; set; }
            public CookieContainer CookieContainer { get; set; }
            string Accept { get; set; }
            /// <summary>
            /// 获取或设置 GetResponse 和 GetRequestStream 方法的超时值（毫秒）
            /// </summary>
            public int Timeout { get; set; }
            public string Referer { get; set; }
            /// <summary>
            /// 获取或设置写入或读取流时的超时（以毫秒为单位）。
            /// </summary>
            public int ReadWriteTimeout { get; set; }
            public Version ProtocolVersion { get; set; }
            public bool KeepAlive { get; set; }
            public DateTime IfModifiedSince { get; set; }
            public string Expect { get; set; }
            public string ContentType { get; set; }
            string UserAgent { get; set; }
            public IDictionary<string, string> FormData { get; set; }

            #region 文件上传
            /// <summary>
            /// 以post方式上传带附件的请求时使用
            /// </summary>
            public string Boundary { get; set; }
            public IDictionary<string, BinaryData> BinaryData { get; set; }
            #endregion

            public ClientRequest(string url)
            {
                Url = url;
            }
        }

        public const string Boundary = "----WebKitFormBoundary";

        public static T GetResponseResult<T,ET>(ClientRequest clientRequest,ET err)
        {
            T result = default(T);
            JavaScriptSerializer JsonQuick = new JavaScriptSerializer();
            string strJson = GetResponseForText(clientRequest);
            if (strJson.Contains("errcode"))
            {
                err = JsonQuick.Deserialize<ET>(strJson);
            }
            else
            {
                result = JsonQuick.Deserialize<T>(strJson);
            }
            return result;
        }

        public static string GetResponseForText(ClientRequest clientRequest)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(clientRequest.Url);
            webRequest.Method = clientRequest.HttpMethod;

            HttpWebResponse webResponse = null;
            Stream stream = null;
            string result = null;

            if (webRequest.Method == WebRequestMethods.Http.Get)
            {
                //GET请求
                webResponse = (HttpWebResponse)webRequest.GetResponse();
                stream = webResponse.GetResponseStream();

                if (stream.CanRead)
                {
                    var readStream = new StreamReader(stream, Encoding.UTF8);

                    var responseContent = readStream.ReadToEnd();

                    result = responseContent;
                }
            }
            else if (webRequest.Method == WebRequestMethods.Http.Post)
            {
                //POST请求
                if (clientRequest.FormData == null)
                {
                    //不带form data数据
                    webResponse = (HttpWebResponse)webRequest.GetResponse();
                    stream = webResponse.GetResponseStream();
                    if (stream.CanRead)
                    {
                        var readStream = new StreamReader(stream, Encoding.UTF8);

                        var responseContent = readStream.ReadToEnd();

                        result = responseContent;
                    }
                }
                else if (clientRequest.FormData.Count == 0)
                {
                    //不带form data数据
                    webResponse = (HttpWebResponse)webRequest.GetResponse();
                    stream = webResponse.GetResponseStream();
                    if (stream.CanRead)
                    {
                        var readStream = new StreamReader(stream, Encoding.UTF8);

                        var responseContent = readStream.ReadToEnd();

                        result = responseContent;
                    }
                }
                else
                {
                    //带form data数据
                    webRequest.ContentType = clientRequest.ContentType;

                    StringBuilder sbFormData = new StringBuilder();
                    byte[] formDataBytes = null;
                    Stream requestWriteStream = null;

                    if (webRequest.ContentType == "application/x-www-form-urlencoded")
                    {
                        //普通post提交
                        foreach (var item in clientRequest.FormData)
                        {
                            sbFormData.Append("&" + item.Key + "=" + item.Value);
                        }

                        if (sbFormData.Length > 0)
                        {
                            sbFormData.Remove(0, 1); //移除第一个&字符
                        }

                        formDataBytes = Encoding.UTF8.GetBytes(sbFormData.ToString());
                        webRequest.ContentLength = formDataBytes.Length;

                        requestWriteStream = webRequest.GetRequestStream();
                        requestWriteStream.Write(formDataBytes, 0, formDataBytes.Length);
                        requestWriteStream.Close();

                        webResponse = (HttpWebResponse)webRequest.GetResponse();
                        stream = webResponse.GetResponseStream();
                        if (stream.CanRead)
                        {
                            var readStream = new StreamReader(stream, Encoding.UTF8);

                            var responseContent = readStream.ReadToEnd();

                            result = responseContent;
                        }
                    }
                    else if (webRequest.ContentType.StartsWith("multipart/form-data"))
                    {
                        string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(clientRequest.Url);
                        httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                        httpWebRequest.Method = clientRequest.HttpMethod;
                        httpWebRequest.KeepAlive = clientRequest.KeepAlive;
                        httpWebRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;

                        Stream requestStream = httpWebRequest.GetRequestStream();


                        byte[] boundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");

                        foreach (var item in clientRequest.FormData)
                        {
                            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);

                            string formData = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

                            formDataBytes = Encoding.UTF8.GetBytes(string.Format(formData, item.Key, item.Value));

                            requestStream.Write(formDataBytes, 0, formDataBytes.Length);


                        }

                        foreach (var item in clientRequest.BinaryData)
                        {
                            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);

                            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\";filename=\"{1}\"\r\n Content-Type: {2}\r\n\r\n";

                            string header = string.Format(headerTemplate, item.Key, item.Value.FileName, item.Value.ContentType);

                            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);

                            requestStream.Write(headerbytes, 0, headerbytes.Length);

                            requestStream.Write(item.Value.Binary, 0, item.Value.Binary.Length);
                        }


                        boundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                        requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);

                        requestStream.Close();



                        webResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        Stream responseStream = webResponse.GetResponseStream();
                        StreamReader responseReader = new StreamReader(responseStream);

                        string responseString = responseReader.ReadToEnd();

                        webResponse.Close();

                        result = responseString;
                    }
                }
            }

            return result;
        }

    }
}
