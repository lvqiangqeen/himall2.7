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
using System.Web;

namespace Himall.Plugin.Payment.Alipay_App
{
    public class Service : _ServiceBase, IPaymentPlugin
    {


        public Service()
            : base()
        {

        }
        public FormData GetFormData()
        {
            FormData formData = new FormData();
            formData.Items = new FormData.FormItem[] { 
            new FormData.FormItem(){ 
                DisplayName="合作者身份ID"
                , IsRequired=true, Name="partner"
                , Type= FormData.FormItemType.text
                , Value=_Config.Partner
            }
            ,new FormData.FormItem(){
                DisplayName="商户支付宝帐号"
                , IsRequired=true
                , Name="seller_email"
                , Type= FormData.FormItemType.text
                , Value=_Config.Seller_email
            }

            ,new FormData.FormItem(){
                DisplayName="private key"
                , IsRequired=true, Name="Private_key"
                , Type= FormData.FormItemType.text
                , Value=_Config.Private_key
            },new FormData.FormItem(){
                DisplayName="public key"
                , IsRequired=true, Name="Public_key"
                , Type= FormData.FormItemType.text
                , Value=_Config.Public_key
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
            if (string.IsNullOrEmpty(returnUrl))
                throw new PluginConfigException("返回URL不能为空!");

            
            Dictionary<string, string> dicPara = new Dictionary<string, string>();
            dicPara.Add("service", _Config.GetDataService);//取令牌接口
            dicPara.Add("partner", _Config.Partner);//合作者ID，支付宝提供
            dicPara.Add("_input_charset", _Config.Input_charset);
            dicPara.Add("out_trade_no", orderId);//固定参数
            dicPara.Add("subject", productInfo.Length>128?productInfo.Substring(0,128):productInfo) ;//固定参数
            dicPara.Add("payment_type", "1");//固定参数
            dicPara.Add("seller_id", _Config.Seller_email);//固定参数
            dicPara.Add("total_fee", totalFee.ToString());//固定参数
            dicPara.Add("body", productInfo);//固定参数
            dicPara.Add("it_b_pay", "30m");//固定参数
            dicPara.Add("notify_url", notifyUrl);//固定参数

            var paraStr = Submit.BuildRequestParaToString(dicPara, Encoding.GetEncoding(_Config.Input_charset), _Config);

            return paraStr;
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
            string notifyid = context.Form["notify_id"];//获取notify_id
            string sign = context.Form["sign"];//获取sign
            var signStr = "";
            //foreach(var para in paras)
            //{
            //    signStr += "\r\n" + para.Key + ":" + para.Value+ ";\r\n";
            //}

            //Core.Log.Debug("notifyid:" + notifyid+"，签名方式:"+_Config.Sign_type+"\r\n参数:"+signStr+ ";\r\n");

            bool isSign = notify.Verify(paras, notifyid, sign, _Config);
            if (isSign)
            {
                /*
                 discount=0.00,
                 * payment_type=1,
                 * subject=AMII及简2015秋新品A型大摆圆领撞色开衫大码羊毛衣女,
                 * trade_no=2015103000001000360068771405,
                 * buyer_email=18684719574,
                 * gmt_create=2015-10-30 10:19:57,
                 * notify_type=trade_status_sync,
                 * quantity=1,
                 * out_trade_no=20151030061392932,
                 * seller_id=2088701375176746,
                 * notify_time=2015-10-30 10:19:58,
                 * body=AMII及简2015秋新品A型大摆圆领撞色开衫大码羊毛衣女,
                 * trade_status=TRADE_SUCCESS,
                 * is_total_fee_adjust=N,
                 * total_fee=0.01,
                 * gmt_payment=2015-10-30 10:19:58,
                 * seller_email=hishop@live.cn,
                 * price=0.01,
                 * buyer_id=2088702728193364,
                 * notify_id=1923f07d295db2519efa956c186da67940,
                 * use_coupon=N,
                 * sign_type=RSA,
                 * sign=RZj/aJ+6TKyt4ozlVrM32M/96ybsPLMz+9CynkO6CI8sD2c6AvOf04p52yzu0TObxoDbF53anT6GWwp+jfsbAzX6ZBJp1vwArFO8gjUMbeY82BktRu5Zn5EgXSczeO5Yv9R92D8cRaCoJ8kk/tBIssdUhqxo/IKYrWRm1qtOj0A=
                 */
                string out_trade_no =coll["out_trade_no"];
                string trade_status =coll["trade_status"] ;
                string notify_time = coll["notify_time"] ;
                string TradNo = coll["trade_no"];
                if (trade_status == "TRADE_FINISHED" || trade_status == "TRADE_SUCCESS")
                {
                    info.OrderIds = out_trade_no.Split(',').Select(item => long.Parse(item));
                    info.TradeTime = DateTime.Parse(notify_time);
                    info.TradNo = TradNo;
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
            bool isSign = notify.Verify(paras, string.Empty, (string)coll["sign"], _Config);
            PaymentInfo info = new PaymentInfo();
            if (isSign)
            {
                info.OrderIds = coll["out_trade_no"].Split(',').Select(item => long.Parse(item));
                info.TradNo = coll["trade_no"];
            }
            return info;
        }


        public void SetFormValues(IEnumerable<KeyValuePair<string, string>> values)
        {
            KeyValuePair<string, string> partner = values.FirstOrDefault(item => item.Key == "partner");
            if (string.IsNullOrEmpty(partner.Value))
                throw new PluginConfigException("未设置合作者身份ID!");

            KeyValuePair<string, string> seller_email = values.FirstOrDefault(item => item.Key == "seller_email");
            if (string.IsNullOrEmpty(seller_email.Value))
                throw new PluginConfigException("未设置商户支付宝!");

            KeyValuePair<string, string> key = values.FirstOrDefault(item => item.Key == "Private_key");
            if (string.IsNullOrEmpty(key.Value))
                throw new PluginConfigException("RSA加密方式，未设置Private_key!");

            KeyValuePair<string, string> pubkey = values.FirstOrDefault(item => item.Key == "Public_key");
            if (string.IsNullOrEmpty(pubkey.Value))
                throw new PluginConfigException("RSA加密方式，未设置Public_key!");

            _Config.Private_key = key.Value;
            _Config.Public_key = pubkey.Value;
            _Config.Partner = partner.Value;
            _Config.Seller_email = seller_email.Value;
            Utility<Config>.SaveConfig(_Config, WorkDirectory);
        }

        public void CheckCanEnable()
        {
            if (string.IsNullOrEmpty(_Config.Partner))
            {
                throw new PluginConfigException("未设置合作者身份ID!");
            }
            if (string.IsNullOrEmpty(_Config.Seller_email))
            {
                throw new PluginConfigException("未设置商户支付宝!");
            }
            if (string.IsNullOrEmpty(_Config.Public_key))
            {
                throw new PluginConfigException("未设置Public_key!");
            }
            if (string.IsNullOrEmpty(_Config.Private_key))
            {
                throw new PluginConfigException("未设置Private_key!");
            }
        }
    }
}
