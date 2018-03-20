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

namespace Himall.Plugin.Payment.Alipay_QRCode
{
    public class Service: _ServiceBase, IPaymentPlugin
    {

        static Config _config = null;
        public Service()
            : base()
        {
            
        }
        
        public FormData GetFormData()
        {
            _config = _Config;
            FormData formData = new FormData();
            formData.Items = new FormData.FormItem[] { 
            new FormData.FormItem(){ 
                DisplayName="合作者身份ID"
                , IsRequired=true, Name="partner"
                , Type= FormData.FormItemType.text
                , Value=_config.Partner
            },new FormData.FormItem(){ 
                DisplayName="Key"
                , IsRequired=true, Name="key"
                , Type= FormData.FormItemType.text
                , Value=_config.Key
            }
            };
            return formData;
            
        }

        public string GetRequestUrl(string returnUrl, string notifyUrl, string orderId, decimal totalFee, string productInfo, string openId = null)
        {

            if (string.IsNullOrEmpty(productInfo))
                throw new PluginConfigException("商品信息不能为空!");
            if (string.IsNullOrEmpty(orderId))
                throw new PluginConfigException("订单号不能为空!");
            if (string.IsNullOrWhiteSpace(returnUrl) && string.IsNullOrWhiteSpace(notifyUrl))
                throw new PluginConfigException("返回URL不能为空!");

            if (_config==null)
            {
                _config = Utility<Config>.GetConfig(WorkDirectory);
            }

            string strResult = string.Empty;
            Dictionary<string, string> dicPara = new Dictionary<string, string>();
            dicPara.Add("service", _config.GetCodeService);//取二维码接口
            dicPara.Add("partner", _config.Partner);//合作者ID，支付宝提供
            dicPara.Add("timestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dicPara.Add("sign_type", _config.Sign_type);//签名方式，暂时使用MD5
            dicPara.Add("_input_charset", _config.Input_charset);
            dicPara.Add("method", "add");
            dicPara.Add("biz_type", "10");//业务类型：商品码

            //**************************
            #region 整理请求数据
            StringBuilder reqdata = new StringBuilder();
            reqdata.Append("{");
            reqdata.Append("\"trade_type\":\"" + _config.Trade_Type + "\"");
            reqdata.Append(",\"need_address\":\"F\"");

            reqdata.Append(",\"goods_info\":{");
            reqdata.Append("\"id\":\"" + orderId);
            reqdata.Append("\",\"name\":\"" + productInfo.Substring(0, productInfo.Length > 32 ? 32 : productInfo.Length));
            reqdata.Append("\",\"desc\":\"" + productInfo);
            reqdata.Append("\",\"price\":\"" + totalFee.ToString("F2"));
            reqdata.Append("\"}");

            reqdata.Append(",\"notify_url\":\"" + notifyUrl);
            reqdata.Append(",\"return_url\":\"" + notifyUrl);
            reqdata.Append("\"}");
            
            #endregion 
            //**************************
            dicPara.Add("biz_data", reqdata.ToString());

            string strToken = Submit.BuildRequest(dicPara, _config);//调用接口取令牌
            if (!string.IsNullOrWhiteSpace(strToken))
            {
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.LoadXml(strToken);
                    string is_success = xmlDoc.SelectSingleNode("/alipay/is_success").InnerText;
                    if (is_success == "F")
                    {
                        string error = xmlDoc.SelectSingleNode("/alipay/error").InnerText;
                        throw new PluginConfigException("生成二维码出现异常:" + error);
                    }
                    else if (is_success == "T")
                    {
                        XmlNode node = xmlDoc.SelectSingleNode("/alipay/response/alipay/result_code");
                        if (node != null && node.InnerText == "SUCCESS")
                        {
                            strResult = xmlDoc.SelectSingleNode("/alipay/response/alipay/qrcode").InnerText;
                        }
                        else
                        {
                            string error = xmlDoc.SelectSingleNode("/alipay/response/alipay/error_message").InnerText;
                            throw new PluginConfigException("生成二维码出现异常:" + error);
                        }
                    }
                }
                catch (Exception exp)
                {
                    throw new PluginConfigException("生成二维码出现异常:" + exp.Message);
                }
            }
            return strResult;
        }

        public string ConfirmPayResult()
        {
            return "success";
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
            NameValueCollection coll = context.Form;
            Dictionary<string, string> paras = new Dictionary<string, string>();
            foreach (string key in coll.AllKeys)
            {
                paras.Add(key, coll[key]);
            }
            if (_config == null)
            {
                _config = _Config;
            }
            Notify notify = new Notify(WorkDirectory);
            PaymentInfo info = new PaymentInfo();
            string notifyid = notify.GetNotifyId(paras);
            bool isSign = notify.Verify(paras, notifyid, (string)coll["sign"], _config);
            if (isSign)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(coll["notify_data"]);
                string out_trade_no = xmlDoc.SelectSingleNode("/notify/out_trade_no").InnerText;
                string trade_no = xmlDoc.SelectSingleNode("/notify/trade_no").InnerText;
                string trade_status = xmlDoc.SelectSingleNode("/notify/trade_status").InnerText;
                string notify_time = xmlDoc.SelectSingleNode("/notify/gmt_create").InnerText;
                if (trade_status == "TRADE_FINISHED" || trade_status == "TRADE_SUCCESS" || trade_status == "WAIT_SELLER_SEND_GOODS")
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
            foreach(string key in coll.AllKeys)
            {
                paras.Add(key, coll[key]);
            }
            if (_config == null)
            {
                _config = _Config;
            }
            Notify notify = new Notify(WorkDirectory);
            string notifyid = notify.GetNotifyId(paras);
            bool isSign = notify.Verify(paras, notifyid, (string)coll["sign"], _config);//验证签名
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
            get { return UrlType.QRCode; }
        }

        public void SetFormValues(IEnumerable<KeyValuePair<string, string>> values)
        {
            KeyValuePair<string, string> partner = values.FirstOrDefault(item => item.Key == "partner");
            if (string.IsNullOrEmpty(partner.Value))
                throw new PluginConfigException("未设置合作者身份ID!");

            KeyValuePair<string, string> key = values.FirstOrDefault(item => item.Key == "key");
            if (string.IsNullOrEmpty(key.Value))
                throw new PluginConfigException("未设置Key!");
            _config.Partner = partner.Value;
            _config.Key = key.Value;
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
            if (string.IsNullOrEmpty(_config.Key))
            {
                throw new PluginConfigException("未设置Key!");
            }
        }
    }
}
