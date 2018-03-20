using Himall.PaymentPlugin;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Himall.Plugin.Payment.Alipay.Base
{
    public class Config : ConfigBase
    {

        #region 属性

        [XmlIgnoreAttribute]
        public static string PluginListUrl { get; set; }

        /// <summary>
        /// 获取或设置合作者身份ID
        /// 合作身份者ID，以2088开头由16位纯数字组成的字符串
        /// </summary>
        [Required(ErrorMessage = "分类名称必填,且不能多于5个字符")]
        public string Partner
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设交易安全校验码
        /// 交易安全检验码，由数字和字母组成的32位字符串
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        //字符编码格式 目前支持 gbk 或 utf-8
        static string _input_charset = "utf-8";

        /// <summary>
        /// 获取字符编码格式
        /// </summary>
        [XmlIgnoreAttribute]
        public string Input_charset
        {
            get
            {
                //字符编码格式 目前支持 gbk 或 utf-8
                return _input_charset;
            }
        }

        //签名方式，选择项：RSA、DSA、MD5
        static string _sign_Type = "MD5";

        /// <summary>
        /// 获取签名方式
        /// </summary>
        public string Sign_type
        {
            get
            {
                //签名方式，选择项：RSA、DSA、MD5
                return _sign_Type;
            }
            set
            {
                _sign_Type = value;
            }
        }

        /// <summary>
        /// 获取或设置商户的私钥
        /// 如果签名方式设置为“0001”时，请设置该参数
        /// </summary>
        public string Private_key
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置支付宝的公钥
        /// 如果签名方式设置为“0001”时，请设置该参数
        /// </summary>
        public string Public_key
        {
            get;
            set;
        }
        /// <summary>
        /// 商家邮箱
        /// </summary>
        public string Seller_email
        {
            get;
            set;
        }


        /// <summary>
        /// 支付宝网关地址
        /// <para>带?或&后缀</para>
        /// </summary>
        public string GateWay { get; set; }
        /// <summary>
        /// 支付宝确认地址
        /// </summary>
        public string VeryfyUrl { get; set; }

        /// <summary>
        /// 支付宝账号
        /// </summary>
        public string AlipayAccount { get; set; }


        /// <summary>
        /// Logo
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string GetTokenService
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string GetDataService
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string GetCodeService
        {
            get;
            set;
        }

        public string Trade_Type
        {
            get;
            set;
        }
        public string HelpImage
        {
            get;
            set;
        }


        #endregion
    }
}
