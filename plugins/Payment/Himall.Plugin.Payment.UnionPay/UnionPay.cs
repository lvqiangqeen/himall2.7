using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core.Plugins.Payment;
using Himall.PaymentPlugin;
using Himall.Core.Plugins;
using Himall.Plugin.Payment.UnionPay.Util;
using System.Web;

namespace Himall.Plugin.Payment.UnionPay
{
    /// <summary>
    /// 网关跳转支付（前台交易）
    /// </summary>
    public class UnionPay:PaymentBase<Util.ChinaUnionConfig>, IPaymentPlugin
    {
        ChinaUnionConfig CUConfig;
        public string ConfirmPayResult()
        {
            return "success";
        }

        public Core.Plugins.FormData GetFormData()
        {
            if (CUConfig == null)
                CUConfig = Utility<ChinaUnionConfig>.GetConfig(WorkDirectory);
            return new  FormData(){
                Items=new FormData.FormItem[]{
                    new FormData.FormItem(){
                       DisplayName = "商户代码",
                       Name = "merId",
                       IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.text,
                       Value=CUConfig.merId
                    },
                    new FormData.FormItem(){
                       DisplayName = "商户私钥证书文件名",
                       Name = "signCertpath",
                       IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.text,
                       Value=CUConfig.signCertpath
                    },
                    new FormData.FormItem(){
                       DisplayName = "商户私钥证书密码",
                       Name = "signCertpwd",
                       IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.text,
                       Value=CUConfig.signCertpwd
                    }
                    ,
                    new FormData.FormItem(){
                       DisplayName = "银联公钥证书文件名",
                       Name = "encryptCertpath",
                       IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.text,
                       Value=CUConfig.encryptCertpath
                    }
                }
            };
        }

        public string GetRequestUrl(string returnUrl, string notifyUrl, string orderId, decimal totalFee, string productInfo, string openId = null)
        {
            string resultUrl = string.Empty;
            if (CUConfig == null)
                CUConfig = Utility<ChinaUnionConfig>.GetConfig(WorkDirectory);
            if (string.IsNullOrEmpty(CUConfig.merId))
                throw new PluginException("未设置商户编码！");
            if (string.IsNullOrEmpty(CUConfig.signCertpath))
                throw new PluginException("未设置商户私钥证书！");

            Dictionary<string, string> dictParams = new Dictionary<string, string>();
            dictParams.Add("merId", CUConfig.merId);
            dictParams.Add("version", CUConfig.version);
            dictParams.Add("encoding", CUConfig.encoding);
            dictParams.Add("signMethod", CUConfig.signMethod);
            dictParams.Add("txnType", CUConfig.txnType);
            dictParams.Add("txnSubType", CUConfig.txnSubType);
            dictParams.Add("bizType", CUConfig.bizType);
            dictParams.Add("channelType", CUConfig.channelType);
            dictParams.Add("accessType", CUConfig.accessType);
            dictParams.Add("currencyCode", CUConfig.currencyCode);
            dictParams.Add("frontUrl", returnUrl);
            dictParams.Add("backUrl", notifyUrl);
            dictParams.Add("orderId", orderId);
            dictParams.Add("txnTime", DateTime.Now.ToString("yyyyMMddHHmmss"));
            dictParams.Add("txnAmt", ((int)(totalFee * 100)).ToString());
            string signCertpath = WorkDirectory + "\\Cert\\" + CUConfig.signCertpath;
            try
            {
                //string signCertpath = WorkDirectory + "\\Cert\\" + CUConfig.signCertpath;
                bool isSign = SignUtil.Sign(dictParams, System.Text.ASCIIEncoding.GetEncoding(CUConfig.encoding), signCertpath, CUConfig.signCertpwd);
                if (!isSign)
                    throw new PluginException("签名失败！");
            }
            catch (Exception ex)
            {
                throw new PluginException(ex.Message);
            }
            resultUrl = CUConfig.frontTransUrl + "?" + SignUtil.CoverDictionaryToString(dictParams);
            return resultUrl;
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
                    _logo = Utility<ChinaUnionConfig>.GetConfig(WorkDirectory).Logo;
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
        
        public UrlType RequestUrlType
        {
            get { return UrlType.FormPost; }
        }

        public void SetFormValues(IEnumerable<KeyValuePair<string, string>> values)
        {
            ChinaUnionConfig oldConfig = Utility<ChinaUnionConfig>.GetConfig(WorkDirectory);
            var merIdItem = values.FirstOrDefault(item => item.Key == "merId");
            if (string.IsNullOrWhiteSpace(merIdItem.Value))
                throw new PluginException("商户ID不能为空");

            oldConfig.merId = merIdItem.Value;

            var Item = values.FirstOrDefault(item => item.Key == "signCertpath");
            if (string.IsNullOrWhiteSpace(Item.Value))
                throw new PluginException("商户私钥证书文件名不能为空");
            oldConfig.signCertpath = Item.Value;

            Item = values.FirstOrDefault(item => item.Key == "signCertpwd");
            if (string.IsNullOrWhiteSpace(Item.Value))
                throw new PluginException("商户私钥证书密码不能为空");
            oldConfig.signCertpwd = Item.Value;

            Item = values.FirstOrDefault(item => item.Key == "encryptCertpath");
            if (string.IsNullOrWhiteSpace(Item.Value))
                throw new PluginException("银联公钥证书文件名不能为空");
            oldConfig.encryptCertpath = Item.Value;
            Utility<ChinaUnionConfig>.SaveConfig(oldConfig, WorkDirectory);
        }

        public void CheckCanEnable()
        {
            ChinaUnionConfig oldConfig = Utility<ChinaUnionConfig>.GetConfig(WorkDirectory);
            if (string.IsNullOrWhiteSpace(oldConfig.merId))
                throw new PluginException("商户ID不能为空");

            if (string.IsNullOrWhiteSpace(oldConfig.signCertpath))
                throw new PluginException("商户私钥证书文件名不能为空");

            if (string.IsNullOrWhiteSpace(oldConfig.signCertpwd))
                throw new PluginException("商户私钥证书密码不能为空");

            if (string.IsNullOrWhiteSpace(oldConfig.encryptCertpath))
                throw new PluginException("银联公钥证书文件名不能为空");
        }

        public PaymentInfo ProcessNotify(System.Web.HttpRequestBase context)
        {
            Dictionary<string, string> dicReturn = new Dictionary<string, string>();
            context.Form.AllKeys.ToList().ForEach(item =>
            {
                dicReturn.Add(item.ToString(), context.Form[item.ToString()]);
            });
            CUConfig = Utility<ChinaUnionConfig>.GetConfig(WorkDirectory);
            string validateCertdir = WorkDirectory + "\\Cert\\";
            try
            {

                bool isValidate = SignUtil.Validate(dicReturn, System.Text.ASCIIEncoding.GetEncoding(CUConfig.encoding), validateCertdir);
                if (!isValidate)
                    throw new PluginException("验签失败！");

                PaymentInfo payInfo = new PaymentInfo();
                if (context.Form.AllKeys.Contains("orderId"))
                {
                    var orderids = context.Form["orderId"].ToString().Replace("/", ",");
                    payInfo.OrderIds = orderids.Split(',').Select(item => long.Parse(item));
                }
                if (context.Form.AllKeys.Contains("queryId"))
                    payInfo.TradNo = context.Form["queryId"].ToString();
                payInfo.TradeTime = DateTime.Now;
                payInfo.ResponseContentWhenFinished = string.Empty;
                return payInfo;
            }
            catch (Exception ex)
            {
                throw new PluginException("后台通知验签异常：" + ex.Message);
            }
        }

        public PaymentInfo ProcessReturn(System.Web.HttpRequestBase context)
        {
            Dictionary<string, string> dicReturn = new Dictionary<string, string>();
            context.Form.AllKeys.ToList().ForEach(item =>
            {
                dicReturn.Add(item.ToString(), context.Form[item.ToString()]);
            });
            CUConfig = Utility<ChinaUnionConfig>.GetConfig(WorkDirectory);
            string validateCertdir = WorkDirectory + "\\Cert\\";
            try
            {

                bool isValidate = SignUtil.Validate(dicReturn, System.Text.ASCIIEncoding.GetEncoding(CUConfig.encoding), validateCertdir);
                if (!isValidate)
                    throw new PluginException("验签失败！");

                PaymentInfo payInfo = new PaymentInfo();
                if (context.Form.AllKeys.Contains("orderId"))
                {
                    var orderids = context.Form["orderId"].ToString().Replace("/", ",");
                    payInfo.OrderIds = orderids.Split(',').Select(item => long.Parse(item));
                }
                if (context.Form.AllKeys.Contains("queryId"))
                    payInfo.TradNo = context.Form["queryId"].ToString();
                payInfo.TradeTime = DateTime.Now;
                payInfo.ResponseContentWhenFinished = string.Empty;
                return payInfo;
            }
            catch (Exception ex)
            {
                throw new PluginException("后台通知验签异常：" + ex.Message);
            }
        }
    }
}
