using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System;
using System.Collections.Generic;
using Himall.PaymentPlugin;

namespace Himall.Plugin.Payment.Alipay_WAP.Code
{
    /// <summary>
    /// 类名：Config
    /// 功能：基础配置类
    /// 详细：设置帐户有关信息及返回路径
    /// </summary>
    public class Config:ConfigBase
    {
        

        #region 属性
        /// <summary>
        /// 获取或设置合作者身份ID
        /// 合作身份者ID，以2088开头由16位纯数字组成的字符串
        /// </summary>
        public  string partner
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设交易安全校验码
        /// </summary>
        /// 交易安全检验码，由数字和字母组成的32位字符串
        /// 如果签名方式设置为“MD5”时，请设置该参数
        public  string key
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置商户的私钥
        /// 如果签名方式设置为“0001”时，请设置该参数
        /// </summary>
        public  string private_key
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置支付宝的公钥
        /// 如果签名方式设置为“0001”时，请设置该参数
        /// </summary>
        public  string public_key
        {
            get;
            set;
        }

        /// <summary>
        /// 获取字符编码格式
        /// </summary>
        public  string input_charset
        {
            get;
            set;
        }

        /// <summary>
        /// 获取签名方式
        /// </summary>
        public  string sign_type
        {
            get;
            set;
        }
        public string seller_email
        {
            get;
            set;
        }

        public string gateWay
        {
            get;
            set;
        }
        public string getTokenService
        {
            get;
            set;
        }
        public string getDataService { get; set; }

        public string Logo { get; set; }
        /// <summary>
        /// 验证消息合法性网关
        /// </summary>
        public string veryfy_url { get; set; }
        #endregion
    }
}