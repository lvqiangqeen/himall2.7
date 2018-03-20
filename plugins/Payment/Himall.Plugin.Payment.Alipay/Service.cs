using Himall.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Himall.Core.Plugins.Payment;
using Himall.Core.Helper;
using Himall.PaymentPlugin;
using Himall.Plugin.Payment.Alipay.Base;
using System.Web;


namespace Himall.Plugin.Payment.Alipay
{
    public class Service :_ServiceBase, IPaymentPlugin
    {
        public string GetRequestUrl(string returnUrl, string notifyUrl, string orderId, decimal totalFee,string productInfo,string openId=null)
        {

            Config config = _Config;

            //支付类型，必填，不能修改
            string paymentType = "1";

            //服务器异步通知页面路径
            notifyUrl = string.Format(notifyUrl);

            //页面跳转同步通知页面路径
            returnUrl = string.Format(returnUrl);

            //收款支付宝帐户
            string sellerEmail = config.AlipayAccount;

            //合作者身份ID
            string partner = config.Partner;

            //交易安全检验码
            string key = config.Key;

            //商户订单号
            string outTradeNo = orderId.ToString();

            //订单名称
            string subject = productInfo;

            //订单描述
            string body = "";

            //防钓鱼时间戳
            string anti_phishing_key = "";

            //客户端的IP地址,非局域网的外网IP地址
            string exter_invoke_ip = "";

            //把请求参数打包成数组
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("partner", partner);
            parameters.Add("return_url", returnUrl);
            parameters.Add("seller_email", sellerEmail);
            parameters.Add("out_trade_no", outTradeNo);
            parameters.Add("_input_charset", config.Input_charset);
            parameters.Add("service", "create_direct_pay_by_user");
           // parameters.Add("service", "trade_create_by_buyer");
            parameters.Add("payment_type", paymentType);
            parameters.Add("notify_url", notifyUrl);
            parameters.Add("subject", subject);
            parameters.Add("total_fee", totalFee.ToString("F2"));
            parameters.Add("body", body);
            parameters.Add("anti_phishing_key", anti_phishing_key);
            parameters.Add("exter_invoke_ip", exter_invoke_ip);
            parameters.Add("it_b_pay","30m");
            return Submit.BuildRequestUrl(parameters, config);
        }

        public PaymentInfo ProcessReturn(HttpRequestBase request)
        {
            var queryString = GetQuerystring(request);
            Dictionary<string, string> sPara = UrlHelper.GetRequestGet(queryString);
            Config config = Utility<Config>.GetConfig(WorkDirectory);
            PaymentInfo paymentInfo = new PaymentInfo ();
            if (sPara.Count > 0)//判断是否有带返回参数
            {
                Notify notify = new Notify(WorkDirectory);

                bool verifyResult = notify.Verify(sPara, queryString["notify_id"], queryString["sign"], config);
                Core.Log.Debug("ProcessReturn verifyResult=" + verifyResult);
                if (verifyResult && (queryString["trade_status"] == "TRADE_FINISHED" || queryString["trade_status"] == "TRADE_SUCCESS"))//验证成功
                {
                    paymentInfo.OrderIds = queryString["out_trade_no"].Split(',').Select(t => long.Parse(t));//商户订单号
                    paymentInfo.TradNo = queryString["trade_no"];//支付宝交易流水号
                    paymentInfo.TradeTime = TypeHelper.StringToDateTime(queryString["notify_time"]);//交易时间
                }
                else//验证失败
                {
                    throw new ApplicationException("支付宝支付返回请求验证失败,QueryString:" + queryString.ToString());
                }
            }
            else
                throw new ApplicationException("支付宝支付返回请求未带参数,QueryString:" + queryString.ToString());
            return paymentInfo;
        }


        public PaymentInfo ProcessNotify(HttpRequestBase request)
        {
            var queryString = GetQuerystring(request);

            Dictionary<string, string> sPara = UrlHelper.GetRequestPost(queryString);
            PaymentInfo paymentInfo = null ;

            if (queryString.Count > 0)//判断是否有带返回参数
            {
                Config config = GetConfig();

                Notify notify = new Notify(WorkDirectory);

                bool verifyResult = notify.Verify(sPara, queryString["notify_id"], queryString["sign"],config);
                Core.Log.Debug("ProcessNotify verifyResult="+ verifyResult);
                if (verifyResult && (queryString["trade_status"] == "TRADE_FINISHED" || queryString["trade_status"] == "TRADE_SUCCESS"))//验证成功
                {
                    string tradeNo = queryString["trade_no"];//支付宝交易号
                    DateTime tradeTime = TypeHelper.StringToDateTime(queryString["gmt_payment"]);//交易时间

                    paymentInfo = new PaymentInfo()
                    {
                        OrderIds = queryString["out_trade_no"].Split(',').Select(t=>long.Parse(t)),//商户订单号
                        TradNo = queryString["trade_no"],//支付宝交易流水号
                        TradeTime = TypeHelper.StringToDateTime(queryString["gmt_payment"])//交易时间
                    };
                }
                else//验证失败
                {
                    throw new ApplicationException("支付宝支付Notify请求验证失败,QueryString:" + queryString.ToString());
                }
            }
            else
                throw new ApplicationException("支付宝支付Notify请求未带参数,QueryString:" + queryString.ToString());
            return paymentInfo;
        }


        NameValueCollection GetQuerystring(HttpRequestBase request)
        {
            NameValueCollection querystring;
            if (request.HttpMethod == "POST")
                querystring = request.Form;
            else
                querystring = request.QueryString;
            return querystring;
        }

        public string PluginListUrl
        {
            set { Config.PluginListUrl = value; }
        }

        public void CheckCanEnable()
        {
            Config config = Utility<Config>.GetConfig(WorkDirectory);
            if (string.IsNullOrWhiteSpace(config.AlipayAccount))
                throw new PluginConfigException("未设置支付宝账号");

            if (string.IsNullOrWhiteSpace(config.GateWay))
                throw new PluginConfigException("未设置支付宝网关");


            if (string.IsNullOrWhiteSpace(config.Input_charset))
                throw new PluginConfigException("未设置编码格式设置");

            if (string.IsNullOrWhiteSpace(config.Key))
                throw new PluginConfigException("未设置安全校验Key");

            if (string.IsNullOrWhiteSpace(config.Partner))
                throw new PluginConfigException("未设置合作者身份ID");

            if (string.IsNullOrWhiteSpace(config.Sign_type))
                throw new PluginConfigException("未设置签名方式");

            if (string.IsNullOrWhiteSpace(config.VeryfyUrl))
                throw new PluginConfigException("未设置支付宝确认地址");
        }


        public Himall.Core.Plugins.FormData GetFormData()
        {
            Config config = Utility<Config>.GetConfig(WorkDirectory);

            var formData = new Himall.Core.Plugins.FormData()
            {
                Items = new Himall.Core.Plugins.FormData.FormItem[] { 

                    //Partner
                   new  Himall.Core.Plugins.FormData.FormItem(){
                     DisplayName = "合作者身份ID",
                     Name = "Partner",
                     IsRequired = true,
                      Type= Himall.Core.Plugins.FormData.FormItemType.text,
                      Value=config.Partner
                   },

                   //Key
                   new  Himall.Core.Plugins.FormData.FormItem(){
                     DisplayName = "交易安全检验码",
                     Name = "Key",
                     IsRequired = true,
                       Type= Himall.Core.Plugins.FormData.FormItemType.text,
                       Value=config.Key
                   },
                   
                   //AlipayAccount
                   new  Himall.Core.Plugins.FormData.FormItem(){
                     DisplayName = "收款支付宝帐户",
                     Name = "AlipayAccount",
                     IsRequired = true,
                      Type= Himall.Core.Plugins.FormData.FormItemType.text,
                      Value=config.AlipayAccount
                   },
                }

            };
            return formData;
        }

        public void SetFormValues(IEnumerable<KeyValuePair<string, string>> values)
        {
            var partnerItem = values.FirstOrDefault(item => item.Key == "Partner");
            if (string.IsNullOrWhiteSpace(partnerItem.Value))
                throw new PluginConfigException("合作者身份ID不能为空");

            var keyItem = values.FirstOrDefault(item => item.Key == "Key");
            if (string.IsNullOrWhiteSpace(keyItem.Value))
                throw new PluginConfigException("交易安全检验码不能为空");

            var alipayAccountItem = values.FirstOrDefault(item => item.Key == "AlipayAccount");
            if (string.IsNullOrWhiteSpace(alipayAccountItem.Value))
                throw new PluginConfigException("收款支付宝帐户不能为空");


            Config oldConfig = Utility<Config>.GetConfig(WorkDirectory);
            oldConfig.AlipayAccount = alipayAccountItem.Value;
            oldConfig.Key = keyItem.Value;
            oldConfig.Partner = partnerItem.Value;
            Utility<Config>.SaveConfig(oldConfig, WorkDirectory);
        }

        string _logo;

        public string Logo
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_logo))
                    _logo = Utility<Config>.GetConfig(WorkDirectory).Logo;
                return _logo;
            }
            set
            {
                _logo = value;
            }
        }

        public string ConfirmPayResult()
        {
            return "success";
        }




        public UrlType RequestUrlType
        {
            get { return UrlType.Page; }
        }


        public string HelpImage
        {
            get { throw new NotImplementedException(); }
        }
    }
}
