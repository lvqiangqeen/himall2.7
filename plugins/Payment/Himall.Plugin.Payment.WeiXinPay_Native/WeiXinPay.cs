using Himall.Core.Plugins;
using Himall.Core.Plugins.Payment;
using Himall.PaymentPlugin;
using Himall.WeixinPaymentBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Himall.Plugin.Payment.WeiXinPay_Native
{
    /// <summary>
    /// 微信原生支付(Native)，采用模式二
    /// </summary>
    public class WeiXinPay : ServiceBase,IPaymentPlugin
    {
        public string GetRequestUrl(string returnUrl, string notifyUrl, string orderId, decimal totalFee, string productInfo, string openId = null)
        {
            string strResult = string.Empty;
            Config payConfig = Utility<Config>.GetConfig(WorkDirectory);
            if (string.IsNullOrEmpty(payConfig.AppId))
                throw new PluginException("未设置AppId");
            if (string.IsNullOrEmpty(payConfig.MCHID))
                throw new PluginException("未设置MCHID");
            
            string strNonce = TenPayUtil.GetNoncestr();
            string strStartTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            //参数处理
            RequestHandler paraHandler = new RequestHandler();
            paraHandler.SetParameter("appid", payConfig.AppId);//微信分配的公众账号ID
            paraHandler.SetParameter("mch_id", payConfig.MCHID);//微信支付分配的商户号
            paraHandler.SetParameter("device_info", string.Empty);//可不填
            paraHandler.SetParameter("nonce_str", strNonce);
            paraHandler.SetParameter("body", productInfo);
            paraHandler.SetParameter("attach", string.Empty);//可不填
            paraHandler.SetParameter("out_trade_no", orderId);//内部订单号，订单系统全局唯一
            paraHandler.SetParameter("total_fee", ((int)(totalFee * 100)).ToString());
            paraHandler.SetParameter("spbill_create_ip", "222.240.184.122");
            paraHandler.SetParameter("time_start", strStartTime);
            paraHandler.SetParameter("time_expire", string.Empty);
            paraHandler.SetParameter("goods_tag", string.Empty);
            paraHandler.SetParameter("notify_url", notifyUrl);//支付成功通知商户URL
            paraHandler.SetParameter("trade_type", "NATIVE");//原生支付
            paraHandler.SetParameter("openid", openId);//Native模式可不填
            paraHandler.SetParameter("product_id", orderId);//Native模式必填

            string sign = paraHandler.CreateMd5Sign("key", payConfig.Key);//按约定规则生成MD5，规则参考接口文档
            paraHandler.SetParameter("sign", sign);
            
            string strXml = paraHandler.ParseXML();
            string result = TenPayV3.Unifiedorder(strXml);//调用统一接口
            XDocument xmlDocument = XDocument.Parse(result);
            if (xmlDocument == null)
                throw new PluginException("调用统一支付接口(Native)时出错：" + strXml);

            XElement e_return = xmlDocument.Element("xml").Element("return_code");
            XElement e_result = xmlDocument.Element("xml").Element("return_msg");
            if (e_return == null)
                throw new PluginException("调用统一支付接口(Native)时,返回参数异常");

            //处理返回时先判断协议错误码，再业务，最后交易状态 
            if (e_return.Value == "SUCCESS")
            {
                e_result = xmlDocument.Element("xml").Element("result_code");
                XElement e_errdes = xmlDocument.Element("xml").Element("err_code_des");
                if (e_result.Value == "SUCCESS")
                {
                    e_result = xmlDocument.Element("xml").Element("code_url");
                    strResult = e_result.Value;
                }
                else
                {
                    throw new PluginException("调用统一支付接口(Native)时,接口返回异常:" + e_errdes.Value);
                }
            }
            else
            {
                throw new PluginException("调用统一支付接口(Native)时,接口返回异常:" + e_result.Value);
            }
            return strResult;
        }
        string _HelpImage = string.Empty;
        public string HelpImage
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_HelpImage))
                    _HelpImage = Utility<Config>.GetConfig(WorkDirectory).HelpImage;
                return _HelpImage;
            }
        }


        public UrlType RequestUrlType
        {
            get { return UrlType.QRCode; }
        }

    }
}
