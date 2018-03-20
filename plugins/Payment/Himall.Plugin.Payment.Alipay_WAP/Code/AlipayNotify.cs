using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System;
using System.Collections.Generic;

namespace Himall.Plugin.Payment.Alipay_WAP.Code
{
    /// <summary>
    /// 功能：支付宝通知处理类
    /// 详细：处理支付宝各接口通知返回
    /// //////////////////////注意/////////////////////////////
    /// 调试通知返回时，可查看或改写log日志的写入TXT里的数据，来检查通知返回是否正常 
    /// </summary>
    public class Notify
    {
                   
        /// <summary>
        ///  验证消息是否是支付宝发出的合法消息，验证callback
        /// </summary>
        /// <param name="inputPara">通知返回参数数组</param>
        /// <param name="sign">支付宝生成的签名结果</param>
        /// <returns>验证结果</returns>
        public bool VerifyReturn(Dictionary<string, string> inputPara, string sign ,Config config)
        {
            //获取返回时的签名验证结果
            bool isSign = GetSignVeryfy(inputPara, sign,true,config);

            //写日志记录（若要调试，请取消下面两行注释）
            //string sWord = "isSign=" + isSign.ToString() + "\n 返回回来的参数：" + GetPreSignStr(inputPara) + "\n ";
            //Core.LogResult(sWord);

            //判断isSign是否为true
            //isSign不是true，与安全校验码、请求时的参数格式（如：带自定义参数等）、编码格式有关
            if (isSign)//验证成功
            {
                return true;
            }
            else//验证失败
            {
                return false;
            }
        }

        /// <summary>
        ///  验证消息是否是支付宝发出的合法消息，验证服务器异步通知
        /// </summary>
        /// <param name="inputPara">通知返回参数数组</param>
        /// <param name="sign">支付宝生成的签名结果</param>
        /// <returns>验证结果</returns>
        public bool VerifyNotify(Dictionary<string, string> inputPara, string sign,Config config)
        {
            //解密
            if (config.sign_type == "0001")
            {
                inputPara = Decrypt(inputPara,config);
            }

            //获取是否是支付宝服务器发来的请求的验证结果
            string responseTxt = "true";
            try
            {
                //XML解析notify_data数据，获取notify_id
                string notify_id = "";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(inputPara["notify_data"]);
                notify_id = xmlDoc.SelectSingleNode("/notify/notify_id").InnerText;

                if (notify_id != "") { responseTxt = GetResponseTxt(notify_id,config); }
            }
            catch(Exception e)
            {
                responseTxt = e.ToString();
            }

            //获取返回时的签名验证结果
            bool isSign = GetSignVeryfy(inputPara, sign, false,config);

            //写日志记录（若要调试，请取消下面两行注释）
            //string sWord = "responseTxt=" + responseTxt + "\n isSign=" + isSign.ToString() + "\n 返回回来的参数：" + GetPreSignStr(inputPara) + "\n ";
            //Core.LogResult(sWord);

            //判断responsetTxt是否为true，isSign是否为true
            //responsetTxt的结果不是true，与服务器设置问题、合作身份者ID、notify_id一分钟失效有关
            //isSign不是true，与安全校验码、请求时的参数格式（如：带自定义参数等）、编码格式有关
            if (responseTxt == "true" && isSign)//验证成功
            {
                return true;
            }
            else//验证失败
            {
                return false;
            }
        }

        /// <summary>
        /// 获取待签名字符串
        /// </summary>
        /// <param name="inputPara">通知返回参数数组</param>
        /// <returns>待签名字符串</returns>
        public string GetPreSignStr(Dictionary<string, string> inputPara)
        {
            Dictionary<string, string> sPara = new Dictionary<string, string>();

            //过滤空值、sign与sign_type参数
            sPara = Core.FilterPara(inputPara);

            //根据字母a到z的顺序把参数排序
            sPara = Core.SortPara(sPara);

            //获取待签名字符串
            string preSignStr = Core.CreateLinkString(sPara);

            return preSignStr;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="inputPara">要解密数据</param>
        /// <returns>解密后结果</returns>
        public Dictionary<string, string> Decrypt(Dictionary<string, string> inputPara,Config config)
        {
            try
            {
                inputPara["notify_data"] = RSAFromPkcs8.decryptData(inputPara["notify_data"], config.private_key, config.input_charset);
            }
            catch (Exception e) { }

            return inputPara;
        }

        /// <summary>
        /// 异步通知时，对参数做固定排序
        /// </summary>
        /// <param name="dicArrayPre">排序前的参数组</param>
        /// <returns>排序后的参数组</returns>
        private Dictionary<string, string> SortNotifyPara(Dictionary<string, string> dicArrayPre)
        {
            Dictionary<string, string> sPara = new Dictionary<string, string>();
            sPara.Add("service", dicArrayPre["service"]);
            sPara.Add("v", dicArrayPre["v"]);
            sPara.Add("sec_id", dicArrayPre["sec_id"]);
            sPara.Add("notify_data", dicArrayPre["notify_data"]);

            return sPara;
        }

        /// <summary>
        /// 获取返回时的签名验证结果
        /// </summary>
        /// <param name="inputPara">通知返回参数数组</param>
        /// <param name="sign">对比的签名结果</param>
        /// <param name="isSort">是否对待签名数组排序</param>
        /// <returns>签名验证结果</returns>
        private bool GetSignVeryfy(Dictionary<string, string> inputPara, string sign,bool isSort,Config config)
        {
            Dictionary<string, string> sPara = new Dictionary<string, string>();

            //过滤空值、sign与sign_type参数
            sPara = Core.FilterPara(inputPara);

            if (isSort)
            {
                //根据字母a到z的顺序把参数排序
                sPara = Core.SortPara(sPara);
            }
            else
            {
                sPara = SortNotifyPara(sPara);
            }
            
            //获取待签名字符串
            string preSignStr = Core.CreateLinkString(sPara);

            //获得签名验证结果
            bool isSgin = false;
            if (sign != null && sign != "")
            {
                switch (config.sign_type)
                {
                    case "MD5":
                        isSgin = AlipayMD5.Verify(preSignStr, sign, config.key, config.input_charset);
                        break;
                    case "RSA":
                        isSgin = RSAFromPkcs8.verify(preSignStr, sign, config.public_key, config.input_charset);
                        break;
                    case "0001":
                        isSgin = RSAFromPkcs8.verify(preSignStr, sign, config.public_key, config.input_charset);
                        break;
                    default:
                        break;
                }
            }

            return isSgin;
        }

        /// <summary>
        /// 获取是否是支付宝服务器发来的请求的验证结果
        /// </summary>
        /// <param name="notify_id">通知验证ID</param>
        /// <returns>验证结果</returns>
        private string GetResponseTxt(string notify_id,Config config)
        {
            string veryfy_url = config.veryfy_url + "partner=" + config.partner + "&notify_id=" + notify_id;

            //获取远程服务器ATN结果，验证是否是支付宝服务器发来的请求
            string responseTxt = Get_Http(veryfy_url, 120000);

            return responseTxt;
        }

        /// <summary>
        /// 获取远程服务器ATN结果
        /// </summary>
        /// <param name="strUrl">指定URL路径地址</param>
        /// <param name="timeout">超时时间设置</param>
        /// <returns>服务器ATN结果</returns>
        private string Get_Http(string strUrl, int timeout)
        {
            string strResult;
            try
            {
                HttpWebRequest myReq = (HttpWebRequest)HttpWebRequest.Create(strUrl);
                myReq.Timeout = timeout;
                HttpWebResponse HttpWResp = (HttpWebResponse)myReq.GetResponse();
                Stream myStream = HttpWResp.GetResponseStream();
                StreamReader sr = new StreamReader(myStream, Encoding.Default);
                StringBuilder strBuilder = new StringBuilder();
                while (-1 != sr.Peek())
                {
                    strBuilder.Append(sr.ReadLine());
                }

                strResult = strBuilder.ToString();
            }
            catch (Exception exp)
            {
                strResult = "错误：" + exp.Message;
            }

            return strResult;
        }
    }
}