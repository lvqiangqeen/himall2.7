using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core.Plugins.Payment;
using Himall.Core.Plugins;
using Himall.PaymentPlugin;
using Himall.Core;
using System.Collections.Specialized;
using System.Xml;
using Himall.Plugin.Payment.Alipay.Base;

namespace Himall.Plugin.Payment.Alipay_WAP
{
    public class Service : _ServiceBase, IPaymentPlugin
    {

        static Config _config = null;

        public FormData GetFormData()
        {
            if (_config == null)
            {
                _config = Utility<Config>.GetConfig(WorkDirectory);
            }
            FormData formData = new FormData();
            formData.Items = new FormData.FormItem[] { 
            new FormData.FormItem(){ 
                DisplayName="合作者身份ID"
                , IsRequired=true, Name="partner"
                , Type= FormData.FormItemType.text
                , Value=_config.Partner
            }
            ,new FormData.FormItem(){
                DisplayName="商户支付宝帐号"
                , IsRequired=true
                , Name="seller_email"
                , Type= FormData.FormItemType.text
                , Value=_config.Seller_email
            }
            //,new FormData.FormItem(){
            //    DisplayName="加密方式(MD5、0001)"
            //    , IsRequired=true, Name="sign_type"
            //    , Type= FormData.FormItemType.text
            //    , Value=_config.sign_type
            //}
            ,new FormData.FormItem(){
                DisplayName="交易安全校验码"
                , IsRequired=true, Name="key"
                , Type= FormData.FormItemType.text
                , Value=_config.Key
            }
            //,new FormData.FormItem(){ 
            //    DisplayName="商户的私钥"
            //    , IsRequired=false, Name="private_key"
            //    , Type= FormData.FormItemType.text
            //    , Value=_config.private_key
            //}
            //,new FormData.FormItem(){
            //    DisplayName="支付宝的公钥"
            //    , IsRequired=false, Name="public_key"
            //    , Type= FormData.FormItemType.text
            //    , Value=_config.public_key
            //}
             
            };
            return formData;

        }

        public string GetRequestUrl(string returnUrl, string notifyUrl, string orderId, decimal totalFee, string productInfo, string openId = null)
        {

            if (string.IsNullOrEmpty(productInfo))
                throw new PluginConfigException("商品信息不能为空!");
            if (string.IsNullOrEmpty(orderId))
                throw new PluginConfigException("订单号不能为空!");
            if (string.IsNullOrEmpty(returnUrl))
                throw new PluginConfigException("返回URL不能为空!");

            if (_config == null)
            {
                _config = Utility<Config>.GetConfig(WorkDirectory);
            }

            string strResult = string.Empty;
            Dictionary<string, string> dicPara = new Dictionary<string, string>();
            dicPara.Add("service", _config.GetTokenService);//取令牌接口
            dicPara.Add("format", "xml");//固定参数
            dicPara.Add("v", "2.0");//固定参数
            dicPara.Add("partner", _config.Partner);//合作者ID，支付宝提供
            dicPara.Add("req_id", System.DateTime.Now.ToString("yyyyMMddHHmmss"));
            dicPara.Add("sec_id", _config.Sign_type);//签名方式，暂时使用MD5
            dicPara.Add("_input_charset", "utf-8");

            //**************************
            #region 整理请求数据
            StringBuilder reqdata = new StringBuilder();
            reqdata.Append("<direct_trade_create_req>");

            reqdata.Append("<notify_url>");
            reqdata.Append(notifyUrl);
            reqdata.Append("</notify_url>");
            //用户购买的商品名称
            reqdata.Append("<subject>");
            reqdata.Append(productInfo);
            reqdata.Append("</subject>");
            //支付宝合作商户网站唯一订单号
            reqdata.Append("<out_trade_no>");
            reqdata.Append(orderId);
            reqdata.Append("</out_trade_no>");
            //该笔订单的资金总额，单位为RMB-Yuan。取值范围为[0.01，100000000.00]，精确到小数点后两位
            reqdata.Append("<total_fee>");
            reqdata.Append(totalFee);
            reqdata.Append("</total_fee>");
            //卖家的支付宝账号
            reqdata.Append("<seller_account_name>");
            reqdata.Append(_config.Seller_email);
            reqdata.Append("</seller_account_name>");
            //支付成功后的跳转页面链接
            reqdata.Append("<call_back_url>");
            reqdata.Append(returnUrl);
            reqdata.Append("</call_back_url>");
            //支付宝服务器主动通知商户网站里指定的页面http路径
            reqdata.Append("<notify_url>");
            reqdata.Append(notifyUrl);
            reqdata.Append("</notify_url>");
            //买家在商户系统的唯一标识。当该买家支付成功一次后，再次支付金额在30元内时，不需要再次输入密码
            reqdata.Append("<out_user>");
            reqdata.Append(string.Empty);
            reqdata.Append("</out_user>");
            //用户付款中途退出返回商户的地址
            reqdata.Append("<merchant_url>");
            reqdata.Append(string.Empty);
            reqdata.Append("</merchant_url>");
            //交易自动关闭时间，单位为分钟。默认值21600（即15天）agent_id
            reqdata.Append("<pay_expire>");
            reqdata.Append("30");
            reqdata.Append("</pay_expire>");
            //代理人ID
            reqdata.Append("<agent_id>");
            reqdata.Append(string.Empty);
            reqdata.Append("</agent_id>");

            reqdata.Append("</direct_trade_create_req>");
            #endregion
            //**************************
            dicPara.Add("req_data", reqdata.ToString());
            string strToken = Submit.BuildRequest(dicPara, _config);//调用接口取令牌
            Dictionary<string, string> dicResult = new Dictionary<string, string>();
            //Log.Debug("strToken=" + strToken);
            dicResult = Submit.ParseResponse(strToken, _config);
            if (dicResult["request_token"] != null)
            {
                dicPara = new Dictionary<string, string>();
                dicPara.Add("service", _config.GetDataService);//支付接口
                dicPara.Add("format", "xml");//固定参数
                dicPara.Add("v", "2.0");//固定参数
                dicPara.Add("partner", _config.Partner);//合作者ID，支付宝提供
                dicPara.Add("sec_id", _config.Sign_type);//签名方式，暂时使用MD5
                dicPara.Add("_input_charset", "utf-8");//固定参数

                reqdata = new StringBuilder();
                reqdata.Append("<auth_and_execute_req>");
                reqdata.Append("<request_token>");
                reqdata.Append(dicResult["request_token"].ToString());
                reqdata.Append("</request_token>");
                reqdata.Append("</auth_and_execute_req>");

                dicPara.Add("req_data", reqdata.ToString());
                strResult = Submit.BuildRequestUrl(dicPara, _config);//生成支付请求地址
            }
            else if (dicResult["res_error"] != null)
            {
                throw new PluginConfigException("调用支付接口返回异常：" + dicResult["res_error"].ToString());
            }
            return strResult;
        }

        public string ConfirmPayResult()
        {
            return "success";
        }

        public string HelpImage
        {
            get { throw new NotImplementedException(); }
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

        public string PluginListUrl
        {
            set { throw new NotImplementedException(); }
        }

        public PaymentInfo ProcessNotify(System.Web.HttpRequestBase context)
        {
            //Post方式
            NameValueCollection coll = context.Form;
            Dictionary<string, string> paras = new Dictionary<string, string>();
            foreach (string key in coll.AllKeys)
            {
                paras.Add(key, coll[key]);
            }
            Notify notify = new Notify(WorkDirectory);
            PaymentInfo info = new PaymentInfo();
            string notifyid = notify.GetNotifyId(paras);
            bool isSign = notify.Verify(paras, notifyid, (string)coll["sign"], _config, false);
            if (isSign)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(coll["notify_data"]);
                string out_trade_no = xmlDoc.SelectSingleNode("/notify/out_trade_no").InnerText;
                string trade_no = xmlDoc.SelectSingleNode("/notify/trade_no").InnerText;
                string trade_status = xmlDoc.SelectSingleNode("/notify/trade_status").InnerText;
                string notify_time = xmlDoc.SelectSingleNode("/notify/notify_time").InnerText;
                if (trade_status == "TRADE_FINISHED" || trade_status == "TRADE_SUCCESS")
                {
                    info.OrderIds = out_trade_no.Split(',').Select(item => long.Parse(item));
                    info.TradNo = trade_no;
                    info.TradeTime = DateTime.Parse(notify_time);
                    info.ResponseContentWhenFinished = "success";
                }
            }
            return info;
        }

        public PaymentInfo ProcessReturn(System.Web.HttpRequestBase context)
        {
            //Get方式
            NameValueCollection coll = context.QueryString;
            Dictionary<string, string> paras = new Dictionary<string, string>();
            foreach (string key in coll.AllKeys)
            {
                paras.Add(key, coll[key]);
            }
            Notify notify = new Notify(WorkDirectory);
            string notifyid = notify.GetNotifyId(paras);
            bool isSign = notify.Verify(paras, notifyid, (string)coll["sign"], _config);
            PaymentInfo info = new PaymentInfo();
            if (isSign)
            {
                info.OrderIds = coll["out_trade_no"].Split(',').Select(item => long.Parse(item));
                info.TradNo = coll["trade_no"];
            }
            return info;
        }

        public UrlType RequestUrlType
        {
            get { return UrlType.Page; }
        }

        public void SetFormValues(IEnumerable<KeyValuePair<string, string>> values)
        {
            KeyValuePair<string, string> partner = values.FirstOrDefault(item => item.Key == "partner");
            if (string.IsNullOrEmpty(partner.Value))
                throw new PluginConfigException("未设置合作者身份ID!");

            KeyValuePair<string, string> seller_email = values.FirstOrDefault(item => item.Key == "seller_email");
            if (string.IsNullOrEmpty(seller_email.Value))
                throw new PluginConfigException("未设置商户支付宝!");

            //KeyValuePair<string, string> sign_type = values.FirstOrDefault(item => item.Key == "sign_type");
            //if (string.IsNullOrEmpty(sign_type.Value))
            //    throw new PluginConfigException("未设置加密方式!");

            //if (_config == null)
            //{
            //    _config = Utility<Config>.GetConfig(WorkDirectory);
            //}
            //if (sign_type.Value.ToLower() == "md5")
            //{
            KeyValuePair<string, string> key = values.FirstOrDefault(item => item.Key == "key");
            if (string.IsNullOrEmpty(key.Value))
                throw new PluginConfigException("MD5加密方式，未设置交易安全校验码!");
            _config.Key = key.Value;
            //}
            //else if (sign_type.Value.ToLower() == "0001")
            //{
            //    KeyValuePair<string, string> private_key = values.FirstOrDefault(item => item.Key == "private_key");
            //    if (string.IsNullOrEmpty(private_key.Value))
            //        throw new PluginConfigException("(0001)RSA加密方式，未设置商户私钥!");
            //    _config.private_key = private_key.Value;

            //    KeyValuePair<string, string> public_key = values.FirstOrDefault(item => item.Key == "public_key");
            //    if (string.IsNullOrEmpty(public_key.Value))
            //        throw new PluginConfigException("(0001)RSA加密方式，未设置支付宝公钥!");
            //    _config.public_key = public_key.Value;
            //}
            //else
            //{
            //    throw new PluginConfigException("加密方式只能为MD5、0001!");
            //}

            _config.Partner = partner.Value;
            _config.Seller_email = seller_email.Value;
            Utility<Config>.SaveConfig(_config, WorkDirectory);
        }

        public void CheckCanEnable()
        {
            if (_config == null)
            {
                _config = Utility<Config>.GetConfig(WorkDirectory);
            }
            if (string.IsNullOrEmpty(_config.Partner))
            {
                throw new PluginConfigException("未设置合作者身份ID!");
            }
            if (string.IsNullOrEmpty(_config.Seller_email))
            {
                throw new PluginConfigException("未设置商户支付宝!");
            }
            //if (string.IsNullOrEmpty(_config.sign_type))
            //{
            //    throw new PluginConfigException("未设置加解密方式!");
            //}
            //else
            //{
            //    if (_config.sign_type.ToLower() == "md5")
            //    {
            if (string.IsNullOrEmpty(_config.Key))
            {
                throw new PluginConfigException("MD5加密方式，未设置交易安全校验码!");
            }
            //}
            //else
            //{
            //    if (_config.sign_type.ToLower() == "0001")
            //    {
            //        if (string.IsNullOrEmpty(_config.private_key) )
            //        {
            //            throw new PluginConfigException("(0001)RSA加密方式，未设置商户私钥!");
            //        }
            //        if (string.IsNullOrEmpty(_config.public_key))
            //        {
            //            throw new PluginConfigException("(0001)RSA加密方式，未设置支付宝公钥!");
            //        }
            //    }
            //}
            //}
        }
    }
}
