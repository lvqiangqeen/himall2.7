using Himall.PaymentPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace Himall.Plugin.Payment.Alipay.Base
{
    /// <summary>
    /// 类名：Notify
    /// 功能：支付宝通知处理类
    /// 详细：处理支付宝各接口通知返回
    /// 版本：3.3
    /// 修改日期：2011-07-05
    /// '说明：
    /// 以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己网站的需要，按照技术文档编写,并非一定要使用该代码。
    /// 该代码仅供学习和研究支付宝接口使用，只是提供一个参考。
    /// 
    /// //////////////////////注意/////////////////////////////
    /// 调试通知返回时，可查看或改写log日志的写入TXT里的数据，来检查通知返回是否正常 
    /// </summary>
    public class Notify
    {
        #region 字段
        private string _partner = "";               //合作身份者ID
        private string _key = "";                   //商户的私钥
        private string _input_charset = "";         //编码格式
        private string _sign_type = "";             //签名方式

        //支付宝消息验证地址
        private string Https_veryfy_url = "https://mapi.alipay.com/gateway.do?service=notify_verify&";
        #endregion


        /// <summary>
        /// 构造函数
        /// 从配置文件中初始化变量
        /// </summary>
        /// <param name="inputPara">通知返回参数数组</param>
        /// <param name="notify_id">通知验证ID</param>
        public Notify(string workDirectory)
        {
            Config config = Utility<Config>.GetConfig(workDirectory);

            //初始化基础配置信息
            _partner = config.Partner.Trim();
            _key = config.Key.Trim();
            _input_charset = config.Input_charset.Trim().ToLower();
            _sign_type = config.Sign_type.Trim().ToUpper();
        }

        /// <summary>
        ///  验证消息是否是支付宝发出的合法消息
        /// </summary>
        /// <param name="inputPara">通知返回参数数组</param>
        /// <param name="notify_id">通知验证ID</param>
        /// <param name="sign">支付宝生成的签名结果</param>
        /// <param name="_config">配置信息</param>
        /// <param name="isSort">是否对待签名数组排序</param>
        /// <returns>验证结果</returns>
        public bool Verify(Dictionary<string, string> inputPara, string notify_id, string sign, Config _config, bool isSort = true)
        {
            //获取返回时的签名验证结果
            bool isSign = GetSignVeryfy(inputPara, sign, _config, isSort);
            //Core.Log.Debug("获取返回时的签名验证结果:isSign=" + isSign);
            //获取是否是支付宝服务器发来的请求的验证结果
            string responseTxt = "false";
            if (notify_id != null && notify_id != "") { responseTxt = GetResponseTxt(notify_id); }
            //Core.Log.Debug("支付宝服务器返回的验证结果：responseTxt=" + responseTxt);
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

        public string GetNotifyId(Dictionary<string, string> inputPara)
        {
            string result = "";
            try
            {
                //XML解析notify_data数据，获取notify_id
                string notify_id = "";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(inputPara["notify_data"]);
                notify_id = xmlDoc.SelectSingleNode("/notify/notify_id").InnerText;

                if (notify_id != "")
                {
                    result = notify_id;
                }
            }
            catch (Exception e)
            {
                result = "";
            }
            return result;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="inputPara">要解密数据</param>
        /// <returns>解密后结果</returns>
        public Dictionary<string, string> Decrypt(Dictionary<string, string> inputPara, Config config)
        {
            try
            {
                inputPara["notify_data"] = RSAFromPkcs8.decryptData(inputPara["notify_data"], config.Private_key, config.Input_charset);
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
        /// <param name="_config">配置信息</param>
        /// <param name="isSort">是否对待签名数组排序</param>
        /// <returns>签名验证结果</returns>
        private bool GetSignVeryfy(Dictionary<string, string> inputPara, string sign, Config _config, bool isSort=true)
        {
            Dictionary<string, string> sPara = new Dictionary<string, string>();

            //Core.Log.Debug("sign_type=" + inputPara["sign_type"]);
            //过滤空值、sign与sign_type参数
            sPara = UrlHelper.FilterPara(inputPara, "sign", "sign_type");

            if (!isSort)
            {
                sPara = SortNotifyPara(sPara);  //固定顺序排序
            }

            //获取待签名字符串
            string preSignStr = UrlHelper.CreateLinkString(sPara);
         // Core.Log.Debug("Public_key=" + _config.Public_key+ "\r\n 待签名字符串=" + preSignStr+ "\r\n，签名应该结果:"+sign);
            //获得签名验证结果
            bool isSgin = false;
            if (sign != null && sign != "")
            {
                switch (_config.Sign_type)
                {
                    case "MD5":
                        isSgin = Sign.VerifyMD5(preSignStr, sign, _config.Key, _config.Input_charset);
                        break;
                    case "RSA":
                        isSgin = RSAFromPkcs8.verify(preSignStr, sign, _config.Public_key, _config.Input_charset);
                        break;
                    case "0001":
                        isSgin = RSAFromPkcs8.verify(preSignStr, sign, _config.Public_key, _config.Input_charset);
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
        private string GetResponseTxt(string notify_id)
        {
            string veryfy_url = Https_veryfy_url + "partner=" + _partner + "&notify_id=" + notify_id;

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