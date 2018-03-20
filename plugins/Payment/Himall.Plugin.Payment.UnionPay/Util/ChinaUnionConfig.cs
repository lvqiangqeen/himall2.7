using Himall.PaymentPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Plugin.Payment.UnionPay.Util
{
    public class ChinaUnionConfig:ConfigBase
    {
        /*
         证书ID certId    通过MPI插件获取
         签名   signature 填写对报文摘要的签名
         前台通知地址 frontUrl	前台返回商户结果时使用，前台类交易需上送
         后台通知地址 backUrl   后台返回商户结果时使用，如上送，则发送商户后台交易结果通知
         商户订单号   orderId   商户端生成(不能含“-”或“_”)
         订单发送时间 txnTime   商户发送交易时间 
         交易金额     txnAmt	交易单位为分
        */
        /// <summary>
        /// 商户ID
        /// </summary>
        public string merId { get; set; }
        /// <summary>
        /// 银联接口版本
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// 编码方式:UTF-8
        /// </summary>
        public string encoding { get; set; }

        /// <summary>
        /// 签名方法:01（表示采用的是RSA）
        /// </summary>
        public string signMethod { get; set; }

        /// <summary>
        /// 交易类型：01
        /// </summary>
        public string txnType { get; set; }

        /// <summary>
        /// 交易子类 默认00
        /// </summary>
        public string txnSubType { get; set; }

        /// <summary>
        /// 产品类型 : 000201 B2C网关支付
        /// </summary>
        public string bizType { get; set; }
        /// <summary>
        /// 渠道类型 07：互联网
        /// </summary>
        public string channelType { get; set; }

        /// <summary>
        /// 接入类型 0：普通商户直连接入 2：平台类商户接入
        /// </summary>
        public string accessType { get; set; }

        /// <summary>
        /// 交易币种 : 默认为156（人民币）
        /// </summary>
        public string currencyCode { get; set; }



        public string signCertpath { get; set; }

        public string signCertpwd { get; set; }

        public string signCerttype { get; set; }

        public string encryptCertpath { get; set; }
        

        public string frontTransUrl { get; set; }

        public string backTransUrl { get; set; }

        public string singleQueryUrl { get; set; }

        public string Logo { get; set; }
    }
}
