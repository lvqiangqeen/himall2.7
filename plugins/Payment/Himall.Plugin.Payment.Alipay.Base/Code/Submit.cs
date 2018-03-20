using Himall.PaymentPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Web;

namespace Himall.Plugin.Payment.Alipay.Base
{
    /// <summary>
    /// 类名：Submit
    /// 功能：支付宝各接口请求提交类
    /// 详细：构造支付宝各接口表单HTML文本，获取远程HTTP数据
    /// 版本：3.3
    /// 修改日期：2011-07-05
    /// 说明：
    /// 以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己网站的需要，按照技术文档编写,并非一定要使用该代码。
    /// 该代码仅供学习和研究支付宝接口使用，只是提供一个参考
    /// </summary>
    public class Submit
    {
        /// <summary>
        /// 生成请求时的签名
        /// </summary>
        /// <param name="sPara">请求给支付宝的参数数组</param>
        /// <param name="_config">配置信息</param>
        /// <returns>签名结果</returns>
        private static string BuildRequestMysign(Dictionary<string, string> sParaTemp, Config _config)
        {
            //把数组所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串
            string prestr = UrlHelper.CreateLinkString(sParaTemp);

            //把最终的字符串签名，获得签名结果
            string mysign = "";
            switch (_config.Sign_type.ToUpper())
            {
                case "MD5":
                    mysign = Sign.MD5(prestr, _config.Key, _config.Input_charset);
                    break;
                case "RSA":
                    mysign = RSAFromPkcs8.sign(prestr, _config.Private_key, _config.Input_charset);
                    break;
                case "0001":
                    mysign = RSAFromPkcs8.sign(prestr, _config.Private_key, _config.Input_charset);
                    break;
                default:
                    mysign = "";
                    break;
            }

            return mysign;
        }

        /// <summary>
        /// 生成要请求给支付宝的参数数组
        /// </summary>
        /// <param name="sParaTemp">请求前的参数数组</param>
        /// <param name="_config">配置信息</param>
        /// <returns>要请求的参数数组</returns>
        private static Dictionary<string, string> BuildRequestPara(Dictionary<string, string> sParaTemp, Config _config)
        {
            //待签名请求参数数组
            Dictionary<string, string> sPara = new Dictionary<string, string>();
            //签名结果
            string mysign = "";

            //过滤签名参数数组
            sPara = UrlHelper.FilterPara(sParaTemp, "sign", "sign_type");

            //获得签名结果
            mysign = BuildRequestMysign(sPara, _config);

            //签名结果与签名方式加入请求提交参数组中
            sPara.Add("sign", mysign);
            if (sPara["service"] != "alipay.wap.trade.create.direct" && sPara["service"] != "alipay.wap.auth.authAndExecute")
            {
                sPara.Add("sign_type", _config.Sign_type);
            }

            return sPara;
        }



        /// <summary>
        /// 生成要请求给支付宝的参数数组
        /// </summary>
        /// <param name="sParaTemp">请求前的参数数组</param>
        /// <param name="code">字符编码</param>
        /// <param name="_config">配置信息</param>
        /// <returns>要请求的参数数组字符串</returns>
        public static string BuildRequestParaToString(Dictionary<string, string> sParaTemp, Encoding code, Config _config, bool valueIsUpper=false)
        {
            //待签名请求参数数组
            Dictionary<string, string> sPara = new Dictionary<string, string>();
            sPara = BuildRequestPara(sParaTemp, _config);

            //把参数组中所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串，并对参数值做urlencode
            string strRequestData = UrlHelper.CreateLinkStringUrlencode(sPara, code, valueIsUpper);

            return strRequestData;
        }
        /// <summary>
        /// 生成请求接口URL
        /// </summary>
        /// <param name="sParaTemp">参数列表</param>
        /// <param name="_config">配置信息</param>
        /// <param name="gateway">网关地址，为空会从而config里取</param>
        /// <param name="valueIsUpper">参数值是否转换大字</param>
        /// <returns>URL（http://......）</returns>
        public static string BuildRequestUrl(Dictionary<string, string> sParaTemp, Config _config, string gateway = "", bool valueIsUpper=false)
        {
            if (string.IsNullOrWhiteSpace(gateway))
            {
                gateway = _config.GateWay;
            }
            string strPara = BuildRequestParaToString(sParaTemp, Encoding.GetEncoding(_config.Input_charset), _config, valueIsUpper);
            string strUrl = UrlHelper.UrlSplicing(gateway, strPara);
            return strUrl;
        }

        /// <summary>
        /// 建立请求，以表单HTML形式构造（默认）
        /// </summary>
        /// <param name="sParaTemp">请求参数数组</param>
        /// <param name="_config">配置信息</param>
        /// <param name="strButtonValue">确认按钮显示文字</param>
        /// <param name="strMethod">提交方式。两个值可选：post、get</param>
        /// <param name="isAutoSubmit">是否自动提交,默认true,为true时隐藏提交按钮</param>
        /// <returns>提交表单HTML文本</returns>
        public static string BuildRequest(Dictionary<string, string> sParaTemp, Config _config, string strButtonValue="", string strMethod="post",bool isAutoSubmit=true)
        {
            //待请求参数数组
            Dictionary<string, string> dicPara = new Dictionary<string, string>();
            dicPara = BuildRequestPara(sParaTemp, _config);

            StringBuilder sbHtml = new StringBuilder();

            sbHtml.Append("<form id='alipaysubmit' name='alipaysubmit' action='" + _config.GateWay + "_input_charset=" + _config.Input_charset + "' method='" + strMethod.ToLower().Trim() + "'>");

            foreach (KeyValuePair<string, string> temp in dicPara)
            {
                sbHtml.Append("<input type='hidden' name='" + temp.Key + "' value='" + temp.Value + "'/>");
            }

            //submit按钮控件请不要含有name属性
            sbHtml.Append("<input type='submit' value='" + strButtonValue + "' " + (isAutoSubmit ? "style='display:none;'" : "") + "></form>");

            if (isAutoSubmit)
            {
                sbHtml.Append("<script>document.forms['alipaysubmit'].submit();</script>");
            }

            return sbHtml.ToString();
        }


        /// <summary>
        /// 建立请求，以模拟远程HTTP的POST请求方式构造并获取支付宝的处理结果
        /// </summary>
        /// <param name="sParaTemp">请求参数数组</param>
        /// <param name="_config">配置信息</param>
        /// <returns>支付宝处理结果</returns>
        public static string BuildRequest(Dictionary<string, string> sParaTemp, Config _config)
        {
            Encoding code = Encoding.GetEncoding(_config.Input_charset);

            //待请求参数数组字符串
            string strRequestData = BuildRequestParaToString(sParaTemp, code, _config);
            //Himall.Core.Log.Debug("strRequestData=" + strRequestData);
            //把数组转换成流中所需字节数组类型
            byte[] bytesRequestData = code.GetBytes(strRequestData);

            //构造请求地址
            string strUrl = UrlHelper.UrlSplicing(_config.GateWay, "_input_charset=" + _config.Input_charset);

            //请求远程HTTP
            string strResult = "";
            try
            {
                //设置HttpWebRequest基本信息
                HttpWebRequest myReq = (HttpWebRequest)HttpWebRequest.Create(strUrl);
                myReq.Method = "post";
                myReq.ContentType = "application/x-www-form-urlencoded";

                //填充POST数据
                myReq.ContentLength = bytesRequestData.Length;
                Stream requestStream = myReq.GetRequestStream();
                requestStream.Write(bytesRequestData, 0, bytesRequestData.Length);
                requestStream.Close();

                //发送POST数据请求服务器
                HttpWebResponse HttpWResp = (HttpWebResponse)myReq.GetResponse();
                Stream myStream = HttpWResp.GetResponseStream();

                //获取服务器返回信息
                StreamReader reader = new StreamReader(myStream, code);
                StringBuilder responseData = new StringBuilder();
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    responseData.Append(line);
                }

                //释放
                myStream.Close();

                strResult = responseData.ToString();
            }
            catch (Exception exp)
            {
                strResult = "报错：" + exp.Message;
            }

            return strResult;
        }

        /// <summary>
        /// 建立请求，以模拟远程HTTP的POST请求方式构造并获取支付宝的处理结果，带文件上传功能
        /// </summary>
        /// <param name="sParaTemp">请求参数数组</param>
        /// <param name="strMethod">提交方式。两个值可选：post、get</param>
        /// <param name="fileName">文件绝对路径</param>
        /// <param name="data">文件数据</param>
        /// <param name="contentType">文件内容类型</param>
        /// <param name="lengthFile">文件长度</param>
        /// <param name="_config">配置信息</param>
        /// <returns>支付宝处理结果</returns>
        public static string BuildRequest(Dictionary<string, string> sParaTemp, string strMethod, string fileName, byte[] data, string contentType, int lengthFile, Config _config)
        {

            //待请求参数数组
            Dictionary<string, string> dicPara = new Dictionary<string, string>();
            dicPara = BuildRequestPara(sParaTemp, _config);

            //构造请求地址
            string strUrl = UrlHelper.UrlSplicing(_config.GateWay, "_input_charset=" + _config.Input_charset);

            //设置HttpWebRequest基本信息
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            //设置请求方式：get、post
            request.Method = strMethod;
            //设置boundaryValue
            string boundaryValue = DateTime.Now.Ticks.ToString("x");
            string boundary = "--" + boundaryValue;
            request.ContentType = "\r\nmultipart/form-data; boundary=" + boundaryValue;
            //设置KeepAlive
            request.KeepAlive = true;
            //设置请求数据，拼接成字符串
            StringBuilder sbHtml = new StringBuilder();
            foreach (KeyValuePair<string, string> key in dicPara)
            {
                sbHtml.Append(boundary + "\r\nContent-Disposition: form-data; name=\"" + key.Key + "\"\r\n\r\n" + key.Value + "\r\n");
            }
            sbHtml.Append(boundary + "\r\nContent-Disposition: form-data; name=\"withhold_file\"; filename=\"");
            sbHtml.Append(fileName);
            sbHtml.Append("\"\r\nContent-Type: " + contentType + "\r\n\r\n");
            string postHeader = sbHtml.ToString();
            //将请求数据字符串类型根据编码格式转换成字节流
            Encoding code = Encoding.GetEncoding(_config.Input_charset);
            byte[] postHeaderBytes = code.GetBytes(postHeader);
            byte[] boundayBytes = Encoding.ASCII.GetBytes("\r\n" + boundary + "--\r\n");
            //设置长度
            long length = postHeaderBytes.Length + lengthFile + boundayBytes.Length;
            request.ContentLength = length;

            //请求远程HTTP
            Stream requestStream = request.GetRequestStream();
            Stream myStream;
            try
            {
                //发送数据请求服务器
                requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                requestStream.Write(data, 0, lengthFile);
                requestStream.Write(boundayBytes, 0, boundayBytes.Length);
                HttpWebResponse HttpWResp = (HttpWebResponse)request.GetResponse();
                myStream = HttpWResp.GetResponseStream();
            }
            catch (WebException e)
            {
                return e.ToString();
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                }
            }

            //读取支付宝返回处理结果
            StreamReader reader = new StreamReader(myStream, code);
            StringBuilder responseData = new StringBuilder();

            String line;
            while ((line = reader.ReadLine()) != null)
            {
                responseData.Append(line);
            }
            myStream.Close();
            return responseData.ToString();
        }

        /// <summary>
        /// 解析远程模拟提交后返回的信息
        /// </summary>
        /// <param name="strText">要解析的字符串</param>
        /// <param name="_config">配置信息</param>
        /// <returns>解析结果</returns>
        public static Dictionary<string, string> ParseResponse(string strText, Config _config)
        {
            //以“&”字符切割字符串
            string[] strSplitText = strText.Split('&');
            //把切割后的字符串数组变成变量与数值组合的字典数组
            Dictionary<string, string> dicText = new Dictionary<string, string>();
            for (int i = 0; i < strSplitText.Length; i++)
            {
                //获得第一个=字符的位置
                int nPos = strSplitText[i].IndexOf('=');
                //获得字符串长度
                int nLen = strSplitText[i].Length;
                //获得变量名
                string strKey = strSplitText[i].Substring(0, nPos);
                //获得数值
                string strValue = strSplitText[i].Substring(nPos + 1, nLen - nPos - 1);
                //放入字典类数组中
                strValue = HttpUtility.UrlDecode(strValue);
                dicText.Add(strKey, strValue);
            }

            if (dicText.ContainsKey("res_data"))
            {
                //解析加密部分字符串（RSA与MD5区别仅此一句）
                if (_config.Sign_type == "0001")
                {
                    dicText["res_data"] = RSAFromPkcs8.decryptData(dicText["res_data"], _config.Private_key, _config.Input_charset);
                }

                //token从res_data中解析出来（也就是说res_data中已经包含token的内容）
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.LoadXml(dicText["res_data"]);
                    string strRequest_token = xmlDoc.SelectSingleNode("/direct_trade_create_res/request_token").InnerText;
                    dicText.Add("request_token", strRequest_token);
                }
                catch (Exception exp)
                {
                    dicText.Add("res_error", exp.ToString());
                }
            }
            else if (dicText.ContainsKey("res_error"))
            {
                //token从res_data中解析出来（也就是说res_data中已经包含token的内容）
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.LoadXml(dicText["res_error"]);
                    string strRequest_token = xmlDoc.SelectSingleNode("/err/detail").InnerText;
                    dicText["res_error"] = strRequest_token;
                    //dicText.Add("res_error", strRequest_token);
                }
                catch (Exception exp)
                {
                    dicText.Add("res_error", exp.ToString());
                }
            }

            return dicText;
        }

        /// <summary>
        /// 用于防钓鱼，调用接口query_timestamp来获取时间戳的处理函数
        /// 注意：远程解析XML出错，与IIS服务器配置有关
        /// </summary>
        /// <param name="_config">配置信息</param>
        /// <returns>时间戳字符串</returns>
        public static string Query_timestamp(Config _config)
        {
            string GATEWAY_NEW = _config.GateWay;
            string url = UrlHelper.UrlSplicing(GATEWAY_NEW, "service=query_timestamp&partner=" + _config.Partner + "&_input_charset=" + _config.Input_charset);
            string encrypt_key = "";

            XmlTextReader Reader = new XmlTextReader(url);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Reader);

            encrypt_key = xmlDoc.SelectSingleNode("/alipay/response/timestamp/encrypt_key").InnerText;

            return encrypt_key;
        }
    }
}