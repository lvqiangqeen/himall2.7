using Himall.Core;
using Himall.Core.Plugins;
using Himall.Core.Plugins.Payment;
using Himall.PaymentPlugin;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Himall.WeixinPaymentBase
{
    public class ServiceBase : PaymentBase<Config>
    {
        private const string REFUND_SOURCE_RECHARGE_FUNDS = "REFUND_SOURCE_RECHARGE_FUNDS";
        public string ConfirmPayResult()
        {
            return "SUCCESS";
        }

        public FormData GetFormData()
        {
            Config config = Utility<Config>.GetConfig(WorkDirectory);

            var formData = new FormData()
            {
                Items = new FormData.FormItem[] { 
                    //AppId
                   new  FormData.FormItem(){
                     DisplayName = "AppId",
                     Name = "AppId",
                     IsRequired = true,
                      Type= FormData.FormItemType.text,
                      Value=config.AppId
                   },
                   //AppSecret
                   new  FormData.FormItem(){
                     DisplayName = "AppSecret",
                     Name = "AppSecret",
                     IsRequired = true,
                      Type= FormData.FormItemType.text,
                      Value=config.AppSecret
                   },
                   //MCHID
                   new  FormData.FormItem(){
                     DisplayName = "Key",
                     Name = "Key",
                     IsRequired = true,
                      Type= FormData.FormItemType.text,
                      Value=config.Key
                   },
                  //AppId
                   new  FormData.FormItem(){
                     DisplayName = "MCHID",
                     Name = "MCHID",
                     IsRequired = true,
                      Type= FormData.FormItemType.text,
                      Value=config.MCHID
                   },//安全证书
                   new  FormData.FormItem(){
                     DisplayName = "商户证书",
                     Name = "pkcs12",
                     IsRequired = true,
                      Type= FormData.FormItemType.text,
                      Value=config.pkcs12
                   }
                }
            };
            return formData;
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
            set { throw new System.NotImplementedException(); }
        }

        public PaymentInfo ProcessNotify(HttpRequestBase request)
        {

            ResponseHandler resHandler = new ResponseHandler(request);
            resHandler.init();
            Config Config = Utility<Config>.GetConfig(WorkDirectory);
            resHandler.setKey(Config.Key, Config.AppId);
            bool isSign = resHandler.isTenpaySign();
            if (!isSign)
            {
                throw new PluginException("微信回调验证签名失败！");
            }
            string out_trade_no = resHandler.getParameter("out_trade_no");
            string time_end = resHandler.getParameter("time_end");
            string transaction_id = resHandler.getParameter("transaction_id");

            return new PaymentInfo()
            {
                OrderIds = out_trade_no.Split(',').Select(item => long.Parse(item)),
                ResponseContentWhenFinished = "success",
                TradeTime = DateTime.ParseExact(time_end, "yyyyMMddHHmmss", null),
                TradNo = transaction_id
            };
        }

        public PaymentInfo ProcessReturn(HttpRequestBase request)
        {
            return null;
        }

        public void SetFormValues(IEnumerable<KeyValuePair<string, string>> values)
        {
            var appIdItem = values.FirstOrDefault(item => item.Key == "AppId");
            if (string.IsNullOrWhiteSpace(appIdItem.Value))
                throw new PluginConfigException("合作者身份AppId不能为空");

            var appSecretItem = values.FirstOrDefault(item => item.Key == "AppSecret");
            if (string.IsNullOrWhiteSpace(appSecretItem.Value))
                throw new PluginConfigException("AppSecret不能为空");

            var keyItem = values.FirstOrDefault(item => item.Key == "Key");
            if (string.IsNullOrWhiteSpace(keyItem.Value))
                throw new PluginConfigException("Key不能为空");

            var MCHIDItem = values.FirstOrDefault(item => item.Key == "MCHID");
            if (string.IsNullOrWhiteSpace(MCHIDItem.Value))
                throw new PluginConfigException("MCHID不能为空");
            var pkcs12 = values.FirstOrDefault(item => item.Key == "pkcs12");
            if (string.IsNullOrWhiteSpace(pkcs12.Value))
                throw new PluginConfigException("商户证书不能为空");


            Config oldConfig = Utility<Config>.GetConfig(WorkDirectory);
            oldConfig.AppId = appIdItem.Value;
            oldConfig.Key = keyItem.Value;
            oldConfig.AppSecret = appSecretItem.Value;
            oldConfig.MCHID = MCHIDItem.Value;
            oldConfig.pkcs12 = pkcs12.Value;
            Utility<Config>.SaveConfig(oldConfig, WorkDirectory);
        }

        public void CheckCanEnable()
        {
            Config config = Utility<Config>.GetConfig(WorkDirectory);
            if (string.IsNullOrWhiteSpace(config.AppId))
                throw new PluginConfigException("未设置AppId");
            if (string.IsNullOrWhiteSpace(config.AppSecret))
                throw new PluginConfigException("未设置AppSecret");
            if (string.IsNullOrWhiteSpace(config.Key))
                throw new PluginConfigException("未设置Key");
            if (string.IsNullOrWhiteSpace(config.MCHID))
                throw new PluginConfigException("未设置MCHID");

            if (string.IsNullOrWhiteSpace(config.pkcs12))
                throw new PluginConfigException("未设置商户证书");
        }

        public override RefundFeeReturnModel ProcessRefundFee(PaymentPara para)
        {
            //创建请求对象
            RequestHandler reqHandler = new RequestHandler();

            RefundFeeReturnModel paymentInfo = new RefundFeeReturnModel();
            paymentInfo.RefundMode = RefundRunMode.Sync;
            paymentInfo.RefundResult = RefundState.Failure;
            string strResult = string.Empty;
            Config payConfig = Utility<Config>.GetConfig(WorkDirectory);
            if (string.IsNullOrEmpty(payConfig.AppId))
                throw new PluginException("未设置AppId");
            if (string.IsNullOrEmpty(payConfig.MCHID))
                throw new PluginException("未设置MCHID");
            if (string.IsNullOrWhiteSpace(payConfig.pkcs12))
                throw new PluginConfigException("未设置商户证书");

            //-----------------------------
            //设置请求参数
            //-----------------------------
            reqHandler.SetKey(payConfig.Key);
            var nonceStr = TenPayUtil.GetNoncestr();
            reqHandler.SetParameter("out_trade_no", para.out_trade_no);
            reqHandler.SetParameter("out_refund_no", para.out_refund_no);
            reqHandler.SetParameter("total_fee",Convert.ToInt32((para.total_fee * 100)).ToString());
            reqHandler.SetParameter("refund_fee",Convert.ToInt32((para.refund_fee * 100)).ToString());
            reqHandler.SetParameter("op_user_id", payConfig.MCHID);
            reqHandler.SetParameter("appid", payConfig.AppId);
            reqHandler.SetParameter("mch_id", payConfig.MCHID);
            reqHandler.SetParameter("nonce_str", nonceStr);
            //从哪个帐号退款（余额、未结算帐号）
            reqHandler.SetParameter("refund_account", REFUND_SOURCE_RECHARGE_FUNDS);//REFUND_SOURCE_RECHARGE_FUNDS---可用余额退款/基本账户

            string sign = reqHandler.CreateMd5Sign("key", payConfig.Key);//按约定规则生成MD5，规则参考接口文档
            reqHandler.SetParameter("sign", sign);

            var pkcs12 = WorkDirectory + "\\" + payConfig.pkcs12;
            if (!System.IO.File.Exists(pkcs12))
                throw new PluginException("未找到商户证书文件");

            string strXml = reqHandler.ParseXML();
            string result = string.Empty;
            try
            {
                result = TenPayV3.Refund(strXml, pkcs12, payConfig.MCHID);//调用统一接口
            }
            catch(Exception ex)
            {
                throw new PluginException("原路返回退款时出错：" + ex.Message);
            }
            XDocument xmlDocument = XDocument.Parse(result);
            if (xmlDocument == null)
                throw new PluginException("原路返回退款时出错：" + strXml);

            XElement e_return = xmlDocument.Element("xml").Element("return_code");
            XElement e_result = xmlDocument.Element("xml").Element("return_msg");
            if (e_return == null)
                throw new PluginException("原路返回退款时,返回参数异常");

            //处理返回时先判断协议错误码，再业务，最后交易状态 
            if (e_return.Value == "SUCCESS")
            {
                e_result = xmlDocument.Element("xml").Element("result_code");
                XElement e_errdes = xmlDocument.Element("xml").Element("err_code_des");
                if (e_result.Value == "SUCCESS")
                {
                    //微信退款单号
                    string refund_id = xmlDocument.Element("xml").Element("refund_id").Value;
                    //商户退款单号
                    string out_refund_no = xmlDocument.Element("xml").Element("out_refund_no").Value;

                    //业务处理
                    paymentInfo.RefundResult = RefundState.Success;
                    paymentInfo.OrderId = long.Parse(out_refund_no);
                    paymentInfo.RefundNo = refund_id;
                    paymentInfo.RefundTime = DateTime.Now;
                }
                else
                {
                    throw new PluginException("原路返回退款时,接口返回异常:" + e_errdes.Value);
                }
            }
            else
            {
                throw new PluginException("原路返回退款时,接口返回异常:" + e_result.Value);
            }
            return paymentInfo;
        }

        public override PaymentInfo EnterprisePay(EnterprisePayPara para)
        {
            //创建请求对象
            RequestHandler reqHandler = new RequestHandler();
            PaymentInfo paymentInfo = new PaymentInfo();
            string strResult = string.Empty;
            Config payConfig = Utility<Config>.GetConfig(WorkDirectory);
            if (string.IsNullOrEmpty(payConfig.AppId))
                throw new PluginException("未设置AppId");
            if (string.IsNullOrEmpty(payConfig.MCHID))
                throw new PluginException("未设置MCHID");
            if (string.IsNullOrWhiteSpace(payConfig.pkcs12))
                throw new PluginConfigException("未设置商户证书");

            //-----------------------------
            //设置请求参数
            //-----------------------------

            reqHandler.SetKey(payConfig.Key);
            var nonceStr = TenPayUtil.GetNoncestr();
            reqHandler.SetParameter("partner_trade_no", para.out_trade_no);
            reqHandler.SetParameter("amount", Convert.ToInt32((para.amount * 100)).ToString());
            reqHandler.SetParameter("openid", para.openid);
            reqHandler.SetParameter("mch_appid", payConfig.AppId);
            reqHandler.SetParameter("mchid", payConfig.MCHID);
            reqHandler.SetParameter("nonce_str", nonceStr);
            string checkNameOption = "NO_CHECK";
            if (para.check_name)
            {//是否检测真实姓名
                checkNameOption = "OPTION_CHECK";
            }
            reqHandler.SetParameter("check_name", checkNameOption);
            reqHandler.SetParameter("desc", para.desc);
            reqHandler.SetParameter("spbill_create_ip", string.IsNullOrWhiteSpace(para.spbill_create_ip) ? "222.240.184.122" : para.spbill_create_ip);

            string sign = reqHandler.CreateMd5Sign("key", payConfig.Key);//按约定规则生成MD5，规则参考接口文档
            reqHandler.SetParameter("sign", sign);

            var pkcs12 = WorkDirectory + "\\" + payConfig.pkcs12;
            if (!System.IO.File.Exists(pkcs12))
                throw new PluginException("未找到商户证书文件");

            string strXml = reqHandler.ParseXML();
            string result = string.Empty;
            try
            {
                result = TenPayV3.transfers(strXml, pkcs12, payConfig.MCHID);//调用统一接口
            }
            catch (Exception ex)
            {
                throw new PluginException("企业付款时出错：" + ex.Message);
            }
            XDocument xmlDocument = XDocument.Parse(result);
            if (xmlDocument == null)
                throw new PluginException("企业付款时出错：" + strXml);
            
            XElement e_return = xmlDocument.Element("xml").Element("return_code");
            XElement e_result = xmlDocument.Element("xml").Element("return_msg");
            if (e_return == null)
                throw new PluginException("企业付款时,返回参数异常");
            //处理返回时先判断协议错误码，再业务，最后交易状态 
            if (e_return.Value == "SUCCESS")
            {
                e_result = xmlDocument.Element("xml").Element("result_code");
                XElement e_errdes = xmlDocument.Element("xml").Element("err_code_des");
                if (e_result.Value == "SUCCESS")
                {
                    //微信单号
                    string payment_no = xmlDocument.Element("xml").Element("payment_no").Value;
                    //商户单号
                    string partner_trade_no = xmlDocument.Element("xml").Element("partner_trade_no").Value;
                    string payment_time = xmlDocument.Element("xml").Element("payment_time").Value;

                    //业务处理
                    paymentInfo.OrderIds = new List<long> { long.Parse(partner_trade_no) };
                    paymentInfo.TradNo = payment_no;
                    paymentInfo.TradeTime = DateTime.Parse(payment_time);
                }
                else
                {
                    throw new PluginException("企业付款时,接口返回异常:" + e_errdes.Value);
                }
            }
            else
            {
                throw new PluginException("企业付款时,接口返回异常:" + e_result.Value);
            }
            return paymentInfo;
        }
    }

}
